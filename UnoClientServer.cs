using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class UnoClientServer
{
    public static void Execute()
    {
        var ipAddress = IPAddress.Loopback; // Use localhost
        var port = 8080; // Choose a port

        // Start server (listener)
        var listener = new TcpListener(ipAddress, port);
        listener.Start();
        Console.WriteLine($"Server listening on {ipAddress}:{port}");

        // Accept incoming connections
        var client = listener.AcceptTcpClient(); // Blocking call
        HandleClient(client);

        // Connect client
        using var clientSocket = new TcpClient();
        clientSocket.Connect(ipAddress, port); // Blocking call
        HandleClient(clientSocket);
    }

    static void HandleClient(TcpClient client)
    {
        using var stream = client.GetStream();
        var data = Encoding.UTF8.GetBytes("Hello from server!");
        stream.Write(data, 0, data.Length);
        client.Close();
    }
}