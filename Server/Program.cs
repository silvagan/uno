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
        TcpListener listener = new TcpListener(IPAddress.Any, PORT_NO);
        listener.Start();
        Console.WriteLine("Listening for incoming connections...");

        while (true)
        {
            TcpClient client = listener.AcceptTcpClient();
            Console.WriteLine("Client connected!");

            // Handle the client in a separate thread
            Thread clientThread = new Thread(UnoServer.HandleClient);
            clientThread.Start(client);
        }
    }

}