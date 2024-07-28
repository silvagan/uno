﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Application;

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

public class UnoClient
{
    public string server { get; set; }
    public Int32 port { get; set; }
    private NetworkStream stream { get; set; }
    public string name { get; set; }
    public TcpClient tcp { get; set; }

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
        if (msg.type == MessageType.Connect)
        {
            Console.WriteLine("current connected players: {0}", System.Text.Encoding.ASCII.GetString(msg.payload, 0, msg.payload.Length));
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
}
