﻿using System;
using System.Collections.Generic;
using System.Threading;
using DarkRift.Server;
using LiteNetLib;
public class LiteNetLibListenerPlugin : NetworkListener
{
    public EventBasedNetListener listener;
    public NetManager server;
    public Dictionary<NetPeer, LiteNetLibServerConnection> connections = new Dictionary<NetPeer, LiteNetLibServerConnection>();
    public Thread serverIncomeThread;
    public static double ServerTickRate = 30;
    public static int PeerLimit = 1024;
    public static ushort SetPort = 4296;

    public LiteNetLibListenerPlugin(NetworkListenerLoadData pluginLoadData) : base(pluginLoadData)
    {
        Console.WriteLine("LiteNetLibListenerPlugin Is Being loaded....");
        Version = new Version(1, 0, 0);
        Console.WriteLine("LiteNetLibListenerPlugin ready to go");
    }

    public override Version Version { get; }

    public override void StartListening()
    {
        Console.WriteLine("Starting...");
        listener = new EventBasedNetListener();
        server = new NetManager(listener)
        {
            AutoRecycle = true,
            UnconnectedMessagesEnabled = true,
            NatPunchEnabled = true
        };

        server.Start(SetPort);
        Console.WriteLine("Server Booted");

        listener.ConnectionRequestEvent += request =>
        {
            if (server.ConnectedPeersCount < PeerLimit)
                request.AcceptIfKey("SomeConnectionKey");
            else
                request.Reject();
        };

        listener.PeerConnectedEvent += peer =>
        {
            var con = new LiteNetLibServerConnection(peer);
            RegisterConnection(con);
            connections[peer] = con;
            Console.WriteLine($"Client: {peer.Address} connected.");
        };

        listener.PeerDisconnectedEvent += (peer, info) =>
        {
            if (connections.TryGetValue(peer, out var con))
            {
                HandleDisconnection(peer);
                Console.WriteLine($"Client:  {peer.Address} disconnected.");
            }
        };
        listener.NetworkReceiveEvent += NetworkReceiveEvent;

        serverIncomeThread = new Thread(WorkerThread)
        {
            IsBackground = true // Ensure the thread doesn't prevent the application from exiting
        };
        serverIncomeThread.Start();
    }

    private void NetworkReceiveEvent(NetPeer peer, NetPacketReader reader, byte channel, LiteNetLib.DeliveryMethod deliveryMethod)
    {
        if (connections.TryGetValue(peer, out LiteNetLibServerConnection con))
        {
            con.HandleLiteNetLibMessageReceived(peer, reader, (DarkRift.DeliveryMethod)deliveryMethod);
            reader.Recycle();
        }
    }

    public void WorkerThread()
    {
        double tickInterval = 1000.0 / ServerTickRate;
        TimeSpan span = TimeSpan.FromMilliseconds(tickInterval);
        while (true)
        {
            ServerTick();
            Thread.Sleep(span);
        }
    }

    public void ServerTick()
    {
        server.PollEvents();
    }

    public void HandleDisconnection(NetPeer peer)
    {
        if (connections.TryGetValue(peer, out LiteNetLibServerConnection connection))
        {
            connection.OnDisconnect();
            connections.Remove(peer);
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            server?.Stop();
        }
    }
}
