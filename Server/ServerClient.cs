using Application;
using System;
using System.Collections.Generic;
using System.Dynamic;
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

    UpdateReadiness,

    UpdateMatchData,

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
    public bool clientConnected = true;
    private NetworkStream stream { get; set; }
    public List<ServerClient> clients { get; set; }
    public UnoPlayer player { get; set; }

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
        while (clientConnected)
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
            UnoPlayer temp = new UnoPlayer(receivedMessage);
            player = temp;
            match.players.Add(temp);
            Console.WriteLine($"{receivedMessage} connected!");
            SendMatchData();
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
            clients.Remove(this);
            clientConnected = false;
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
        else if (msg.type == MessageType.UpdateReadiness)
        {
            bool receivedMessage = false;
            try
            {
                receivedMessage = bool.Parse(Encoding.ASCII.GetString(msg.payload, 0, msg.payload.Length));
            }
            catch
            {
                Console.WriteLine("could not parse readiness");
            }

            player.isReady = receivedMessage;
            if (player.isReady)
            {
                Console.WriteLine($"Player [{this.player.name}] is ready.");
            }
            else
            {
                Console.WriteLine($"Player [{this.player.name}] is not ready.");
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

    public void SendMatchData()
    {
        List<UnoPlayer> players = GetAllPlayers();
        int size = 0;

        foreach (UnoPlayer player in players)
        {
            size += 1 + 1 + player.name.Length;
        }
        size += 1;
        byte[] payload = new byte[size];


        int i = 0;
        payload[i++] = (byte)players.Count;
        foreach (UnoPlayer player in players)
        {
            payload[i++] = player.isReady ? (byte)1 : (byte)0;
            payload[i++] = (byte)player.name.Length;
            Encoding.ASCII.GetBytes(player.name).CopyTo(payload, i);
            i += player.name.Length;
        }
        SendMessage(MessageType.UpdateMatchData, payload);
    }

    public List<UnoPlayer> GetAllPlayers()
    {
        List<UnoPlayer> players = new List<UnoPlayer>();
        foreach (ServerClient client in clients)
        {
            players.Add(client.player);
        }
        return players;
    }
}
