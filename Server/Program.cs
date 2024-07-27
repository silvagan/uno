using Application;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server;

class Program
{
    const int PORT_NO = 8080;
    static void Main(string[] args)
    {
        //bool gameStart = false;

        TcpListener listener = new TcpListener(IPAddress.Any, PORT_NO);
        listener.Start();
        Console.WriteLine("Listening for incoming connections...");

        List<ServerClient> clients = new List<ServerClient>();

        listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

        UnoMatch match = new UnoMatch();
        match.isDirectionClockwise = true;

        while (true)
        {

            TcpClient client = listener.AcceptTcpClient();

            ServerClient serverClient = new ServerClient();
            serverClient.clients = clients;
            serverClient.clients.Add(serverClient);

            // Handle the client in a separate thread
            Thread clientThread = new Thread( thread => serverClient.HandleClient(client, match));
            clientThread.Start();










        }
    }
}