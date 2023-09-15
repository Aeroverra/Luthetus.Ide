﻿using Luthetus.Common.RazorLib.ComponentRenderers;
using Fluxor;
using Luthetus.Common.RazorLib.BackgroundTaskCase.BaseTypes;
using Luthetus.Common.RazorLib.Notification;
using Luthetus.Common.RazorLib.FileSystem.Interfaces;

namespace Luthetus.Ide.RazorLib.FileSystemCase.States;

public partial class FileSystemSync
{
    private readonly IFileSystemProvider _fileSystemProvider;
    private readonly ILuthetusCommonComponentRenderers _luthetusCommonComponentRenderers;

    public FileSystemSync(
        IFileSystemProvider fileSystemProvider,
        ILuthetusCommonComponentRenderers luthetusCommonComponentRenderers,
        IBackgroundTaskService backgroundTaskService,
        IDispatcher dispatcher)
    {
        _fileSystemProvider = fileSystemProvider;
        _luthetusCommonComponentRenderers = luthetusCommonComponentRenderers;

        BackgroundTaskService = backgroundTaskService;
        Dispatcher = dispatcher;
    }

    public IBackgroundTaskService BackgroundTaskService { get; }
    public IDispatcher Dispatcher { get; }
}