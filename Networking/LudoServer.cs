    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;

    public class LudoServer
    {
        private TcpListener listener;
        private List<TcpClient> clients = new List<TcpClient>();
        private string connectionString = "Server=localhost;Database=LudoDB;Integrated Security=True;";
        private int currentPlayerIndex = 0;

        List<string> availableColors = new List<string> { "Rojo", "Azul", "Verde", "Amarillo" };
        private Dictionary<string, int> playerRoles = new Dictionary<string, int>(); // Mapa de IP a roles
        private List<string> jugadoresConectados = new List<string>(); // Lista de IPs o identificadores de clientes conectados

    public void AssignRoleToClient(TcpClient client)
    {
        string clientIP = client.Client.RemoteEndPoint.ToString();

        if (jugadoresConectados.Count < 4) // Limita a 4 jugadores
        {
            jugadoresConectados.Add(clientIP);
            int assignedRole = jugadoresConectados.Count - 1; // Asigna el rol basado en el índice
            SendMessageToClient(client, $"RolAsignado:{assignedRole}"); // Enviar asignación de rol
            Console.WriteLine($"Cliente {clientIP} asignado como Jugador {assignedRole + 1}");
        }
        else
        {
            SendMessageToClient(client, "Juego lleno. No puedes unirte.");
            client.Close();
        }
    }
    private void SendMessageToClient(TcpClient client, string message)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            stream.Write(buffer, 0, buffer.Length);
        }

        public void Start(int port)
        {
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Console.WriteLine($"Servidor Ludo iniciado en el puerto {port}");

            Thread acceptThread = new Thread(AcceptClients);
            acceptThread.Start();
        }

        public void NotifyTurn()
        {
            int currentTurnIndex = GetCurrentTurnIndex(); // Método que devuelve el índice del jugador actual

            foreach (var client in clients)
            {
                SendMessageToClient(client, $"TURN:{currentTurnIndex}");
            }

        }


        private void AcceptClients()
        {
            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                lock (clients)
                {
                    clients.Add(client);
                }
                Console.WriteLine("Nuevo jugador conectado.");

                Thread clientThread = new Thread(() => HandleClient(client));
                clientThread.Start();
            }
        }


        private void HandleClient(TcpClient client)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];

                // Asigna un color automáticamente al cliente
                string assignedColor = AssignColor();
                if (assignedColor == "SinColor")
                {
                    SendMessage(client, "Error: No hay más colores disponibles.");
                    client.Close();
                    return;
                }

                // Envía el color asignado al cliente
                SendMessage(client, $"ColorAsignado:{assignedColor}");
                Console.WriteLine($"Color {assignedColor} asignado a un jugador.");

                while (true)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        Console.WriteLine($"Mensaje recibido: {message}");

                        if (message.StartsWith("TirarDado"))
                        {
                            string[] messageParts = message.Split(':');
                            int playerId = int.Parse(messageParts[1]);
                            int diceRoll = int.Parse(messageParts[2]);

                            UpdatePlayerPosition(playerId, diceRoll);
                            UpdateTurn(playerId);

                            BroadcastMessage($"Movimiento: Jugador {playerId} tiró el dado y se movió {diceRoll} posiciones.", client);
                            BroadcastMessage($"Turno: Es el turno del jugador {playerId + 1}.", client);
                        }
                        else
                        {
                            BroadcastMessage(message, client);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                lock (clients)
                {
                    clients.Remove(client);
                }
                client.Close();
            }
        }


        // Método para actualizar la posición del jugador en la base de datos
        private void UpdatePlayerPosition(int playerId, int diceRoll)
        {
            string query = "UPDATE PlayerGame SET CurrentPos = CurrentPos + @DiceRoll WHERE PlayerID = @PlayerID";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@DiceRoll", diceRoll);
                cmd.Parameters.AddWithValue("@PlayerID", playerId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }

            // Obtener nueva posición del jugador para enviar a los clientes
            int newPosition = GetPlayerPosition(playerId);

            // Notificar a los clientes sobre el movimiento
            BroadcastMessage($"UPDATE:MOVE:{playerId}:{newPosition}", null);
        }

        private int GetPlayerPosition(int playerId)
        {
            string query = "SELECT CurrentPos FROM PlayerGame WHERE PlayerID = @PlayerID";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@PlayerID", playerId);
                conn.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        // Método para cambiar el turno del jugador
        private void UpdateTurn(int playerId)
        {
            string query = "UPDATE PlayerGame SET MyTurn = 1 WHERE PlayerID = @PlayerID; UPDATE PlayerGame SET MyTurn = 0 WHERE PlayerID != @PlayerID";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@PlayerID", playerId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }

            // Obtener ID del próximo jugador (simplificado)
            int nextPlayerId = (playerId % clients.Count) + 1;

            // Notificar a los clientes sobre el turno
            BroadcastMessage($"UPDATE:TURN:{nextPlayerId}", null);
        }



        private void CreateGame()
        {
            string query = "INSERT INTO Game (Status, JoinDate) VALUES ('Pendiente', GETDATE());";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                Console.WriteLine("Nueva partida creada.");
            }
        }

        private void AddPlayer(string playerName, string playerColor)
        {
            string query = "INSERT INTO Player (Name, Color, Position, DateTime) VALUES (@Name, @Color, 0, GETDATE());";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Name", playerName);
                cmd.Parameters.AddWithValue("@Color", playerColor);
                conn.Open();
                cmd.ExecuteNonQuery();
                Console.WriteLine($"Jugador {playerName} agregado.");
            }
        }

        private void BroadcastMessage(string message, TcpClient sender)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);

            lock (clients)
            {
                foreach (var client in clients)
                {
                    if (client != sender)
                    {
                        try
                        {
                            client.GetStream().Write(buffer, 0, buffer.Length);
                        }
                        catch
                        {
                            Console.WriteLine("Error enviando mensaje a un cliente.");
                        }
                    }
                }
            }
        }

        public void Stop()
        {
            listener.Stop();
            lock (clients)
            {
                foreach (var client in clients)
                {
                    client.Close();
                }
                clients.Clear();
            }
            Console.WriteLine("Servidor detenido.");
        }


        private string AssignColor()
        {
            lock (availableColors)
            {
                if (availableColors.Count > 0)
                {
                    string color = availableColors[0];
                    availableColors.RemoveAt(0);
                    return color;
                }
                return "SinColor"; // Si no hay colores disponibles
            }
        }

        private void SendMessage(TcpClient client, string message)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            client.GetStream().Write(buffer, 0, buffer.Length);
        }

        public void ActualizarPosicionJugador(int jugadorId, int nuevaPosicion)
        {
            // Cadena de conexión a tu base de datos
            string connectionString = "Server=localhost;Database=LudoDB;Trusted_Connection=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "UPDATE Jugadores SET Posicion = @nuevaPosicion WHERE Id = @jugadorId";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@nuevaPosicion", nuevaPosicion);
                    command.Parameters.AddWithValue("@jugadorId", jugadorId);

                    command.ExecuteNonQuery();
                }
            }
        }

        private int GetCurrentTurnIndex()
        {
            return currentPlayerIndex;
        }


        private void BroadcastNextTurn()
        {
        currentPlayerIndex = (currentPlayerIndex + 1) % 4; // Cicla entre 0 y 3 (Jugadores 1 a 4)
        while (!jugadoresConectados.Contains($"Jugador {currentPlayerIndex + 1}"))
        {
            currentPlayerIndex = (currentPlayerIndex + 1) % 4; // Salta a los jugadores no conectados
        }
        BroadcastMessage($"TURN:{currentPlayerIndex}", null);
        }


    private void StartGame()
    {
        if (jugadoresConectados.Count > 0)
        {
            // Inicia el juego indicando que es el turno del primer jugador
            BroadcastMessage($"TURN:0", null); // "TURN:0" indica que es el turno de Jugador 1
            Console.WriteLine("Juego iniciado, turno de Jugador 1");
        }
    }
}
