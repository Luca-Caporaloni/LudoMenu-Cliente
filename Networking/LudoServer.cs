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

    public void Start(int port)
    {
        listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Console.WriteLine($"Servidor Ludo iniciado en el puerto {port}");

        Thread acceptThread = new Thread(AcceptClients);
        acceptThread.Start();
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

            while (true)
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Mensaje recibido: {message}");

                    // Si el mensaje es un lanzamiento de dado
                    if (message.StartsWith("TirarDado"))
                    {
                        // Extraemos el jugador y el valor del dado
                        string[] messageParts = message.Split(':');
                        int playerId = int.Parse(messageParts[1]);
                        int diceRoll = int.Parse(messageParts[2]);

                        // Actualizar la posición del jugador en la base de datos
                        UpdatePlayerPosition(playerId, diceRoll);

                        // Cambiar el turno del jugador
                        UpdateTurn(playerId);

                        // Enviar a todos los clientes la actualización de la posición y el turno
                        BroadcastMessage($"Movimiento: Jugador {playerId} tiró el dado y se movió {diceRoll} posiciones.", client);
                        BroadcastMessage($"Turno: Es el turno del jugador {playerId + 1}.", client);
                    }
                    // Reenviar otros mensajes a los clientes (por ejemplo, chat o información adicional)
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
            Console.WriteLine($"Posición de jugador {playerId} actualizada.");
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
            Console.WriteLine($"Turno cambiado a jugador {playerId}.");
        }
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
}
