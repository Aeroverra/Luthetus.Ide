﻿using Luthetus.Common.RazorLib.Notification.Models;
using Luthetus.Common.RazorLib.BackgroundTaskCase.Models;
using Luthetus.Common.RazorLib.KeyCase.Models;
using Luthetus.Common.RazorLib.FileSystem.Models;

namespace Luthetus.Ide.RazorLib.FileSystemCase.States;

public partial class FileSystemSync
{
    public void SaveFile(
        IAbsolutePath absolutePath,
        string content,
        Action<DateTime?> onAfterSaveCompletedWrittenDateTimeAction,
        CancellationToken cancellationToken = default)
    {
        BackgroundTaskService.Enqueue(Key<BackgroundTask>.NewKey(), ContinuousBackgroundTaskWorker.Queue.Key,
            "Handle Save File Action",
            async () => await SaveFileAsync(
                absolutePath,
                content,
                onAfterSaveCompletedWrittenDateTimeAction,
                cancellationToken));
    }

    private async Task SaveFileAsync(
        IAbsolutePath absolutePath,
        string content,
        Action<DateTime?> onAfterSaveCompletedWrittenDateTimeAction,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return;

        var absolutePathString = absolutePath.FormattedInput;

        string notificationMessage;

        if (absolutePathString is not null &&
            await _fileSystemProvider.File.ExistsAsync(absolutePathString))
        {
            await _fileSystemProvider.File.WriteAllTextAsync(
                absolutePathString,
                content);

            notificationMessage = $"successfully saved: {absolutePathString}";
        }
        else
        {
            // TODO: Save As to make new file
            notificationMessage = "File not found. TODO: Save As";
        }

        NotificationHelper.DispatchInformative("Save Action", notificationMessage, _luthetusCommonComponentRenderers, Dispatcher);

        DateTime? fileLastWriteTime = null;

        if (absolutePathString is not null)
        {
            fileLastWriteTime = await _fileSystemProvider.File
                .GetLastWriteTimeAsync(
                    absolutePathString,
                    CancellationToken.None);
        }

        onAfterSaveCompletedWrittenDateTimeAction?.Invoke(fileLastWriteTime);
    }
}