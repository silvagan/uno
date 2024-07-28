using Application;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Common;
using System.Text.RegularExpressions;

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

        ServerData server = new ServerData(0, new UnoMatch());
        server.match.isDirectionClockwise = true;

        bool matchStart = false;

        while (!matchStart)
        {
            List<UnoPlayer> connectedPlayers = ServerClient.GetAllPlayers(clients);
            bool isNull = false;
            foreach (UnoPlayer player in connectedPlayers)
            {
                if (player == null)
                {
                    isNull = true;
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

            if (listener.Pending())
            {
                TcpClient client = listener.AcceptTcpClient();
                ServerClient serverClient = new ServerClient();
                serverClient.clients = clients;
                serverClient.clients.Add(serverClient);

                // Handle the client in a separate thread
                Thread clientThread = new Thread(thread => serverClient.HandleClient(client, server));
                clientThread.Start();
            }
        }
        if (matchStart)
        {
            var rng = new Random();
            List<UnoCard> cards =  UnoCard.GenerateDeck();
            do
            {
                server.match.topCard = cards[rng.Next(0, cards.Count)];
            } while (server.match.topCard.type != UnoCardType.Number);

            foreach (ServerClient client in clients)
            {
                client.SendGameStart(server.match.topCard);
            }

            Console.WriteLine("match start");
        }
    }
}
public class ServerData
{
    public int IDincrement { get; set; }
    public UnoMatch match { get; set; }

    public ServerData(int idIncrement, UnoMatch match)
    {
        IDincrement = idIncrement;
        this.match = match;
    }
}