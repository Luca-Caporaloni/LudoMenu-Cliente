using System;
using System.Collections.Generic;

namespace BL
{
    public class GameLogic
    {
        private List<Player> players = new List<Player>(); // Cambiar de Dictionary a List si necesario
        private int currentPlayerIndex;
        private Dice dice;

        public GameLogic()
        {
            players = new List<Player>();
            currentPlayerIndex = 0;
            dice = new Dice();
        }

        public IReadOnlyList<Player> Players => players.AsReadOnly(); // Propiedad pública para obtener jugadores

        public void AddPlayer(string name, string color)
        {
            if (players.Count >= 4)
                throw new InvalidOperationException("El juego solo permite 4 jugadores.");

            players.Add(new Player(name, color));
        }

        public Player GetCurrentPlayer()
        {
            return players[currentPlayerIndex];
        }

        public int RollDice()
        {
            return dice.Roll();
        }

        public bool MoveToken(Player player, Token token, int diceRoll)
        {
            if (token.IsInHome)
            {
                // Sacar una ficha de la casa si se sacó un 6
                if (diceRoll == 6)
                {
                    token.Position = 1; // Ubica la ficha en la casilla 1
                    token.IsInHome = false; // La ficha ya no está en la casa
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
                    return true; // Movimiento exitoso
                }

                return true; // Movimiento exitoso si no supera la casilla 100
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
            currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
        }

        public bool IsGameOver()
        {
            return players.Exists(player => player.HasWon());
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
