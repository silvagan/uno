using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Common;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Application;

public class UnoClient
{
    public string server { get; set; }
    public Int32 port { get; set; }
    private NetworkStream stream { get; set; }
    public string name { get; set; }
    public TcpClient tcp { get; set; }
    public UnoMatch? match = null;

    public int ClientId = -1;

    public UnoClient(string server, Int32 port)
    {
        this.server = server;
        this.port = port;
    }

    public void AddStream(NetworkStream stream)
    {
        this.stream = stream;
    }

    public void OnMessageRecieved(ReceivedMessage msg)
    {
        if (msg.type == MessageType.ID)
        {
            ClientId = msg.payload[0];
            Console.WriteLine("Your ID is: {0}", ClientId);
        }
        if (msg.type == MessageType.Connect)
        {
            Console.WriteLine("Current connected players: {0}", System.Text.Encoding.ASCII.GetString(msg.payload, 0, msg.payload.Length));
        }
        else if (msg.type == MessageType.UpdateMatchData)
        {
            match = new UnoMatch();
            match.players = RecieveMatchData(msg.payload);
            Console.WriteLine("recieved match data");
        }
        else if (msg.type == MessageType.Disconnect)
        {
            Console.WriteLine("disconnect");
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
        MessageType type;
        int messageLength;

        try
        {
            type = (MessageType)stream.ReadByte();
            messageLength = stream.ReadByte();
            stream.Read(packet, 0, messageLength);
        }
        catch
        {
            return null;
        }

        byte[] payload = packet.ToArray();
        return new ReceivedMessage
        {
            type = type,
            payload = payload
        };
    }


    public void Update()
    {
        if (tcp.Client.Poll(0, SelectMode.SelectRead))
        {
            var message = ReceiveMessage();
            if (message != null)
            {
                OnMessageRecieved(message);
            }
        }
    }
    public void Connect()
    {
        try
        {
            TcpClient client = new TcpClient(server, port);
            tcp = client;
            NetworkStream stream = client.GetStream();
            AddStream(stream);

            // Translate the passed message into ASCII and store it as a Byte array.
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(name);

            SendMessage(MessageType.Connect, data);

            Console.WriteLine("Client name: {0}", name);

            // Receive the server response.

            // Explicit close is not necessary since TcpClient.Dispose() will be
            // called automatically.
            // stream.Close();
            // client.Close();
        }
        catch (ArgumentNullException e)
        {
            Console.WriteLine("ArgumentNullException: {0}", e);
        }
        catch (SocketException e)
        {
            Console.WriteLine("SocketException: {0}", e);
        }
    }
    public void Disconnect()
    {
        try
        {
            // Translate the passed message into ASCII and store it as a Byte array.
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(name);

            SendMessage(MessageType.Disconnect, data);

            Console.WriteLine("Disconnect: {0}", name);

            stream.Close();
            tcp.Close();
        }
        catch
        {
            Console.WriteLine("Disconnect error");
        }
    }
    public void UpdateReadiness(bool ready)
    {
        try
        {
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(ready.ToString());
            SendMessage(MessageType.UpdateReadiness, data);

            Console.WriteLine($"ready = {ready}");
        }
        catch
        {
            Console.WriteLine("UpdateReadyness error");
        }
    }

    public List<UnoPlayer> RecieveMatchData(byte[] payload)
    {
        List<UnoPlayer> players = new List<UnoPlayer>();

        int playerCount = payload[0];
        int currentByte = 1;
        for (int i = 0; i < playerCount; i++)
        {
            bool isReady = BitConverter.ToBoolean(payload, currentByte++);
            int id = payload[currentByte++];
            int nameLength = payload[currentByte++];
            string name = Encoding.ASCII.GetString(payload, currentByte, nameLength);
            currentByte += nameLength;
            UnoPlayer player = new UnoPlayer(name);
            player.isReady = isReady;
            player.id = id;
            players.Add(player);
        }
        return players;
    }

    public UnoMatch? GetMatchUpdate()
    {
        var m = match;
        if (m != null)
        {
            match = null;
        }
        return m;
    }
}
