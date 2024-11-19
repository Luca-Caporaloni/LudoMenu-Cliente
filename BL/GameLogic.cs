using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace BL
{
    public class GameLogic
    {
        private List<Player> players = new List<Player>(); // Cambiar de Dictionary a List si necesario
        private int currentPlayerIndex;

        public int CurrentPlayerIndex => currentPlayerIndex;


        private Dice dice;
        private string connectionString;

        private string[] availableColors = { "Red", "Green", "Blue", "Yellow" };

        private int currentGameId; // Declara la variable globalmente

        private int currentTurn;

        public GameLogic()
        {
            connectionString = "Data Source=localhost;Initial Catalog=LudoDB;Integrated Security=True;Encrypt=False;TrustServerCertificate=True";  // Asegúrate de usar una cadena de conexión válida
            players = new List<Player>();
            currentPlayerIndex = 0;
            dice = new Dice();
        }

        public IReadOnlyList<Player> Players => players.AsReadOnly(); // Propiedad pública para obtener jugadores

        public int GameId { get; set; }

        public void AddPlayer(string name, string color)
        {
            if (players.Count >= 4)
                throw new InvalidOperationException("El juego solo permite 4 jugadores.");

            players.Add(new Player(name, color));

        }

        public void StartGame(int gameId) // O alguna otra función que reciba el gameId
        {
            currentGameId = gameId; // Asignación del valor al iniciar el juego

            if (players.Count < 4)
            {
                // Deshabilitar turnos para jugadores 3 y 4 si no están conectados
                for (int i = players.Count; i < 4; i++)
                {
                    players[i].SkipTurn = true; // O una lógica similar para desactivar sus turnos
                }
            }

        }

        public int GetCurrentPlayer(int gameId)
        {
            return currentTurn;
        }

        public Player GetCurrentPlayer()
        {
            if (currentPlayerIndex >= 0 && currentPlayerIndex < players.Count)
            {
                return players[currentPlayerIndex];
            }
            else
            {
                throw new InvalidOperationException("Índice de jugador fuera de rango.");
            }
        }

        public int RollDice()
        {
            return dice.Roll();
        }

        public bool MoveToken(Player player, Token token, int diceRoll)
        {

            if (token == null)
            {
                throw new ArgumentNullException(nameof(token), "El token no puede ser null.");
            }

            if (token.IsInHome)
            {
                Console.WriteLine("La ficha está en casa y no puede moverse.");

                // Sacar una ficha de la casa si se sacó un 6
                if (diceRoll == 6)
                {
                    token.Position = 1; // Ubica la ficha en la casilla 1
                    token.IsInHome = false; // La ficha ya no está en la casa
                    UpdatePlayerPositionInDatabase(player, token); // Actualiza la base de datos
                    return true; // Movimiento exitoso
                }
                else
                {
                    return false; // No se puede mover una ficha en casa sin un 6
                }
            }
            else
            {
                // Mueve la ficha si no está en la casa
                token.Position += diceRoll;

                // Si la ficha llega a la casilla 100 o más, esa ficha ha ganado
                if (token.Position >= 100)
                {
                    token.Position = 100; // Ficha llega al final
                    token.IsInHome = true; // Se marca como "fuera" del juego
                }

                UpdatePlayerPositionInDatabase(player, token); // Actualiza la base de datos
                return true; // Movimiento exitoso
            }
        }

        private void UpdatePlayerPositionInDatabase(Player player, Token token)
        {
            string query = "UPDATE PlayerGame SET CurrentPos = @CurrentPos WHERE PlayerID = @PlayerID";

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@CurrentPos", token.Position);
                command.Parameters.AddWithValue("@PlayerID", player.Id); // Asumiendo que 'Id' es la propiedad del jugador

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void SetCurrentPlayer(string playerName)
        {
            var player = players.FirstOrDefault(p => p.Name == playerName);
            if (player != null)
            {
                currentPlayerIndex = players.IndexOf(player);
            }
            else
            {
                throw new Exception($"El jugador con el nombre '{playerName}' no existe.");
            }
        }



        public string MoveCurrentPlayer(int diceRoll)
        {
            Player currentPlayer = GetCurrentPlayer();

            // Si el jugador sacó un 6 y tiene fichas en casa, sacar una ficha
            if (diceRoll == 6 && currentPlayer.HasTokensInHome())
            {
                currentPlayer.ReleaseTokenFromHome();
                return $"{currentPlayer.Name} sacó una ficha de la casa.";
            }

            // Intentar mover una ficha en el tablero
            Token tokenToMove = currentPlayer.GetTokenToMove(diceRoll);
            if (tokenToMove == null)
            {
                if (diceRoll != 6)
                {
                    return $"{currentPlayer.Name} no puede mover ninguna ficha.";
                }
                else
                {
                    return $"{currentPlayer.Name} sacó un 6, pero no tiene fichas para mover.";
                }
            }

            int newPosition = tokenToMove.Position + diceRoll;

            // Si la ficha llega o supera la casilla 100
            if (newPosition >= 100)
            {
                tokenToMove.Position = 100;
                tokenToMove.IsActive = false;

                if (currentPlayer.HasWon())
                {
                    return $"{currentPlayer.Name} ha ganado el juego.";
                }
            }
            else
            {
                // Verificar captura de fichas enemigas
                foreach (var opponent in players)
                {
                    if (opponent != currentPlayer && opponent.HasTokenAtPosition(newPosition))
                    {
                        opponent.SendTokenToHome(newPosition);
                        break;
                    }
                }

                tokenToMove.Position = newPosition;
            }

            return $"{currentPlayer.Name} movió una ficha a la posición {newPosition}.";
        }


        public void NextTurn()
        {
            do
            {
                currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
            } while (players[currentPlayerIndex].SkipTurn); // Saltar turnos si el jugador no está activo

            UpdateCurrentTurnInDatabase();
        }


        private void UpdateCurrentTurnInDatabase()
        {
            string query = "UPDATE Games SET CurrentTurn = @CurrentTurn WHERE GameID = @GameID";

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@CurrentTurn", currentPlayerIndex); // El índice del jugador actual
                command.Parameters.AddWithValue("@GameID", currentGameId); // El ID de la partida

                connection.Open();
                command.ExecuteNonQuery();
            }
        }


        public bool IsGameOver()
        {
            if (players.Exists(player => player.HasWon()))
            {
                return true;
            }
            return false;
        }

        public void SkipNextTurn(Player player)
        {
            player.SkipTurn = true;
        }

        private bool ClearSkipTurnFlag(Player player)
        {
            // Limpia el indicador de "saltar turno" después de aplicarlo
            player.SkipTurn = false;
            return true;
        }


        public void ResetGame()
        {
            foreach (var player in Players)
            {
                foreach (var token in player.Tokens)
                {
                    token.ReturnToHome();
                }
            }

            currentPlayerIndex = 0; // Reiniciar al primer jugador
        }



    }
}
