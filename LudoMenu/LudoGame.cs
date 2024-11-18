using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BL;
using Networking;

namespace LudoMenu
{
    public partial class LudoGame : Form
    {
        private Random random = new Random();
        private GameLogic gameLogic;
        private Dictionary<string, Panel> playerTokens = new Dictionary<string, Panel>(); // Cambiado a Panel
        private Dictionary<string, Panel> boardCells = new Dictionary<string, Panel>();
        private Dictionary<int, Color> specialCellColors = new Dictionary<int, Color>();
        private GameClient gameClient;
        private LudoClient client;

        public LudoGame(LudoClient client)
        {
            InitializeComponent();
            InitializeGame();
            InitializeBoard();
        }
      

        private void InitializeGame()
        {
            // Inicializa la lógica del juego
            gameLogic = new GameLogic();
            gameLogic.AddPlayer("Jugador 1", "Blue");
            gameLogic.AddPlayer("Jugador 2", "Red");
            gameLogic.AddPlayer("Jugador 3", "Green");
            gameLogic.AddPlayer("Jugador 4", "Yellow");

            lblCurrentPlayer.Text = $"Turno: {gameLogic.GetCurrentPlayer().Name}";
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
            int diceRoll = random.Next(1, 7);
            MessageBox.Show($"Has sacado un {diceRoll}");
            var currentPlayer = gameLogic.GetCurrentPlayer();
           // gameClient.Send($"MOVE:{currentPlayer.Name}:{diceRoll}");

            lblDadoNum.Text = $"{currentPlayer.Name} sacó un {diceRoll}.";

            if (diceRoll == 6 && currentPlayer.HasTokensInHome())
            {
                Token token = currentPlayer.ReleaseTokenFromHome();
                if (token != null)
                {
                    lblNarrator.Text = $"{currentPlayer.Name} ha sacado una ficha de la casa.";
                    UpdateBoard();
                }
                return; // Permitir que el jugador vuelva a tirar
            }

            Token tokenToMove = currentPlayer.SelectTokenToMove(diceRoll);

            if (tokenToMove == null)
            {
                lblNarrator.Text = $"{currentPlayer.Name} no puede mover ninguna ficha con este dado.";
                gameLogic.NextTurn();
                UpdateBoard();
                return;
            }

            // Intentar mover la ficha seleccionada
            if (gameLogic.MoveToken(currentPlayer, tokenToMove, diceRoll))
            {
                lblNarrator.Text = $"{currentPlayer.Name} movió la ficha a la casilla {tokenToMove.Position}.";

                // Verificar si el jugador llegó a la casilla 100
                if (tokenToMove.Position == 100)
                {
                    lblNarrator.Text = $"{currentPlayer.Name} ha llegado a la casilla 100 y ganó la partida. ¡Felicidades!";
                    MessageBox.Show($"{currentPlayer.Name} ha ganado el juego.", "Juego terminado");
                    EndGame(); // Lógica para terminar el juego
                    return;
                }

                // Manejar colisiones con otras fichas
                HandleTokenCollision(currentPlayer, tokenToMove);

                // Manejar casillas especiales
                HandleSpecialCell(currentPlayer, tokenToMove);

                UpdateBoard();
            }
            else
            {
                lblNarrator.Text = $"{currentPlayer.Name} no puede mover la ficha.";
            }

            // Actualizar el tablero después del movimiento
            UpdateBoard();

            // Si el jugador sacó un 6, no pasa el turno
            if (diceRoll == 6)
            {
                lblNarrator.Text = $"{currentPlayer.Name} ha sacado un 6 y puede volver a tirar.";
                return;
            }

            // Cambiar al siguiente jugador si no sacó un 6
            gameLogic.NextTurn();
            lblCurrentPlayer.Text = $"Turno: {gameLogic.GetCurrentPlayer().Name}";
        }

        private void UpdateBoard()
        {
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

            // Actualizar la posición de los jugadores en el tablero
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
                    if (boardCells.TryGetValue(token.Position.ToString(), out Panel cell))
                    {
                        cell.BackColor = Color.FromName(player.Color); // Color del jugador
                    }
                }
            }

            // Actualizar el turno del jugador actual
            lblCurrentPlayer.Text = $"Turno: {gameLogic.GetCurrentPlayer().Name}";
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

        private void HandleServerMessage(string message)
        {
            string[] parts = message.Split(':');

            if (parts[0] == "MOVE")
            {
                string playerName = parts[1];
                int newPosition = int.Parse(parts[2]);

                var player = gameLogic.Players.FirstOrDefault(p => p.Name == playerName);
                if (player != null)
                {
                    player.CurrentPosition = newPosition;
                    UpdateBoard();
                }
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
