using System;
using System.Collections.Generic;
using System.Net;
using DarkRift;
using DarkRift.Client;
using LiteNetLib.Utils;
using LiteNetLib;
public class LiteNetLibClientConnection : NetworkClientConnection
{
    public NetManager client;
    public EventBasedNetListener listener;
    public NetPeer peer;
    public bool disposedValue = false;
    public IPEndPoint[] remoteEndPoints;
    public DarkRift.ConnectionState state;

    private void NetworkLatencyEvent(NetPeer peer, int latency)
    {

    }
    public void DeliveryMessage(NetPeer peer, NetPacketReader reader, byte channel, LiteNetLib.DeliveryMethod deliveryMethod)
    {
        HandleLiteNetLibMessageReceived(reader, channel, (DarkRift.DeliveryMethod)deliveryMethod);
        reader.Recycle();
    }

    public override DarkRift.ConnectionState ConnectionState => state;

    public override IEnumerable<IPEndPoint> RemoteEndPoints => remoteEndPoints;

    public override IPEndPoint GetRemoteEndPoint(string name)
    {
        throw new ArgumentException("Not a valid endpoint name!");
    }
    public override void Connect(string ip, int port, byte[] array)
    {
        listener = new EventBasedNetListener();
        client = new NetManager(listener)
        {
            AutoRecycle = true,
            UnconnectedMessagesEnabled = true,
            NatPunchEnabled = true,
            AllowPeerAddressChange = true,
            BroadcastReceiveEnabled = true,
            UseNativeSockets = false,
            ChannelsCount = 7,
            EnableStatistics = false,

        };

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
        listener.NetworkLatencyUpdateEvent += NetworkLatencyEvent;




        remoteEndPoints = new[] { new IPEndPoint(IPAddress.Parse(ip), port) };
        client.Start();
        NetDataWriter netDataWriter = new NetDataWriter();
        netDataWriter.Put(array);
        peer = client.Connect(ip, port, netDataWriter);
    }

    public override bool SendMessageToReceiver(MessageBuffer message, byte channel, DarkRift.DeliveryMethod sendMode)
    {
        peer.Send(message.Buffer, message.Offset, message.Count, channel, (LiteNetLib.DeliveryMethod)sendMode);
        message.Dispose();
        return true;
    }

    public override bool Disconnect(string reason)
    {
        Console.WriteLine(reason);
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
            Disconnect("Disposing of LiteNetLib");
        }

        disposedValue = true;
    }

    private void HandleLiteNetLibMessageReceived(NetDataReader reader, byte channel, DarkRift.DeliveryMethod mode)
    {
        int length = reader.AvailableBytes;
        MessageBuffer message = MessageBuffer.Create(length);
        reader.GetBytes(message.Buffer, length);
        message.Offset = 0;
        message.Count = length;
        HandleMessageReceived(message, channel, mode);
        message.Dispose();
    }
    public void PerformUpdate()
    {
        client.PollEvents();
    }
}
