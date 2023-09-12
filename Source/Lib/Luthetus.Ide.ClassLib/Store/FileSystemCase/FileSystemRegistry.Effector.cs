﻿using Luthetus.Common.RazorLib.ComponentRenderers;
using Fluxor;
using Luthetus.Common.RazorLib.BackgroundTaskCase.BaseTypes;
using Luthetus.Common.RazorLib.Notification;
using Luthetus.Common.RazorLib.ComponentRenderers.Types;
using Luthetus.Common.RazorLib.Store.NotificationCase;
using Luthetus.Common.RazorLib.FileSystem.Interfaces;
using Luthetus.Ide.ClassLib.HostedServiceCase.FileSystem;

namespace Luthetus.Ide.ClassLib.Store.FileSystemCase;

public partial class FileSystemRegistry
{
    private class Effector
    {
        private readonly IFileSystemProvider _fileSystemProvider;
        private readonly ILuthetusCommonComponentRenderers _luthetusCommonComponentRenderers;
        private readonly ILuthetusIdeFileSystemBackgroundTaskService _luthetusIdeFileSystemBackgroundTaskService;

        private readonly object _syncRoot = new object();

        public Effector(
            IFileSystemProvider fileSystemProvider,
            ILuthetusCommonComponentRenderers luthetusCommonComponentRenderers,
            ILuthetusIdeFileSystemBackgroundTaskService luthetusIdeFileSystemBackgroundTaskService)
        {
            _fileSystemProvider = fileSystemProvider;
            _luthetusCommonComponentRenderers = luthetusCommonComponentRenderers;
            _luthetusIdeFileSystemBackgroundTaskService = luthetusIdeFileSystemBackgroundTaskService;
        }

        [EffectMethod]
        public Task HandleSaveFileAction(
            SaveFileAction saveFileAction,
            IDispatcher dispatcher)
        {
            // The lock is used here because I'm worried that the 'Effect' is not concurrency safe.
            //
            // I don't want 3 requests to save the same file, to end up writing out
            //     the first request last, resulting in lost data.
            //
            lock (_syncRoot)
            {
                var backgroundTask = new BackgroundTask(
                    async cancellationToken =>
                    {
                        if (saveFileAction.CancellationToken.IsCancellationRequested)
                            return;

                        var absolutePathString = saveFileAction.AbsolutePath.FormattedInput;

                        string notificationMessage;

                        if (absolutePathString is not null &&
                            await _fileSystemProvider.File.ExistsAsync(absolutePathString))
                        {
                            await _fileSystemProvider.File.WriteAllTextAsync(
                                absolutePathString,
                                saveFileAction.Content);

                            notificationMessage = $"successfully saved: {absolutePathString}";
                        }
                        else
                        {
                            // TODO: Save As to make new file
                            notificationMessage = "File not found. TODO: Save As";
                        }

                        if (_luthetusCommonComponentRenderers.InformativeNotificationRendererType is not null)
                        {
                            var notificationInformative = new NotificationRecord(
                                NotificationKey.NewKey(),
                                "Save Action",
                                _luthetusCommonComponentRenderers.InformativeNotificationRendererType,
                                new Dictionary<string, object?>
                                {
                                    {
                                        nameof(IInformativeNotificationRendererType.Message),
                                        notificationMessage
                                    },
                                },
                                TimeSpan.FromSeconds(5),
                                true,
                                null);

                            dispatcher.Dispatch(new NotificationRegistry.RegisterAction(
                                notificationInformative));
                        }

                        DateTime? fileLastWriteTime = null;

                        if (absolutePathString is not null)
                        {
                            fileLastWriteTime = await _fileSystemProvider.File
                                .GetLastWriteTimeAsync(
                                    absolutePathString,
                                    cancellationToken);
                        }

                        saveFileAction.OnAfterSaveCompletedWrittenDateTimeAction?.Invoke(fileLastWriteTime);
                    },
                    "Save File",
                    "TODO: Describe this task",
                    false,
                    _ => Task.CompletedTask,
                    dispatcher,
                    CancellationToken.None);

                _luthetusIdeFileSystemBackgroundTaskService.Enqueue(backgroundTask);
            }

            return Task.CompletedTask;
        }
    }
}