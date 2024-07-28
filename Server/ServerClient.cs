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
        Console.WriteLine(((IPEndPoint)client.Client.RemoteEndPoint).Port);
        byte[] buffer = new byte[1024];
        int bytesRead;
        AddStream(stream);

        ReceivedMessage msg = ReceiveMessage();
        if (msg != null)
        {
            OnMessageRecieved(msg, match);
        }

    }

    public void OnMessageRecieved(ReceivedMessage msg, UnoMatch match)
    {
        if (msg.type == MessageType.Connect)
        {
            string receivedMessage = Encoding.ASCII.GetString(msg.payload, 0, msg.payload.Length);
            match.players.Add(new UnoPlayer(receivedMessage));
            Console.WriteLine($"{receivedMessage} connected!");
            string responseMessage = "";
            foreach (UnoPlayer player in match.players)
            {
                responseMessage += player.name;
            }

            byte[] responseBytes = Encoding.ASCII.GetBytes(responseMessage);

           
            foreach (ServerClient client in clients)
            {
                client.SendMessage(MessageType.Connect, responseBytes);
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

        byte[] payload = packet.Skip(2).ToArray();
        return new ReceivedMessage
        {
            type = messageType,
            payload = payload
        };
    }
}
