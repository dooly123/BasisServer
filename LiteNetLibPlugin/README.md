# EnetListener for Darkrift 2.

This listener allows you to send UDP and RUDP messages thus chan ging the SendMode.Reliable to use RUDP instead of TCP which has many advantages
over TCP. It should also perform much faster but I haven't run any benchmarks yet.

## How to use
### UnityServer:
- Import Darkrift from the asset store.
- Download the neweset Unity release of [Enet C#] (https://github.com/nxrighthere/ENet-CSharp/releases) and add it to the assets folder.
- Clone/download and put this repository in the assets folder
- Add a EnetServer component to a gameobject to create a server
- Set the Configuration field of the server to the ExampleConfiguration in this repository
### UnityClient:
- Same import steps as for the UnityServer
- Add a EnetClient to a gameobject to create a client
### Standalone:
- Reference Enet, Darkrift, Darkrift.Server, Darkrift.Client
- In the Server.config set the lister type to "EnetListenerPlugin"
- Don't forget to call EnetListenerPlugin.ServerTick() somewhere in a loop of your plugin you can find the EnetListenerPlugin with
```Server.NetworkListenerManager.GetNetworkListenersByType<EnetListenerPlugin>().First();```

## Known Bugs
EnetClient.Connect() doesn't work, use EnetClient.ConnectInBackground() instead.
