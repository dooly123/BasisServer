using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DarkRift.Server.Plugins.BasisNetworking.Server_Reduction_System
{
    public class BasisServerReductionPulse
    {
        public int[] lodUpdateRates; // Array to store update rates for each LOD
        private readonly List<Task> lodTasks;  // List of tasks for each LOD
        private readonly CancellationTokenSource cancellationTokenSource; // Token to cancel tasks
        public int numberOfLods;      // Number of LODs

        public BasisServerReductionPulse(int lodCount, int[] updateRatesMilliseconds)
        {
            // Validate inputs
            if (lodCount <= 0)
                throw new ArgumentException("LOD count must be greater than zero.");
            if (updateRatesMilliseconds == null || updateRatesMilliseconds.Length != lodCount)
                throw new ArgumentException("Update rates array must match the number of LODs.");

            numberOfLods = lodCount;
            lodUpdateRates = updateRatesMilliseconds;
            lodTasks = new List<Task>();
            cancellationTokenSource = new CancellationTokenSource();

            // Start tasks for each LOD
            for (int index = 0; index < numberOfLods; index++)
            {
                int lodIndex = index; // Capture loop variable for task
                var task = Task.Run(() => Pulse(lodIndex, cancellationTokenSource.Token));
                lodTasks.Add(task);
            }
        }

        private async Task Pulse(int lodIndex, CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    // Perform network-related logic for this LOD
                    Console.WriteLine($"LOD {lodIndex} pulse sent at {DateTime.Now}");

                    // Delay based on the update rate for this LOD
                    await Task.Delay(lodUpdateRates[lodIndex], token);
                }
            }
            catch (TaskCanceledException)
            {
                // Task was cancelled, handle cleanup if needed
                Console.WriteLine($"LOD {lodIndex} task canceled.");
            }
        }

        public void StopPulse()
        {
            // Signal cancellation
            cancellationTokenSource.Cancel();

            // Wait for all tasks to complete
            Task.WaitAll(lodTasks.ToArray());
            Console.WriteLine("All LOD pulses stopped.");
        }
    }
}
