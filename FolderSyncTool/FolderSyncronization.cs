using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FolderSyncTool;

namespace FolderSyncTool
{
    internal class FolderSyncronization
    {
        private readonly string sourcePath;
        private readonly string replicaPath;
        private readonly Logger logger;
        private readonly int syncInterval;

        public FolderSyncronization(string sourcePath, string replicaPath, string logFilePath, int syncInterval)
        {
            this.sourcePath = sourcePath;
            this.replicaPath = replicaPath;
            this.syncInterval = syncInterval;
            logger = new Logger(logFilePath);
        }

        public async Task StartSynchronizationAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await SynchronizeFoldersAsync(cancellationToken);
                await Task.Delay(syncInterval * 1000, cancellationToken);
            }
        }

        private async Task SynchronizeFoldersAsync(CancellationToken cancellationToken)
        {
            logger.LogMessage("Synchronization started.");

            try
            {
                await SyncDirectoriesAsync(sourcePath, replicaPath, cancellationToken);
                await SyncFilesAsync(sourcePath, replicaPath, cancellationToken);

                logger.LogMessage("Synchronization completed.");
            }
            catch (Exception ex)
            {
                logger.LogMessage($"Error during synchronization: {ex}");
            }
        }
        private async Task SyncDirectoriesAsync(string sourceDirectory, string replicaDirectory, CancellationToken cancellationToken)
        {
            
                foreach (var sourceSubDir in Directory.GetDirectories(sourceDirectory, "*", SearchOption.AllDirectories))
                {
                    string relativePath = sourceSubDir.Substring(sourceDirectory.Length).TrimStart(Path.DirectorySeparatorChar);
                    string targetSubDir = Path.Combine(replicaDirectory, relativePath);

                    if (!Directory.Exists(targetSubDir))
                    {
                        Directory.CreateDirectory(targetSubDir);
                        logger.LogMessage($"Created folder: {relativePath}");
                    }
                }

                foreach (var targetSubDir in Directory.GetDirectories(replicaDirectory, "*", SearchOption.AllDirectories))
                {
                    string relativePath = targetSubDir.Substring(replicaDirectory.Length).TrimStart(Path.DirectorySeparatorChar);
                    string sourceSubDir = Path.Combine(sourceDirectory, relativePath);

                    if (!Directory.Exists(sourceSubDir))
                    {
                        Directory.Delete(targetSubDir, true);
                        logger.LogMessage($"Deleted folder: {relativePath}");
                    }
                }
            
        }
        private async Task SyncFilesAsync(string sourceDir, string targetDir, CancellationToken cancellationToken)
        {
            try
            {
                var sourceFileInfo = Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories)
                .Select(filePath => FileHashHelper.GetHashFileInfo(filePath))
                .ToList();

                var targetFileInfo = Directory.GetFiles(targetDir, "*", SearchOption.AllDirectories)
                    .Select(filePath => FileHashHelper.GetHashFileInfo(filePath))
                    .ToList();

                var filesToDelete = targetFileInfo
                    .Where(targetFile => !sourceFileInfo.Select(sourceFile => sourceFile.FileName).Contains(targetFile.FileName)
                      || !sourceFileInfo.Select(sourceFile => sourceFile.Hash).Contains(targetFile.Hash))
                    .ToList();

                var filesToUpdate = sourceFileInfo
                    .Where(sourceFile => targetFileInfo.Any(targetFile =>
                    (targetFile.Hash == sourceFile.Hash && targetFile.FileName != sourceFile.FileName) ||
                    (targetFile.Hash != sourceFile.Hash && targetFile.FileName == sourceFile.FileName)))
                    .ToList();

                var filesToAdd = sourceFileInfo
                     .Where(sourceFile => !targetFileInfo.Select(targetFile => targetFile.FileName).Contains(sourceFile.FileName)
                      && !targetFileInfo.Select(targetFile => targetFile.Hash).Contains(sourceFile.Hash))
                     .ToList();

                foreach (var fileToDelete in filesToDelete)
                {
                    File.Delete(fileToDelete.FilePath);
                    logger.LogMessage($"Deleted file: {fileToDelete.FileName} at {targetDir}");
                }

                foreach (var fileToUpdate in filesToUpdate)
                {
                    string targetFile = fileToUpdate.FilePath.Replace(sourcePath, replicaPath);
                    await CopyFileAsync(fileToUpdate.FilePath, targetFile, cancellationToken);
                    logger.LogMessage($"Updated file: {fileToUpdate.FileName} at {targetDir}");
                }

                foreach (var fileToAdd in filesToAdd)
                {
                    string targetFile = fileToAdd.FilePath.Replace(sourcePath, replicaPath);
                    await CopyFileAsync(fileToAdd.FilePath, targetFile, cancellationToken);
                    logger.LogMessage($"Added file: {fileToAdd.FileName} at {targetDir}");
                }

                if (!filesToAdd.Any() && !filesToDelete.Any() && !filesToUpdate.Any())
                {
                    logger.LogMessage($"No file changes were found.");
                }
            }
            catch (Exception ex)
            {
                logger.LogMessage($"Error during file synchronization: {ex}");
            }
        }

        private async Task CopyFileAsync(string sourceFile, string targetFile, CancellationToken cancellationToken)
        {
            using (var sourceStream = File.OpenRead(sourceFile))
            using (var targetStream = File.Create(targetFile))
            {
                await sourceStream.CopyToAsync(targetStream, bufferSize: 81920, cancellationToken);
            }
        }
    }
}
