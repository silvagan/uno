using Application;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server;

public enum MessageType
{
    Connect,
    Disconnect,
    StartGame,
    PlaceCard,
    EndGame
}
public class ReceivedMessage
{
    public MessageType type;
    public byte[] payload;
};

public class ServerClient
{
    private NetworkStream stream { get; set; }
    public List<ServerClient> clients { get; set; }

    public ServerClient() { }


    public void AddStream(NetworkStream stream)
    {
        this.stream = stream;
    }

    public void HandleClient(object clientObj, UnoMatch match)
    {
        TcpClient client = (TcpClient)clientObj;
        NetworkStream stream = client.GetStream();
        Console.WriteLine($"from port: {((IPEndPoint)client.Client.RemoteEndPoint).Port}");
        byte[] buffer = new byte[1024];
        int bytesRead;
        AddStream(stream);
        while (true)
        {
            ReceivedMessage msg = ReceiveMessage();
            if (msg != null)
            {
                OnMessageRecieved(msg, match);
            }
        }

    }

    public void OnMessageRecieved(ReceivedMessage msg, UnoMatch match)
    {
        if (msg.type == MessageType.Connect)
        {
            string receivedMessage = Encoding.ASCII.GetString(msg.payload, 0, msg.payload.Length);
            match.players.Add(new UnoPlayer(receivedMessage));
            Console.WriteLine($"{receivedMessage} connected!");
            string responseString = "";
            for (int i = 0; i < match.players.Count; i++)
            {
                if (i == match.players.Count-1)
                {
                    responseString += $"{match.players[i].name}.";
                }
                else
                {
                    responseString += $"{match.players[i].name}, ";
                }
            }
            Console.WriteLine($"Current connected players: {responseString}");
            byte[] responseMessage = Encoding.ASCII.GetBytes(responseString);

            foreach (ServerClient client in clients)
            {
                client.SendMessage(MessageType.Connect, responseMessage);
            }
        }
        if (msg.type == MessageType.Disconnect)
        {
            string receivedMessage = Encoding.ASCII.GetString(msg.payload, 0, msg.payload.Length);
            match.players.RemoveAll(p => p.name == receivedMessage);
            Console.WriteLine($"{receivedMessage} disconnected");
            string responseString = "";
            for (int i = 0; i < match.players.Count; i++)
            {
                if (i == match.players.Count - 1)
                {
                    responseString += $"{match.players[i].name}.";
                }
                else
                {
                    responseString += $"{match.players[i].name}, ";
                }
            }
            Console.WriteLine($"Current connected players: {responseString}");
            byte[] responseMessage = Encoding.ASCII.GetBytes(responseString);

            foreach (ServerClient client in clients)
            {
                client.SendMessage(MessageType.Connect, responseMessage);
            }
        }
        else if (msg.type == MessageType.StartGame)
        {
            Console.WriteLine("startgame");
        }
        else if (msg.type == MessageType.EndGame)
        {
            Console.WriteLine("endgame");
        }
        else if (msg.type == MessageType.PlaceCard)
        {
            Console.WriteLine("placecard");
        }
    }
    public void SendMessage(MessageType type, Span<byte> payload)
    {
        int messageSize = 0;
        messageSize += 1; // Type
        messageSize += 1; // Length
        messageSize += payload.Length;
        byte[] messageBytes = new byte[messageSize];
        messageBytes[0] = (byte)type;
        messageBytes[1] = (byte)payload.Length;
        for (int i = 0; i < payload.Length; i++)
        {
            messageBytes[i + 2] = payload[i];
        }
        try
        {
            stream.Write(messageBytes);
        }
        catch
        {
            return;
        }
    }
    public ReceivedMessage? ReceiveMessage()
    {
        byte[] packet = new byte[255 + 2];
        Int32 packetLength;
        try
        {
            packetLength = stream.Read(packet, 0, packet.Length);
        }
        catch
        {
            return null;
        }
        MessageType messageType = (MessageType)packet[0];
        byte messageLength = packet[1];
        //Debug.Assert(messageLength == packetLength - 2);

        byte[] payload = packet.Skip(2).Take(messageLength).ToArray();
        return new ReceivedMessage
        {
            type = messageType,
            payload = payload
        };
    }
}
