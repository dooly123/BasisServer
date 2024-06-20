using System;
using System.Collections.Generic;
using System.Net;
using DarkRift;
using DarkRift.Client;
using LiteNetLib.Utils;
using LiteNetLib;
public class LiteNetLibClientConnection : NetworkClientConnection
{
    public string IP;
    public int port;
    public NetManager client;
    public EventBasedNetListener listener;
    public NetPeer peer;
    public bool disposedValue = false;
    public readonly IPEndPoint[] remoteEndPoints;
    public DarkRift.ConnectionState state;

    public LiteNetLibClientConnection(string ip, int port)
    {
        this.IP = ip;
        this.port = port;

        remoteEndPoints = new[] { new IPEndPoint(IPAddress.Parse(ip), port) };

        listener = new EventBasedNetListener();
        client = new NetManager(listener);

        listener.PeerConnectedEvent += (peer) =>
        {
            state = DarkRift.ConnectionState.Connected;
            Console.WriteLine("Client connected to server - ID: " + peer.Id);
        };

        listener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
        {
            Console.WriteLine("Client disconnected from server");
            state = DarkRift.ConnectionState.Disconnected;
            HandleDisconnection(new ArgumentException("LiteNetLib disconnected"));
        };
        listener.NetworkReceiveEvent += DeliveryMessage;
    }

    public void DeliveryMessage(NetPeer peer, NetPacketReader reader, byte channel, LiteNetLib.DeliveryMethod deliveryMethod)
    {
        HandleLiteNetLibMessageReceived(reader, (DarkRift.DeliveryMethod)deliveryMethod);
        reader.Recycle();
    }

    public override DarkRift.ConnectionState ConnectionState => state;

    public override IEnumerable<IPEndPoint> RemoteEndPoints => remoteEndPoints;

    public override IPEndPoint GetRemoteEndPoint(string name)
    {
        throw new ArgumentException("Not a valid endpoint name!");
    }

    public override void Connect()
    {
        client.Start();
        peer = client.Connect(IP, port, "SomeConnectionKey");
    }

    public override bool SendMessageToReceiver(MessageBuffer message, DarkRift.DeliveryMethod sendMode)
    {
        byte[] data = new byte[message.Count];
        Array.Copy(message.Buffer, message.Offset, data, 0, message.Count);
        bool result = Send(data, sendMode, peer);
        message.Dispose();
        return result;
    }

    public override bool Disconnect()
    {
        peer?.Disconnect();
        client.Stop();
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

    private bool Send(byte[] data, DarkRift.DeliveryMethod sendMode, NetPeer peer)
    {
        NetDataWriter writer = new NetDataWriter();
        writer.Put(data);
        peer.Send(writer, (LiteNetLib.DeliveryMethod)sendMode);
        return true;
    }

    private void HandleLiteNetLibMessageReceived(NetDataReader reader, DarkRift.DeliveryMethod mode)
    {
        int length = reader.AvailableBytes;
        MessageBuffer message = MessageBuffer.Create(length);
        reader.GetBytes(message.Buffer, length);
        message.Offset = 0;
        message.Count = length;
        HandleMessageReceived(message, mode);
        message.Dispose();
    }

    public void PerformUpdate()
    {
        client.PollEvents();
    }
}
