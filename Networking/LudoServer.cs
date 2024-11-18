using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class LudoServer
{
    private TcpListener listener;
    private List<TcpClient> clients = new List<TcpClient>();

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

                    // Reenvía el mensaje a los demás clientes
                    BroadcastMessage(message, client);
                }
            }
        }
        catch
        {
            Console.WriteLine("Un jugador se desconectó.");
            lock (clients)
            {
                clients.Remove(client);
            }
            client.Close();
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
}
