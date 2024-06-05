using System;
using System.Collections.Generic;
using System.Threading;
using DarkRift;
using DarkRift.Server;
using ENet;
using Event = ENet.Event;
using EventType = ENet.EventType;
public class EnetListenerPlugin : NetworkListener
{
    private Host server;
    private Dictionary<Peer, EnetServerConnection> connections = new Dictionary<Peer, EnetServerConnection>();
    public Thread ServerIncomeThread;
    public static double ServerTickRate = 30;
    public static int PeerLimit = 1024;
    public static ushort SetPort = 4296;
    public EnetListenerPlugin(NetworkListenerLoadData pluginLoadData) : base(pluginLoadData)
    {
        Console.WriteLine("EnetListenerPlugin Is Being loaded....");
        Version = new Version(1, 0, 0);
        Console.WriteLine("EnetListenerPlugin ready to go");
    }
    public override Version Version { get; }
    public override void StartListening()
    {
        Console.WriteLine("Starting...");
        Library.Initialize();
        Console.WriteLine("Initialize!");
        server = new Host();
        Address address = new Address();
        Port = SetPort;
        address.Port = Port;
        server.Create(address, PeerLimit);
        Console.WriteLine("Server Booted");
        ServerIncomeThread = new Thread(WorkerThread);
        ServerIncomeThread.IsBackground = true; // Ensure the thread doesn't prevent the application from exiting
        ServerIncomeThread.Start();
    }
    public void WorkerThread()
    {
        double tickInterval = 1000.0 / ServerTickRate;
       TimeSpan Span = TimeSpan.FromMilliseconds(tickInterval);
        while (true)
        {
            // Place the code to be executed in the loop here
            ServerTick();
            Thread.Sleep(Span);
        }
    }
    public void ServerTick()
    {
        bool StillNeedToRun = true;
        while (StillNeedToRun)
        {
            server.Service(0, out Event netEvent);
            switch (netEvent.Type)
            {
                case EventType.None:
                    StillNeedToRun = false;
                    break;

                case EventType.Connect:
                    EnetServerConnection con = new EnetServerConnection(netEvent.Peer);
                    RegisterConnection(con);
                    connections.Add(netEvent.Peer, con);
                    Logger.Log("Client: " + netEvent.Peer.IP + netEvent.Peer.Port + " connected.", LogType.Trace);
                    break;

                case EventType.Disconnect:
                case EventType.Timeout:
                    HandleDisconnection(netEvent);
                    break;

                case EventType.Receive:
                    SendMode sendMode = (SendMode)netEvent.ChannelID;
                    connections[netEvent.Peer].HandleEnetMessageReceived(netEvent, sendMode);
                    netEvent.Packet.Dispose();
                    break;
            }
        }
        server.Flush();
    }

    public void HandleDisconnection(Event netEvent)
    {
        Logger.Log("Client: " + netEvent.Peer.IP + netEvent.Peer.Port + " disconnected.", LogType.Trace);
        connections[netEvent.Peer].OnDisconnect();
        connections.Remove(netEvent.Peer);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            server?.Dispose();
        }
    }
}
