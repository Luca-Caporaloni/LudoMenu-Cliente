using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System;
using BL;

public class LudoClient
{
    private TcpClient client;
    private NetworkStream stream;


    string connectionString = "Data Source=localhost;Initial Catalog=LudoDB;Integrated Security=True;Encrypt=False;TrustServerCertificate=True";

    // Propiedad para almacenar el nombre del jugador
    public string PlayerName { get; set; }

    public void Connect(string serverIp, int port)
    {
        client = new TcpClient();
        client.Connect(serverIp, port);
        stream = client.GetStream();
        Console.WriteLine("Conectado al servidor.");

        Thread listenThread = new Thread(ListenForMessages);
        listenThread.Start();
    }

    public void SendDiceRoll(int playerId, int diceRoll)
    {
        // Enviar el mensaje de lanzamiento de dado al servidor
        string message = $"TirarDado:{playerId}:{diceRoll}";
        SendMessage(message);
    }

    public void SendMessage(string message)
    {
        if (stream != null && stream.CanWrite)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            try
            {
                stream.Write(buffer, 0, buffer.Length);
                Console.WriteLine($"Mensaje enviado al servidor: {message}");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Error de E/S al enviar el mensaje: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("La conexión no está disponible para escribir.");
        }
    }

    public event Action<int> OnTurnReceived;

    private void ListenForMessages()
    {
        byte[] buffer = new byte[1024];
        while (true)
        {
            try
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    if (message.StartsWith("TURN"))
                    {
                        int currentTurn = int.Parse(message.Split(':')[1]);
                        OnTurnReceived?.Invoke(currentTurn);
                    }
                }
            }
            catch
            {
                Console.WriteLine("Desconectado del servidor.");
                break;
            }
        }
    }





    public void SetAsPlayerOne()
    {
        PlayerName = "Jugador 1"; // Configura el nombre del jugador
        Console.WriteLine("Te has configurado como Jugador 1.");
    }


    public void Disconnect()
    {
        client.Close();
        Console.WriteLine("Desconectado del servidor.");
    }
}
