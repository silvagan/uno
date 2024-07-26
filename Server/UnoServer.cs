using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class UnoServer
    {
        public static void HandleClient(object clientObj)
        {
            TcpClient client = (TcpClient)clientObj;
            NetworkStream stream = client.GetStream();

            byte[] buffer = new byte[1024];
            int bytesRead;

            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                string receivedMessage = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Received: {receivedMessage}");

                // Process the received message as needed
                // ...

                // Send a response back to the client (optional)
                string responseMessage = "Server received your message!";
                byte[] responseBytes = Encoding.ASCII.GetBytes(responseMessage);
                stream.Write(responseBytes, 0, responseBytes.Length);
            }

            client.Close();
        }

    }
}
