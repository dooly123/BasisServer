using System;
using System.Collections.Generic;
using System.Net;
using DarkRift;
using DarkRift.Server;
using LiteNetLib;
using LiteNetLib.Utils;
using DeliveryMethod = DarkRift.DeliveryMethod;

public class LiteNetLibServerConnection : NetworkServerConnection
{
    public NetPeer peer;
    public readonly IPEndPoint[] remoteEndPoints;
    public DarkRift.ConnectionState connectionState;

    public LiteNetLibServerConnection(NetPeer peer)
    {
        this.peer = peer;
        remoteEndPoints = new[] { new IPEndPoint(IPAddress.Parse(peer.Address.ToString()), peer.Port) };
        connectionState = DarkRift.ConnectionState.Connected;
    }

    public override DarkRift.ConnectionState ConnectionState => connectionState;

    public override IEnumerable<IPEndPoint> RemoteEndPoints => remoteEndPoints;

    public override IPEndPoint GetRemoteEndPoint(string name)
    {
        throw new ArgumentException("Not a valid endpoint name!");
    }

    public override void StartListening()
    {
        // No explicit start listening needed in LiteNetLib
    }

    public override bool Disconnect()
    {
        peer.Disconnect();
        connectionState = DarkRift.ConnectionState.Disconnected;
        return true;
    }

    public void OnDisconnect()
    {
        HandleDisconnection();
    }

    public override bool SendMessageReceiver(MessageBuffer message, DeliveryMethod sendMode)
    {
        byte[] data = new byte[message.Count];
        Array.Copy(message.Buffer, message.Offset, data, 0, message.Count);
        bool result = Send(data, sendMode, peer);
        message.Dispose();
        return result;
    }

    private bool Send(byte[] data, DeliveryMethod sendMode, NetPeer peer)
    {
        peer.Send(data, (LiteNetLib.DeliveryMethod)sendMode);
        return true;
    }

    public void HandleLiteNetLibMessageReceived(NetPeer fromPeer, NetDataReader reader, DeliveryMethod deliveryMethod)
    {
        int length = reader.AvailableBytes;
        MessageBuffer message = MessageBuffer.Create(length);
        reader.GetBytes(message.Buffer, length);
        message.Offset = 0;
        message.Count = length;
        HandleMessageReceived(message, deliveryMethod);
        message.Dispose();
    }
}
