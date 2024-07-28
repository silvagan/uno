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

        bool matchStart = false;

        while (!matchStart)
        {

            TcpClient client = listener.AcceptTcpClient();

            ServerClient serverClient = new ServerClient();
            serverClient.clients = clients;
            serverClient.clients.Add(serverClient);
            List<UnoPlayer> connectedPlayers = ServerClient.GetAllPlayers(clients);

            // Handle the client in a separate thread
            Thread clientThread = new Thread( thread => serverClient.HandleClient(client, match));
            clientThread.Start();

            bool isNull = false;
            foreach (UnoPlayer player in connectedPlayers)
            {
                if (player == null)
                {
                    Console.WriteLine("null");
                    isNull = true;
                }
                else
                {
                    Console.WriteLine(player.name); 
                }
            }

            if (isNull)
            {
                continue;
            }

            if (connectedPlayers.Count >= 2)
            {
                matchStart = true;
                foreach (UnoPlayer player in connectedPlayers)
                {
                    if (!player.isReady)
                    {
                        matchStart = false;
                    }
                }
            }
        }
        while (matchStart)
        {
            Console.WriteLine("match start");
        }
    }
}