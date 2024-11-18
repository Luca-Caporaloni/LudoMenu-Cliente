using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class LudoClient
{
    private TcpClient client;
    private NetworkStream stream;

    public void Connect(string serverIp, int port)
    {
        client = new TcpClient();
        client.Connect(serverIp, port);
        stream = client.GetStream();
        Console.WriteLine("Conectado al servidor.");

        Thread listenThread = new Thread(ListenForMessages);
        listenThread.Start();
    }

    public void SendMessage(string message)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(message);
        stream.Write(buffer, 0, buffer.Length);
    }

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
                    Console.WriteLine($"Mensaje del servidor: {message}");
                }
            }
            catch
            {
                Console.WriteLine("Desconectado del servidor.");
                break;
            }
        }
    }
}
