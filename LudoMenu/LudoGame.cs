using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BL;



namespace LudoMenu
{


    public partial class LudoGame : Form
    {


        private Random random = new Random();
        private GameLogic gameLogic;
        private Dictionary<string, Panel> playerTokens = new Dictionary<string, Panel>(); // Cambiado a Panel
        private Dictionary<string, Panel> boardCells = new Dictionary<string, Panel>();
        private Dictionary<int, Color> specialCellColors = new Dictionary<int, Color>();



        private LudoClient ludoClient;
        string connectionString = "Data Source = localhost; Initial Catalog = LudoDB; Integrated Security = True; Encrypt=False;TrustServerCertificate=True";
        private int sessionId;
        private string PlayerName; // Agrega este campo a la clase

        public LudoGame(LudoClient client)
        {
            InitializeComponent();
            InitializeGame();
            InitializeBoard();

            ludoClient = client ?? throw new ArgumentNullException(nameof(client)); // Verifica que `client` no sea null
            ludoClient.OnTurnReceived += HandleTurnReceived; // Suscripción al evento   
        }

        private void HandleTurnReceived(int currentTurn)
        {
            if (PlayerName == $"Jugador {currentTurn + 1}")
            {
                btnRollDice.Invoke((Action)(() => btnRollDice.Enabled = true));
            }
            else
            {
                btnRollDice.Invoke((Action)(() => btnRollDice.Enabled = false));
            }
        }

        public void SetCurrentPlayer(string playerName)
        {
            foreach (var player in gameLogic.Players)
            {
                Console.WriteLine($"Jugador: {player.Name}, Color: {player.Color}");
            }

            int currentIndex = gameLogic.CurrentPlayerIndex;
            Console.WriteLine($"Índice del jugador actual: {currentIndex}");
        }

        private void InitializeGame()
        {

            // Inicializa la lógica del juego
            gameLogic = new GameLogic();


            // Crear el juego en la base de datos
            int gameId = CreateGame(sessionId, 1); // CurrentTurn = 1 (Jugador 1)
            gameLogic.GameId = gameId;

            foreach (var player in gameLogic.Players)
            {
                int playerId = AddPlayer(player.Name, player.Color, sessionId);
                player.PlayerId = playerId;

                // Establecer "MyTurn" como 1 solo para el primer jugador
                AddPlayerToGame(playerId, gameId, player.Name == "Jugador 1" ? 1 : 0);
            }

            gameLogic.AddPlayer("Jugador 1", "Blue");
            gameLogic.AddPlayer("Jugador 2", "Red");
            gameLogic.AddPlayer("Jugador 3", "Green");
            gameLogic.AddPlayer("Jugador 4", "Yellow");

            gameLogic.SetCurrentPlayer("Jugador 1");
            gameLogic.StartGame(gameId); // Sincronizar con la base de datos


            lblCurrentPlayer.Text = $"Turno: {gameLogic.GetCurrentPlayer().Name}";

            // Agregar jugadores a la base de datos
            foreach (var player in gameLogic.Players)
            {
                int playerId = AddPlayer(player.Name, player.Color, sessionId); // Llamada a la base de datos
                player.PlayerId = playerId; // Asignar el ID al jugador
            }

            
        }

        public void LoadGameState(int gameId)
        {
            string query = "SELECT CurrentTurn FROM Games WHERE GameID = @GameID";

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@GameID", gameId);
                connection.Open();

                int currentTurn = (int)command.ExecuteScalar();
                gameLogic.SetCurrentPlayer(gameLogic.Players[currentTurn].Name);
            }
        }


        public void AddPlayerToGame(int playerId, int gameId, int myTurn)
        {
            string query = "INSERT INTO PlayerGame (PlayerID, GameID, MyTurn, CurrentPos) " +
                           "VALUES (@PlayerID, @GameID, @MyTurn, @CurrentPos)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@PlayerID", playerId);
                command.Parameters.AddWithValue("@GameID", gameId);
                command.Parameters.AddWithValue("@MyTurn", myTurn);
                command.Parameters.AddWithValue("@CurrentPos", 0);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }



        private void InitializeBoard()
        {
            int cellNumber = 1;

            for (int i = 0; i < 10; i++) // 10 filas
            {
                for (int j = 0; j < 10; j++) // 10 columnas
                {
                    Panel cell = new Panel
                    {
                        Size = new Size(40, 40),
                        Location = new Point(40 * j, 40 * i),
                        BorderStyle = BorderStyle.FixedSingle,
                        Tag = cellNumber
                    };

                    // Agregar el número de la casilla como etiqueta
                    Label cellLabel = new Label
                    {
                        Text = cellNumber.ToString(),
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Font = new Font("Arial", 8, FontStyle.Bold),
                        BackColor = Color.Transparent
                    };
                    cell.Controls.Add(cellLabel);

                    Controls.Add(cell);
                    boardCells[cellNumber.ToString()] = cell;

                    cellNumber++;
                }
            }

            AssignSpecialCells(); // Configurar casillas especiales
        }

        public int AddPlayer(string name, string color, int sessionId)
        {
            int playerId = 0;

            string query = "INSERT INTO Players2 (Name, Color, SessionID) " +
                           "VALUES (@Name, @Color, @SessionID); " +
                           "SELECT CAST(SCOPE_IDENTITY() as int);";

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Name", name);
                command.Parameters.AddWithValue("@Color", color);
                command.Parameters.AddWithValue("@SessionID", sessionId);

                connection.Open();
                playerId = (int)command.ExecuteScalar();
            }

            if (playerId <= 0)
            {
                throw new Exception("Error al insertar el jugador en la tabla Players2.");
            }

            return playerId; // Retorna el ID generado automáticamente.
        }


        public int CreateGame(int sessionId, int currentTurn)
        {
            int gameId = 0;

            string query = "INSERT INTO Games (SessionID, CurrentTurn, IsGameOver) " +
                           "VALUES (@SessionID, @CurrentTurn, @IsGameOver); " +
                           "SELECT CAST(SCOPE_IDENTITY() as int);";

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@SessionID", sessionId);
                command.Parameters.AddWithValue("@CurrentTurn", currentTurn);
                command.Parameters.AddWithValue("@IsGameOver", 0); // FALSE en BIT

                connection.Open();
                gameId = (int)command.ExecuteScalar();
            }

            return gameId;
        }


        public void EndGame(int gameId)
        {
            // Terminar el juego en la base de datos
            EndGame(gameLogic.GameId);

            btnRollDice.Enabled = false; // Desactivar el botón de tirar el dado
            lblCurrentPlayer.Text = "Juego terminado.";
            lblNarrator.Text = "Gracias por jugar.";

            // Opcional: Preguntar si quieren reiniciar el juego
            var result = MessageBox.Show("¿Deseas reiniciar el juego?", "Reiniciar", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                // Reiniciar el juego
                RestartGame();
            }
        }


        public int GetCurrentTurn(int gameId)
        {
            int currentTurn = 0;

            string query = "SELECT CurrentTurn FROM Games WHERE GameID = @GameID";

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@GameID", gameId);

                connection.Open();
                currentTurn = (int)command.ExecuteScalar();
            }

            return currentTurn;
        }

        private void ProcessTurn(Player currentPlayer, int diceRoll)
        {
            // Verificar si el jugador tiene fichas en casa y el dado es 6
            if (diceRoll == 6 && currentPlayer.HasTokensInHome())
            {
                Token token = currentPlayer.ReleaseTokenFromHome();
                if (token != null)
                {
                    lblNarrator.Text = $"{currentPlayer.Name} ha sacado una ficha de la casa.";
                    UpdateBoard();
                    return; // Permitir que el jugador vuelva a tirar
                }
            }

            // Seleccionar ficha para mover
            Token tokenToMove = currentPlayer.SelectTokenToMove(diceRoll);
            if (tokenToMove == null)
            {
                lblNarrator.Text = $"{currentPlayer.Name} no tiene fichas para mover.";
                if (diceRoll != 6)
                {
                    EndTurn(); // Si no hay fichas y no sacó un 6, pasar turno
                }
                return;
            }

            // Mover ficha si es posible
            if (gameLogic.MoveToken(currentPlayer, tokenToMove, diceRoll))
            {
                lblNarrator.Text = $"{currentPlayer.Name} movió la ficha a la casilla {tokenToMove.Position}.";
                UpdatePlayerPosition(currentPlayer.PlayerId, tokenToMove.Position, gameLogic.GameId);

                if (tokenToMove.Position == 100)
                {
                    lblNarrator.Text = $"{currentPlayer.Name} ha llegado a la casilla 100 y ganó la partida. ¡Felicidades!";
                    MessageBox.Show($"{currentPlayer.Name} ha ganado el juego.", "Juego terminado");
                    EndGame();
                    return;
                }

                HandleTokenCollision(currentPlayer, tokenToMove);
                HandleSpecialCell(currentPlayer, tokenToMove);
            }
            else
            {
                lblNarrator.Text = $"{currentPlayer.Name} no puede mover la ficha.";
            }

            UpdateBoard();

            // Si sacó un 6, permitir volver a tirar; de lo contrario, finalizar el turno
            if (diceRoll == 6)
            {
                lblNarrator.Text = $"{currentPlayer.Name} ha sacado un 6 y puede volver a tirar.";
            }
            else
            {
                EndTurn();
            }
        }


        private void CreateToken(string tokenName, Color color, string playerColor)
        {
            Panel token = new Panel
            {
                Name = tokenName,
                Size = new Size(30, 30),
                BackColor = color,
                Location = GetHomePosition(playerColor, int.Parse(tokenName.Last().ToString()) - 1), // Posición inicial en casa
                Visible = false // Inicialmente invisible
            };

            Controls.Add(token);
            playerTokens[tokenName] = token;
        }


        private void btnRollDice_Click(object sender, EventArgs e)
        {

            // Obtener jugador actual
            var currentPlayer = gameLogic.GetCurrentPlayer();
            Console.WriteLine($"Nombre del jugador actual: {currentPlayer?.Name}");
            Console.WriteLine($"Nombre del jugador del cliente: {ludoClient.PlayerName}");

            if (currentPlayer == null || ludoClient.PlayerName != currentPlayer.Name)
            {
                MessageBox.Show("No es tu turno.");
                return;
            }

            // Generar el dado
            int diceRoll = gameLogic.RollDice();
            MessageBox.Show($"Has sacado un {diceRoll}");

            

            if (currentPlayer == null)
            {
                MessageBox.Show("No hay jugador actual.");
                return;
            }

            lblDadoNum.Text = $"{currentPlayer.Name} sacó un {diceRoll}.";
            gameLogic.MoveCurrentPlayer(diceRoll);


            // Verificar si el jugador tiene fichas en casa y el dado es 6
            if (diceRoll == 6 && currentPlayer.HasTokensInHome())
            {
                Token token = currentPlayer.ReleaseTokenFromHome();
                if (token != null)
                {
                    lblNarrator.Text = $"{currentPlayer.Name} ha sacado una ficha de la casa.";
                    UpdateBoard();
                    return; // Permitir que el jugador vuelva a tirar
                }
            }

            // Seleccionar ficha para mover
            Token tokenToMove = currentPlayer.SelectTokenToMove(diceRoll);
            if (tokenToMove == null)
            {
                lblNarrator.Text = $"{currentPlayer.Name} no tiene fichas para mover.";
                if (diceRoll != 6)
                {
                    gameLogic.NextTurn();
                    lblCurrentPlayer.Text = $"Turno: {gameLogic.GetCurrentPlayer().Name}";
                }
                return;
            }

            // Mover ficha si es posible
            if (gameLogic.MoveToken(currentPlayer, tokenToMove, diceRoll))
            {
                lblNarrator.Text = $"{currentPlayer.Name} movió la ficha a la casilla {tokenToMove.Position}.";

                // Actualizar posición en la base de datos
                UpdatePlayerPosition(currentPlayer.PlayerId, tokenToMove.Position, gameLogic.GetCurrentPlayer(gameLogic.GameId));

                if (tokenToMove.Position == 100)
                {
                    lblNarrator.Text = $"{currentPlayer.Name} ha llegado a la casilla 100 y ganó la partida. ¡Felicidades!";
                    MessageBox.Show($"{currentPlayer.Name} ha ganado el juego.", "Juego terminado");
                    EndGame();
                    return;
                }

                HandleTokenCollision(currentPlayer, tokenToMove);
                HandleSpecialCell(currentPlayer, tokenToMove);
            }
            else
            {
                lblNarrator.Text = $"{currentPlayer.Name} no puede mover la ficha.";
            }

            UpdateBoard();

            // Si sacó un 6, permitir volver a tirar; de lo contrario, finalizar el turno
            if (diceRoll == 6)
            {
                lblNarrator.Text = $"{currentPlayer.Name} ha sacado un 6 y puede volver a tirar.";
            }
            else
            {
                EndTurn();
            }
        }

        private void EndTurn()
        {
            gameLogic.NextTurn();
            lblCurrentPlayer.Text = $"Turno: {gameLogic.GetCurrentPlayer().Name}";
        }

        public void UpdatePlayerPosition(int playerId, int newPosition, int turn)
        {
            string query = "UPDATE PlayerGame SET CurrentPos = @CurrentPos, MyTurn = @MyTurn WHERE PlayerID = @PlayerID AND GameID = @GameID";

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@CurrentPos", newPosition);
                command.Parameters.AddWithValue("@MyTurn", turn == 1 ? 1 : 0); // MyTurn indica si es el turno del jugador
                command.Parameters.AddWithValue("@PlayerID", playerId);
                command.Parameters.AddWithValue("@GameID", gameLogic.GameId);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }


        public void UpdateCurrentTurn(int gameId, int newTurn)
        {
            string query = "UPDATE Games SET CurrentTurn = @CurrentTurn WHERE GameID = @GameID";

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@CurrentTurn", newTurn);
                command.Parameters.AddWithValue("@GameID", gameId);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private void SendDiceRoll(string playerName, int diceRoll)
        {
            // Enviar el mensaje de lanzamiento de dado al servidor
            string message = $"TirarDado:{playerName}:{diceRoll}";
            ludoClient.SendMessage(message);
        }

        private void UpdateBoard()
        {
            // Diccionario para rastrear las fichas en cada casilla
            Dictionary<int, List<Token>> tokensInCells = new Dictionary<int, List<Token>>();

            // Restaurar los colores originales de las casillas
            foreach (var kvp in boardCells)
            {
                int cellNumber = int.Parse(kvp.Key);

                Panel cell = kvp.Value;

                // Restaurar el color especial si aplica
                if (specialCellColors.ContainsKey(cellNumber))
                {
                    cell.BackColor = specialCellColors[cellNumber];
                }
                else
                {
                    cell.BackColor = Color.White; // Color normal de casillas
                }
            }

            // Actualizar la posición de las fichas en el tablero
            foreach (var player in gameLogic.Players)
            {
                foreach (var token in player.Tokens)
                {
                    if (token.IsInHome)
                    {
                        // Si la ficha está en casa, no se muestra en el tablero
                        continue;
                    }

                    // Obtener la casilla correspondiente
                    if (!tokensInCells.ContainsKey(token.Position))
                    {
                        tokensInCells[token.Position] = new List<Token>();
                    }
                    tokensInCells[token.Position].Add(token);

                    if (boardCells.TryGetValue(token.Position.ToString(), out Panel cell))
                    {
                        cell.BackColor = Color.FromName(player.Color); // Color del jugador
                    }
                }
            }

            // Detectar colisiones y enviar fichas "comidas" a casa
            foreach (var kvp in tokensInCells)
            {
                if (kvp.Value.Count > 1) // Más de una ficha en la misma casilla
                {
                    List<Token> tokens = kvp.Value;
                    string ownerColor = tokens[0].Player.Color;

                    // Verificar si hay fichas de jugadores diferentes
                    if (tokens.Any(t => t.Player.Color != ownerColor))
                    {
                        foreach (var token in tokens)
                        {
                            if (token.Player.Color != ownerColor)
                            {
                                // Enviar la ficha enemiga de vuelta a la casa
                                token.Position = 0; // Asume que la posición 0 es la casa
                                token.IsInHome = true;
                                Console.WriteLine($"Ficha del jugador {token.Player.Name} fue comida y regresó a la casa.");
                            }
                        }
                    }
                }
            }

            // Actualizar el turno del jugador actual
            lblCurrentPlayer.Text = $"Turno: {gameLogic.GetCurrentPlayer().Name}";
        }

        private void UpdateTurnOnServer(int gameId, string nextPlayerName)
        {
            string message = $"UPDATE_TURN:{nextPlayerName}";
            ludoClient.SendMessage(message); // Notificar al servidor el cambio de turno
        }

        // Método auxiliar para verificar si una casilla está ocupada por alguna ficha
        private bool IsCellOccupied(int cellNumber)
        {
            return gameLogic.Players.Any(player => player.Tokens.Any(token => token.Position == cellNumber && !token.IsInHome));
        }

        // Define posiciones iniciales para las fichas en la casa (casilla 1)
        private Point GetHomePosition(string color, int tokenIndex)
        {
            // Aquí todas las fichas empiezan en la misma casilla (1 o 0, dependiendo de tu preferencia)
            return new Point(20 + tokenIndex * 30, 20); // Ejemplo para la casilla 1
        }

        private void AssignSpecialCells()
        {
            var specialCells = new Dictionary<int, string>
    {
        { 15, "Pierde turno" },
        { 35, "Retrocede 10" },
        { 55, "Avanza 10" },
        { 75, "Pierde ficha" }
    };

            foreach (var cell in specialCells)
            {
                if (boardCells.TryGetValue(cell.Key.ToString(), out Panel panel))
                {
                    panel.BackColor = Color.Orange; // Color para casillas especiales
                    specialCellColors[cell.Key] = Color.Orange; // Guardar el color especial
                    panel.Tag = cell.Value; // Guardar el efecto directamente en el Tag
                }
            }
        }

        private void HandleSpecialCell(Player player, Token token)
        {
            // Verificar si el token está en una casilla especial
            if (boardCells.TryGetValue(token.Position.ToString(), out Panel cell) && cell.Tag is string effect)
            {
                switch (effect)
                {
                    case "Pierde turno":
                        player.SkipTurn = true;
                        lblNarrator.Text = $"{player.Name} cayó en 'Pierde turno' y perderá su siguiente turno.";
                        break;

                    case "Retrocede 10":
                        token.Position = Math.Max(0, token.Position - 10); // Asegurarse de que no sea menor a 0
                        lblNarrator.Text = $"{player.Name} retrocede 10 casillas y ahora está en la casilla {token.Position}.";
                        break;

                    case "Avanza 10":
                        token.Position = Math.Min(100, token.Position + 10); // Asegurarse de que no supere la última casilla
                        lblNarrator.Text = $"{player.Name} avanza 10 casillas y ahora está en la casilla {token.Position}.";
                        break;

                    case "Pierde ficha":
                        token.ReturnToHome();
                        lblNarrator.Text = $"{player.Name} perdió su ficha y vuelve a la casa.";
                        break;
                }

                // Actualizar el tablero después del efecto
                UpdateBoard();
            }
        }

        private void HandleTokenCollision(Player currentPlayer, Token movingToken)
        {
            foreach (var player in gameLogic.Players)
            {
                // Ignorar el jugador actual
                if (player == currentPlayer)
                    continue;

                foreach (var token in player.Tokens)
                {
                    // Si hay colisión (misma posición y el token no está en casa)
                    if (token.Position == movingToken.Position && !token.IsInHome)
                    {
                        // Enviar la ficha del otro jugador a la casa
                        token.ReturnToHome();
                        lblNarrator.Text = $"{currentPlayer.Name} comió a la ficha de {player.Name}. ¡La ficha de {player.Name} vuelve a la casa!";
                        UpdateBoard();
                        return;
                    }
                }
            }
        }

        private void RestartGame()
        {
            // Reinicia la lógica del juego
            gameLogic.ResetGame();

            // Reinicia las posiciones de los tokens
            foreach (var token in playerTokens.Values)
            {
                token.Location = GetHomePosition(token.Name, 0);
                token.Visible = false;
            }

            // Restaurar el estado inicial
            lblCurrentPlayer.Text = $"Turno: {gameLogic.GetCurrentPlayer().Name}";
            lblNarrator.Text = "El juego ha sido reiniciado.";
            btnRollDice.Enabled = true;
            UpdateBoard();
        }


        private void EndGame()
        {
            btnRollDice.Enabled = false; // Desactivar el botón de tirar el dado
            lblCurrentPlayer.Text = "Juego terminado.";
            lblNarrator.Text = "Gracias por jugar.";

            // Opcional: Preguntar si quieren reiniciar el juego
            var result = MessageBox.Show("¿Deseas reiniciar el juego?", "Reiniciar", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                // Reiniciar el juego
                RestartGame();
            }
        }

        // Método para manejar el mensaje recibido del servidor
        private void HandleServerMessage(string message)
        {
            string[] parts = message.Split(':');

            if (parts[0] == "UPDATE_TURN")
            {
                string currentTurnPlayer = parts[1];
                lblCurrentPlayer.Text = $"Turno: {currentTurnPlayer}";

                btnRollDice.Enabled = (ludoClient.PlayerName == currentTurnPlayer); // Habilitar si es tu turno
            }
            else if (parts[0] == "UPDATE_BOARD")
            {
                // Actualizar estado del tablero
                UpdateBoard();
            }

            if (parts[0] == "TirarDado")
            {
                string playerName = parts[1];
                int diceRoll = int.Parse(parts[2]);

                // Lógica para actualizar el estado de la interfaz o realizar el movimiento en otros jugadores
                lblNarrator.Text = $"{playerName} ha sacado un {diceRoll}.";
                UpdateBoard();
            }
            else if (parts[0] == "WIN")
            {
                string winner = parts[1];   
                MessageBox.Show($"{winner} ha ganado el juego.");
                this.Close();
            }
        }

        private void LudoGame_Load(object sender, EventArgs e)
        {

        }
    }
}
