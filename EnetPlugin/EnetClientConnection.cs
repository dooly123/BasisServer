using System;
using System.Collections.Generic;
using System.Net;
using DarkRift;
using DarkRift.Client;
using ENet;
using EnetPlugin;
using Event = ENet.Event;
using EventType = ENet.EventType;

public class EnetClientConnection : NetworkClientConnection
{


    public EnetClientConnection(string ip, int port)
    {
        Library.Initialize();
        this.ip = ip;
        this.port = port;

        remoteEndPoints = new[] {new IPEndPoint(IPAddress.Parse(ip), port)};
    }

    private string ip;
    private int port;
    private Host client;
    private Peer peer;
    private bool disposedValue = false;
    private readonly IPEndPoint[] remoteEndPoints;

    //Whether we're connected
    public override ConnectionState ConnectionState
    {
        get { return connectionState; }
    }
    private ConnectionState connectionState;

    //A list of endpoints we're connected to on the server
    public override IEnumerable<IPEndPoint> RemoteEndPoints
    {
        get
        {
            return remoteEndPoints;
        }
    }

    //Given a named endpoint this should return that
    public override IPEndPoint GetRemoteEndPoint(string name)
    {
            throw new ArgumentException("Not a valid endpoint name!");
    }

    //Called when DarkRiftClient.Connect is called
    public override void Connect()
    {
        client = new Host();
        client.Create(null, 1);
        Address address = new Address();
        address.SetHost(ip);
        address.Port = (ushort) port;
        peer = client.Connect(address, 200);
    }

    //...Sends a message unreliably!
    public override bool SendMessageToReceiver(MessageBuffer message, SendMode sendMode)
    {
        byte[] data = new byte[message.Count];
        Array.Copy(message.Buffer, message.Offset, data, 0, message.Count);
        bool r = Send(data, sendMode, peer);
        message.Dispose();
        return r;
    }

    //Called when the server wants to disconnect the client
    public override bool Disconnect()
    {
        peer.DisconnectNow(0);
        client.Dispose();
        return true;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposedValue)
        {
            return;
        }

        if (disposing)
        {
            Disconnect();
        }

        disposedValue = true;
    }
    private bool Send(byte[] data, SendMode sendMode, Peer peer)
    {
        Packet packet = new Packet();
        packet.Create(data, data.Length, EnetChannelConversion.ActiveSendMode(sendMode)); // Unreliable Sequenced |  PacketFlags.NoAllocate
        return peer.Send((byte)sendMode, ref packet);
    }

    private void HandleEnetMessageReceived(Event netEvent, SendMode mode)
    {
        int length = netEvent.Packet.Length;
        MessageBuffer message = MessageBuffer.Create(length);
        netEvent.Packet.CopyTo(message.Buffer);
        message.Offset = 0;
        message.Count = length;
        HandleMessageReceived(message, mode);
        message.Dispose();
    }

    public void PerformUpdate()
    {
        bool stillNeedToRun = true;
        while (stillNeedToRun)
        {
            Event netEvent;
            client.Service(0, out netEvent);
            switch (netEvent.Type)
            {
                case EventType.None:
                    stillNeedToRun = false;
                    break;

                case EventType.Connect:
                    Console.WriteLine("Client connected to server - ID: " + peer.ID);
                    connectionState = ConnectionState.Connected;
                    break;

                case EventType.Disconnect:
                    Console.WriteLine("Client disconnected from server");
                    connectionState = ConnectionState.Disconnected;
                    HandleDisconnection(new ArgumentException("Enet disconnected"));
                    break;

                case EventType.Timeout:
                    Console.WriteLine("Client connection timeout");
                    break;

                case EventType.Receive:
                    HandleEnetMessageReceived(netEvent, (SendMode)netEvent.ChannelID);
                    netEvent.Packet.Dispose();
                    break;
            }
        }
        client.Flush();
    }
}
