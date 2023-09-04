using System;
using System.IO;
using FolderSyncTool;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;


namespace FolderSyncTool
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length != 4)
            {
                Console.WriteLine("Usage: FolderSync <sourcePath> <replicaPath> <syncInterval> <logFilePath>");
                return;
            }

            string sourcePath = args[0];
            string replicaPath = args[1];
            int syncInterval = Convert.ToInt32(args[2]);
            string logFilePath = args[3];

            using CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken cancellationToken = cts.Token;

            FolderSyncronization synchronizer = new FolderSyncronization(sourcePath, replicaPath, logFilePath, syncInterval);

            var synchronizationTask = synchronizer.StartSynchronizationAsync(cancellationToken);

            Console.WriteLine("Press Enter to stop the synchronization...");
            Console.ReadLine();

            cts.Cancel();
            await synchronizationTask;
        }
    }
}