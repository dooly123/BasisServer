using DarkRift.Server.Plugins.Commands;
using System;
using System.Threading;
using System.Threading.Tasks;
namespace DarkRift.Server.Plugins.BasisNetworking
{
    public class BasisPingManagement
    {
        public bool isRunning;
        public int pingIntervalMilliseconds;  // Interval between pings in milliseconds
        public CancellationTokenSource cancellationTokenSource;
        public Commands.BasisNetworking basisNetworking;
        public void Initalize(Commands.BasisNetworking basisNetworking, int pingIntervalSeconds)
        {
            this.basisNetworking = basisNetworking;
            pingIntervalMilliseconds = pingIntervalSeconds * 1000;
            cancellationTokenSource = new CancellationTokenSource();
            isRunning = true;
            Task.Run(() => PingLoop(cancellationTokenSource.Token));
        }
        public void Stop()
        {
            isRunning = false;
            cancellationTokenSource.Cancel();
        }
        private async Task PingLoop(CancellationToken cancellationToken)
        {
            while (isRunning)
            {
                try
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    // Send ping to server
                    MakeAndSendToEveryonePingMessage();

                    // Wait for the next interval
                    await Task.Delay(pingIntervalMilliseconds, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    // Task was canceled, exit the loop
                    break;
                }
                catch (Exception ex)
                {
                    // Handle exceptions if necessary
                    Console.WriteLine($"Error during ping: {ex.Message}");
                }
            }
        }
        public void MakeAndSendToEveryonePingMessage()
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                using (Message audioSegmentMessage = Message.Create(BasisTags.PingTag, writer))
                {
                    audioSegmentMessage.MakePingMessage();
                    Commands.BasisNetworking.BroadcastMessageToClients(audioSegmentMessage, Commands.BasisNetworking.EventsChannel,
                        Commands.BasisNetworking.ReadyClients, DeliveryMethod.Unreliable);
                }
            }
        }
    }
}
