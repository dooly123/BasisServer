using System;
using System.Collections.Generic;
using System.Net;
using DarkRift;
using DarkRift.Server;
using ENet;
using EnetPlugin;

public class EnetServerConnection : NetworkServerConnection
{

    //Whether we're connected
    public override ConnectionState ConnectionState
    {
        get { return connectionState; }
    }
    private ConnectionState connectionState;
    private readonly IPEndPoint[] remoteEndPoints;

    //A list of endpoints we're connected to on the server
    public override IEnumerable<IPEndPoint> RemoteEndPoints
    {
        get
        {
            return remoteEndPoints;
        }
    }

    public EnetServerConnection(Peer peer)
    {
        this.peer = peer;
        remoteEndPoints = new[] { new IPEndPoint(IPAddress.Parse(peer.IP), peer.Port) };
    }

    private Peer peer;


    //Given a named endpoint this should return that
    public override IPEndPoint GetRemoteEndPoint(string name)
    {
        throw new ArgumentException("Not a valid endpoint name!");
    }

    public override void StartListening()
    {

    }

    //Called when the server wants to disconnect the client
    public override bool Disconnect()
    {
        peer.Disconnect(0);
        return true;
    }
    public void OnDisconnect()
    {
        HandleDisconnection();
    }
    public override bool SendMessageReciever(MessageBuffer message, SendMode sendMode)
    {
        byte[] data = new byte[message.Count];
        Array.Copy(message.Buffer, message.Offset, data, 0, message.Count);
        message.Dispose();
        return Send(data, sendMode, peer);
    }
    private bool Send(byte[] data, SendMode sendMode, Peer peer)
    {
        Packet packet = new Packet();
        packet.Create(data, data.Length, EnetChannelConversion.ActiveSendMode(sendMode)); // Unreliable Sequenced
        return peer.Send((byte)sendMode, ref packet);
    }

    public void HandleEnetMessageReceived(Event netEvent, SendMode ChannelID)
    {
        int length = netEvent.Packet.Length;
        MessageBuffer message = MessageBuffer.Create(length);
        netEvent.Packet.CopyTo(message.Buffer);
        message.Offset = 0;
        message.Count = length;
        HandleMessageReceived(message, ChannelID);
        message.Dispose();
    }
}
