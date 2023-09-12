﻿using Luthetus.Ide.ClassLib.FileTemplates;
using Luthetus.Ide.ClassLib.InputFile;
using Luthetus.Ide.ClassLib.Store.InputFileCase;
using Luthetus.Ide.ClassLib.Store.TerminalCase;
using Luthetus.Ide.ClassLib.Clipboard;
using Luthetus.Ide.ClassLib.CommandLine;
using Luthetus.Ide.ClassLib.ComponentRenderers;
using Luthetus.Ide.ClassLib.ComponentRenderers.Types;
using Luthetus.Ide.ClassLib.FileConstants;
using Luthetus.Ide.ClassLib.TreeViewImplementations;
using Luthetus.Common.RazorLib.BackgroundTaskCase.Usage;
using Luthetus.Common.RazorLib.Menu;
using Fluxor;
using System.Collections.Immutable;
using Luthetus.Common.RazorLib.BackgroundTaskCase.BaseTypes;
using Luthetus.Common.RazorLib.Notification;
using Luthetus.Common.RazorLib.ComponentRenderers.Types;
using Luthetus.Common.RazorLib.Store.NotificationCase;
using Luthetus.Common.RazorLib.Clipboard;
using Luthetus.Common.RazorLib.Namespaces;
using Luthetus.Common.RazorLib.FileSystem.Interfaces;
using Luthetus.Common.RazorLib.ComponentRenderers;
using Luthetus.Common.RazorLib.FileSystem.Classes.LuthetusPath;

namespace Luthetus.Ide.ClassLib.Menu;

public class MenuOptionsFactory : IMenuOptionsFactory
{
    private readonly ILuthetusIdeComponentRenderers _luthetusIdeComponentRenderers;
    private readonly ILuthetusCommonComponentRenderers _luthetusCommonComponentRenderers;
    private readonly IFileSystemProvider _fileSystemProvider;
    private readonly IEnvironmentProvider _environmentProvider;
    private readonly IClipboardService _clipboardService;
    private readonly BackgroundTaskService _backgroundTaskService;

    public MenuOptionsFactory(
        ILuthetusIdeComponentRenderers luthetusIdeComponentRenderers,
        ILuthetusCommonComponentRenderers luthetusCommonComponentRenderers,
        IFileSystemProvider fileSystemProvider,
        IEnvironmentProvider environmentProvider,
        IClipboardService clipboardService,
        BackgroundTaskService backgroundTaskService)
    {
        _luthetusIdeComponentRenderers = luthetusIdeComponentRenderers;
        _luthetusCommonComponentRenderers = luthetusCommonComponentRenderers;
        _fileSystemProvider = fileSystemProvider;
        _environmentProvider = environmentProvider;
        _clipboardService = clipboardService;
        _backgroundTaskService = backgroundTaskService;
    }

    public MenuOptionRecord NewEmptyFile(IAbsolutePath parentDirectory, Func<Task> onAfterCompletion)
    {
        return new MenuOptionRecord("New Empty File", MenuOptionKind.Create,
            WidgetRendererType: _luthetusIdeComponentRenderers.FileFormRendererType,
            WidgetParameters: new Dictionary<string, object?>
            {
                { nameof(IFileFormRendererType.FileName), string.Empty },
                { nameof(IFileFormRendererType.CheckForTemplates), false },
                {
                    nameof(IFileFormRendererType.OnAfterSubmitAction),
                    new Action<string, IFileTemplate?, ImmutableArray<IFileTemplate>>(
                        (fileName, exactMatchFileTemplate, relatedMatchFileTemplates) =>
                            PerformNewFileAction(
                                fileName,
                                exactMatchFileTemplate,
                                relatedMatchFileTemplates,
                                new NamespacePath(string.Empty, parentDirectory),
                                onAfterCompletion))
                },
            });
    }

    public MenuOptionRecord NewTemplatedFile(NamespacePath parentDirectory, Func<Task> onAfterCompletion)
    {
        return new MenuOptionRecord("New Templated File", MenuOptionKind.Create,
            WidgetRendererType: _luthetusIdeComponentRenderers.FileFormRendererType,
            WidgetParameters: new Dictionary<string, object?>
            {
                { nameof(IFileFormRendererType.FileName), string.Empty },
                { nameof(IFileFormRendererType.CheckForTemplates), true },
                {
                    nameof(IFileFormRendererType.OnAfterSubmitAction),
                    new Action<string, IFileTemplate?, ImmutableArray<IFileTemplate>>(
                        (fileName, exactMatchFileTemplate, relatedMatchFileTemplates) =>
                            PerformNewFileAction(
                                fileName,
                                exactMatchFileTemplate,
                                relatedMatchFileTemplates,
                                parentDirectory,
                                onAfterCompletion))
                },
            });
    }

    public MenuOptionRecord NewDirectory(IAbsolutePath parentDirectory, Func<Task> onAfterCompletion)
    {
        return new MenuOptionRecord("New Directory", MenuOptionKind.Create,
            WidgetRendererType: _luthetusIdeComponentRenderers.FileFormRendererType,
            WidgetParameters: new Dictionary<string, object?>
            {
                { nameof(IFileFormRendererType.FileName), string.Empty },
                { nameof(IFileFormRendererType.IsDirectory), true },
                {
                    nameof(IFileFormRendererType.OnAfterSubmitAction),
                    new Action<string, IFileTemplate?, ImmutableArray<IFileTemplate>>(
                        (directoryName, _, _) =>
                            PerformNewDirectoryAction(directoryName, parentDirectory, onAfterCompletion))
                },
            });
    }

    public MenuOptionRecord DeleteFile(IAbsolutePath absolutePath, Func<Task> onAfterCompletion)
    {
        return new MenuOptionRecord("Delete", MenuOptionKind.Delete,
            WidgetRendererType: _luthetusIdeComponentRenderers.DeleteFileFormRendererType,
            WidgetParameters: new Dictionary<string, object?>
            {
                { nameof(IDeleteFileFormRendererType.AbsolutePath), absolutePath },
                { nameof(IDeleteFileFormRendererType.IsDirectory), true },
                {
                    nameof(IDeleteFileFormRendererType.OnAfterSubmitAction),
                    new Action<IAbsolutePath>(afp => PerformDeleteFileAction(afp, onAfterCompletion))
                },
            });
    }

    public MenuOptionRecord RenameFile(
        IAbsolutePath sourceAbsolutePath,
        IDispatcher dispatcher,
        Func<Task> onAfterCompletion)
    {
        return new MenuOptionRecord("Rename", MenuOptionKind.Update,
            WidgetRendererType: _luthetusIdeComponentRenderers.FileFormRendererType,
            WidgetParameters: new Dictionary<string, object?>
            {
                {
                    nameof(IFileFormRendererType.FileName),
                    sourceAbsolutePath.IsDirectory
                        ? sourceAbsolutePath.NameNoExtension
                        : sourceAbsolutePath.NameWithExtension
                },
                { nameof(IFileFormRendererType.IsDirectory), sourceAbsolutePath.IsDirectory },
                {
                    nameof(IFileFormRendererType.OnAfterSubmitAction),
                    new Action<string, IFileTemplate?, ImmutableArray<IFileTemplate>>((nextName, _, _) =>
                        PerformRenameAction(sourceAbsolutePath, nextName, dispatcher, onAfterCompletion))
                },
            });
    }

    public MenuOptionRecord CopyFile(IAbsolutePath absolutePath, Func<Task> onAfterCompletion)
    {
        return new MenuOptionRecord("Copy", MenuOptionKind.Update,
            OnClick: () => PerformCopyFileAction(absolutePath, onAfterCompletion));
    }

    public MenuOptionRecord CutFile(IAbsolutePath absolutePath, Func<Task> onAfterCompletion)
    {
        return new MenuOptionRecord("Cut", MenuOptionKind.Update,
            OnClick: () => PerformCutFileAction(absolutePath, onAfterCompletion));
    }

    public MenuOptionRecord PasteClipboard(
        IAbsolutePath directoryAbsolutePath,
        Func<Task> onAfterCompletion)
    {
        return new MenuOptionRecord("Paste", MenuOptionKind.Update,
            OnClick: () => PerformPasteFileAction(directoryAbsolutePath, onAfterCompletion));
    }

    public MenuOptionRecord RemoveCSharpProjectReferenceFromSolution(
        TreeViewSolution treeViewSolution,
        TreeViewNamespacePath projectNode,
        TerminalSession terminalSession,
        IDispatcher dispatcher,
        Func<Task> onAfterCompletion)
    {
        return new MenuOptionRecord("Remove (no files are deleted)", MenuOptionKind.Delete,
            WidgetRendererType: _luthetusIdeComponentRenderers.RemoveCSharpProjectFromSolutionRendererType,
            WidgetParameters: new Dictionary<string, object?>
            {
                {
                    nameof(IRemoveCSharpProjectFromSolutionRendererType.AbsolutePath),
                    projectNode.Item.AbsolutePath
                },
                {
                    nameof(IDeleteFileFormRendererType.OnAfterSubmitAction),
                    new Action<IAbsolutePath>(_ => PerformRemoveCSharpProjectReferenceFromSolutionAction(
                        treeViewSolution,
                        projectNode,
                        terminalSession,
                        dispatcher,
                        onAfterCompletion))
                },
            });
    }

    public MenuOptionRecord AddProjectToProjectReference(
        TreeViewNamespacePath projectReceivingReference,
        TerminalSession terminalSession,
        IDispatcher dispatcher,
        Func<Task> onAfterCompletion)
    {
        return new MenuOptionRecord("Add Project Reference", MenuOptionKind.Other,
            OnClick: () => PerformAddProjectToProjectReferenceAction(
                projectReceivingReference,
                terminalSession,
                dispatcher,
                onAfterCompletion));
    }

    public MenuOptionRecord RemoveProjectToProjectReference(
        TreeViewCSharpProjectToProjectReference treeViewCSharpProjectToProjectReference,
        TerminalSession terminalSession,
        IDispatcher dispatcher,
        Func<Task> onAfterCompletion)
    {
        return new MenuOptionRecord("Remove Project Reference", MenuOptionKind.Other,
            OnClick: () => PerformRemoveProjectToProjectReferenceAction(
                treeViewCSharpProjectToProjectReference,
                terminalSession,
                dispatcher,
                onAfterCompletion));
    }

    public MenuOptionRecord MoveProjectToSolutionFolder(
        TreeViewSolution treeViewSolution,
        TreeViewNamespacePath treeViewProjectToMove,
        TerminalSession terminalSession,
        IDispatcher dispatcher,
        Func<Task> onAfterCompletion)
    {
        return new MenuOptionRecord("Move to Solution Folder", MenuOptionKind.Other,
            WidgetRendererType: _luthetusIdeComponentRenderers.FileFormRendererType,
            WidgetParameters: new Dictionary<string, object?>
            {
                { nameof(IFileFormRendererType.FileName), string.Empty },
                { nameof(IFileFormRendererType.IsDirectory), false },
                {
                    nameof(IFileFormRendererType.OnAfterSubmitAction),
                    new Action<string, IFileTemplate?, ImmutableArray<IFileTemplate>>((nextName, _, _) =>
                        PerformMoveProjectToSolutionFolderAction(
                            treeViewSolution,
                            treeViewProjectToMove,
                            nextName,
                            terminalSession,
                            dispatcher,
                            onAfterCompletion))
                },
            });
    }

    public MenuOptionRecord RemoveNuGetPackageReferenceFromProject(
        NamespacePath modifyProjectNamespacePath,
        TreeViewCSharpProjectNugetPackageReference treeViewCSharpProjectNugetPackageReference,
        TerminalSession terminalSession,
        IDispatcher dispatcher,
        Func<Task> onAfterCompletion)
    {
        return new MenuOptionRecord("Remove NuGet Package Reference", MenuOptionKind.Other,
            OnClick: () => PerformRemoveNuGetPackageReferenceFromProjectAction(
                modifyProjectNamespacePath,
                treeViewCSharpProjectNugetPackageReference,
                terminalSession,
                dispatcher,
                onAfterCompletion));
    }

    private void PerformNewFileAction(
        string fileName,
        IFileTemplate? exactMatchFileTemplate,
        ImmutableArray<IFileTemplate> relatedMatchFileTemplates,
        NamespacePath namespacePath,
        Func<Task> onAfterCompletion)
    {
        _backgroundTaskService.Enqueue(
            BackgroundTaskKey.NewKey(),
            CommonBackgroundTaskWorker.Queue.Key,
            "New File Action",
            async () =>
            {
                if (exactMatchFileTemplate is null)
                {
                    var emptyFileAbsolutePathString = namespacePath.AbsolutePath.FormattedInput + fileName;

                    var emptyFileAbsolutePath = new AbsolutePath(
                        emptyFileAbsolutePathString,
                        false,
                        _environmentProvider);

                    await _fileSystemProvider.File.WriteAllTextAsync(
                        emptyFileAbsolutePath.FormattedInput,
                        string.Empty,
                        CancellationToken.None);
                }
                else
                {
                    var allTemplates = new[] { exactMatchFileTemplate }
                        .Union(relatedMatchFileTemplates)
                        .ToArray();

                    foreach (var fileTemplate in allTemplates)
                    {
                        var templateResult = fileTemplate.ConstructFileContents.Invoke(
                            new FileTemplateParameter(
                                fileName,
                                namespacePath,
                                _environmentProvider));

                        await _fileSystemProvider.File.WriteAllTextAsync(
                            templateResult.FileNamespacePath.AbsolutePath.FormattedInput,
                            templateResult.Contents,
                            CancellationToken.None);
                    }
                }

                await onAfterCompletion.Invoke();
            });
    }

    private void PerformNewDirectoryAction(
        string directoryName,
        IAbsolutePath parentDirectory,
        Func<Task> onAfterCompletion)
    {
        var directoryAbsolutePathString = parentDirectory.FormattedInput + directoryName;

        var directoryAbsolutePath = new AbsolutePath(
            directoryAbsolutePathString,
            true,
            _environmentProvider);

        _backgroundTaskService.Enqueue(
            BackgroundTaskKey.NewKey(),
            CommonBackgroundTaskWorker.Queue.Key,
            "New Directory Action",
            async () =>
            {
                await _fileSystemProvider.Directory.CreateDirectoryAsync(
                    directoryAbsolutePath.FormattedInput,
                    CancellationToken.None);

                await onAfterCompletion.Invoke();
            });
    }

    private void PerformDeleteFileAction(
        IAbsolutePath absolutePath,
        Func<Task> onAfterCompletion)
    {
        _backgroundTaskService.Enqueue(
            BackgroundTaskKey.NewKey(),
            CommonBackgroundTaskWorker.Queue.Key,
            "Delete File Action",
            async () =>
            {
                if (absolutePath.IsDirectory)
                {
                    await _fileSystemProvider.Directory.DeleteAsync(
                        absolutePath.FormattedInput,
                        true,
                        CancellationToken.None);
                }
                else
                {
                    await _fileSystemProvider.File.DeleteAsync(
                        absolutePath.FormattedInput);
                }

                await onAfterCompletion.Invoke();
            });
    }

    private void PerformCopyFileAction(
        IAbsolutePath absolutePath,
        Func<Task> onAfterCompletion)
    {
        _backgroundTaskService.Enqueue(
            BackgroundTaskKey.NewKey(),
            CommonBackgroundTaskWorker.Queue.Key,
            "Copy File Action",
            async () =>
            {
                await _clipboardService
                    .SetClipboard(
                        ClipboardFacts.FormatPhrase(
                            ClipboardFacts.CopyCommand,
                            ClipboardFacts.AbsolutePathDataType,
                            absolutePath.FormattedInput));

                await onAfterCompletion.Invoke();
            });
    }

    private void PerformCutFileAction(
        IAbsolutePath absolutePath,
        Func<Task> onAfterCompletion)
    {
        _backgroundTaskService.Enqueue(
            BackgroundTaskKey.NewKey(),
            CommonBackgroundTaskWorker.Queue.Key,
            "Cut File Action",
            async () =>
            {
                await _clipboardService
                    .SetClipboard(
                        ClipboardFacts.FormatPhrase(
                            ClipboardFacts.CutCommand,
                            ClipboardFacts.AbsolutePathDataType,
                            absolutePath.FormattedInput));

                await onAfterCompletion.Invoke();
            });
    }

    private void PerformPasteFileAction(
        IAbsolutePath receivingDirectory,
        Func<Task> onAfterCompletion)
    {
        _backgroundTaskService.Enqueue(
            BackgroundTaskKey.NewKey(),
            CommonBackgroundTaskWorker.Queue.Key,
            "Paste File Action",
            async () =>
            {
                var clipboardContents = await _clipboardService.ReadClipboard();

                if (ClipboardFacts.TryParseString(clipboardContents, out var clipboardPhrase))
                {
                    if (clipboardPhrase is not null &&
                        clipboardPhrase.DataType == ClipboardFacts.AbsolutePathDataType)
                    {
                        if (clipboardPhrase.Command == ClipboardFacts.CopyCommand ||
                            clipboardPhrase.Command == ClipboardFacts.CutCommand)
                        {
                            IAbsolutePath? clipboardAbsolutePath = null;

                            if (await _fileSystemProvider.Directory.ExistsAsync(clipboardPhrase.Value))
                            {
                                clipboardAbsolutePath = new AbsolutePath(
                                    clipboardPhrase.Value,
                                    true,
                                    _environmentProvider);
                            }
                            else if (await _fileSystemProvider.File.ExistsAsync(clipboardPhrase.Value))
                            {
                                clipboardAbsolutePath = new AbsolutePath(
                                    clipboardPhrase.Value,
                                    false,
                                    _environmentProvider);
                            }

                            if (clipboardAbsolutePath is not null)
                            {
                                var successfullyPasted = true;

                                try
                                {
                                    if (clipboardAbsolutePath.IsDirectory)
                                    {
                                        var clipboardDirectoryInfo = new DirectoryInfo(
                                            clipboardAbsolutePath.FormattedInput);

                                        var receivingDirectoryInfo = new DirectoryInfo(
                                            receivingDirectory.FormattedInput);

                                        CopyFilesRecursively(clipboardDirectoryInfo, receivingDirectoryInfo);
                                    }
                                    else
                                    {
                                        var destinationAbsolutePathString =
                                            receivingDirectory.FormattedInput +
                                            clipboardAbsolutePath.NameWithExtension;

                                        var sourceAbsolutePathString = clipboardAbsolutePath
                                            .FormattedInput;

                                        await _fileSystemProvider.File.CopyAsync(
                                            sourceAbsolutePathString,
                                            destinationAbsolutePathString);
                                    }
                                }
                                catch (Exception)
                                {
                                    successfullyPasted = false;
                                }

                                if (successfullyPasted && clipboardPhrase.Command == ClipboardFacts.CutCommand)
                                {
                                    // TODO: Rerender the parent of the deleted due to cut file
                                    PerformDeleteFileAction(clipboardAbsolutePath, onAfterCompletion);
                                }
                                else
                                {
                                    await onAfterCompletion.Invoke();
                                }
                            }
                        }
                    }
                }
            });
    }

    private IAbsolutePath? PerformRenameAction(
        IAbsolutePath sourceAbsolutePath,
        string nextName,
        IDispatcher dispatcher,
        Func<Task> onAfterCompletion)
    {
        // If the current and next name match when compared
        // with case insensitivity
        if (string.Compare(
                sourceAbsolutePath.NameWithExtension,
                nextName,
                StringComparison.OrdinalIgnoreCase)
                    == 0)
        {
            var temporaryNextName = _environmentProvider.GetRandomFileName();

            var temporaryRenameResult = PerformRenameAction(
                sourceAbsolutePath,
                temporaryNextName,
                dispatcher,
                () => Task.CompletedTask);

            if (temporaryRenameResult is null)
            {
                onAfterCompletion.Invoke();
                return null;
            }
            else
                sourceAbsolutePath = temporaryRenameResult;
        }

        var sourceAbsolutePathString = sourceAbsolutePath.FormattedInput;

        var parentOfSource = sourceAbsolutePath.AncestorDirectories.Last();

        var destinationAbsolutePathString = parentOfSource.FormattedInput + nextName;

        try
        {
            if (sourceAbsolutePath.IsDirectory)
                _fileSystemProvider.Directory.MoveAsync(sourceAbsolutePathString, destinationAbsolutePathString);
            else
                _fileSystemProvider.File.MoveAsync(sourceAbsolutePathString, destinationAbsolutePathString);
        }
        catch (Exception e)
        {
            if (_luthetusCommonComponentRenderers.ErrorNotificationRendererType is not null)
            {
                var notificationError = new NotificationRecord(
                    NotificationKey.NewKey(),
                    "Rename Action",
                    _luthetusCommonComponentRenderers.ErrorNotificationRendererType,
                    new Dictionary<string, object?>
                    {
                        {
                            nameof(IErrorNotificationRendererType.Message),
                            $"ERROR: {e.Message}"
                        },
                    },
                    TimeSpan.FromSeconds(15),
                    true,
                    IErrorNotificationRendererType.CSS_CLASS_STRING);

                dispatcher.Dispatch(new NotificationRegistry.RegisterAction(notificationError));
            }

            onAfterCompletion.Invoke();
            return null;
        }

        onAfterCompletion.Invoke();

        return new AbsolutePath(
            destinationAbsolutePathString,
            sourceAbsolutePath.IsDirectory,
            _environmentProvider);
    }

    private void PerformRemoveCSharpProjectReferenceFromSolutionAction(
        TreeViewSolution treeViewSolution,
        TreeViewNamespacePath projectNode,
        TerminalSession terminalSession,
        IDispatcher dispatcher,
        Func<Task> onAfterCompletion)
    {
        _backgroundTaskService.Enqueue(
            BackgroundTaskKey.NewKey(),
            CommonBackgroundTaskWorker.Queue.Key,
            "Remove C# Project Reference from Solution Action",
            async () =>
            {
                var workingDirectory =
                    treeViewSolution.Item.NamespacePath.AbsolutePath.ParentDirectory!;

                var removeCSharpProjectReferenceFromSolutionFormattedCommand =
                    DotNetCliFacts.FormatRemoveCSharpProjectReferenceFromSolutionAction(
                        treeViewSolution.Item.NamespacePath.AbsolutePath.FormattedInput,
                        projectNode.Item.AbsolutePath.FormattedInput);

                var removeCSharpProjectReferenceFromSolutionCommand = new TerminalCommand(
                    TerminalCommandKey.NewKey(),
                    removeCSharpProjectReferenceFromSolutionFormattedCommand,
                    workingDirectory.FormattedInput,
                    CancellationToken.None,
                    async () => await onAfterCompletion.Invoke());

                await terminalSession.EnqueueCommandAsync(
                    removeCSharpProjectReferenceFromSolutionCommand);
            });
    }

    public void PerformAddProjectToProjectReferenceAction(
        TreeViewNamespacePath projectReceivingReference,
        TerminalSession terminalSession,
        IDispatcher dispatcher,
        Func<Task> onAfterCompletion)
    {
        _backgroundTaskService.Enqueue(
            BackgroundTaskKey.NewKey(),
            CommonBackgroundTaskWorker.Queue.Key,
            "Add Project Reference to Project",
            () =>
            {
                var requestInputFileStateFormAction = new InputFileRegistry.RequestInputFileStateFormAction(
                    $"Add Project reference to {projectReceivingReference.Item.AbsolutePath.NameWithExtension}",
                    async referencedProject =>
                    {
                        if (referencedProject is null)
                            return;

                        var formattedCommand = DotNetCliFacts.FormatAddProjectToProjectReference(
                            projectReceivingReference.Item.AbsolutePath.FormattedInput,
                            referencedProject.FormattedInput);

                        var addProjectToProjectReferenceTerminalCommand = new TerminalCommand(
                            TerminalCommandKey.NewKey(),
                            formattedCommand,
                            null,
                            CancellationToken.None,
                            async () =>
                            {
                                var notificationInformative = new NotificationRecord(
                                    NotificationKey.NewKey(),
                                    "Add Project Reference",
                                    _luthetusCommonComponentRenderers.InformativeNotificationRendererType,
                                    new Dictionary<string, object?>
                                    {
                                        {
                                            nameof(IInformativeNotificationRendererType.Message),
                                            $"Modified {projectReceivingReference.Item.AbsolutePath.NameWithExtension} to have a reference to {referencedProject.NameWithExtension}"
                                        },
                                    },
                                    TimeSpan.FromSeconds(7),
                                    true,
                                    null);

                                dispatcher.Dispatch(new NotificationRegistry.RegisterAction(notificationInformative));

                                await onAfterCompletion.Invoke();
                            });

                        await terminalSession.EnqueueCommandAsync(addProjectToProjectReferenceTerminalCommand);
                    },
                    afp =>
                    {
                        if (afp is null || afp.IsDirectory)
                            return Task.FromResult(false);

                        return Task.FromResult(
                            afp.ExtensionNoPeriod.EndsWith(ExtensionNoPeriodFacts.C_SHARP_PROJECT));
                    },
                    new[]
                    {
                        new InputFilePattern(
                            "C# Project",
                            afp => afp.ExtensionNoPeriod.EndsWith(ExtensionNoPeriodFacts.C_SHARP_PROJECT))
                    }.ToImmutableArray());

                dispatcher.Dispatch(requestInputFileStateFormAction);

                return Task.CompletedTask;
            });
    }

    public void PerformRemoveProjectToProjectReferenceAction(
        TreeViewCSharpProjectToProjectReference treeViewCSharpProjectToProjectReference,
        TerminalSession terminalSession,
        IDispatcher dispatcher,
        Func<Task> onAfterCompletion)
    {
        _backgroundTaskService.Enqueue(
            BackgroundTaskKey.NewKey(),
            CommonBackgroundTaskWorker.Queue.Key,
            "Remove Project Reference to Project",
            async () =>
            {
                var formattedCommand = DotNetCliFacts.FormatRemoveProjectToProjectReference(
                    treeViewCSharpProjectToProjectReference.Item.ModifyProjectNamespacePath.AbsolutePath.FormattedInput,
                    treeViewCSharpProjectToProjectReference.Item.ReferenceProjectAbsolutePath.FormattedInput);

                var removeProjectToProjectReferenceTerminalCommand = new TerminalCommand(
                    TerminalCommandKey.NewKey(),
                    formattedCommand,
                    null,
                    CancellationToken.None,
                    async () =>
                    {
                        var notificationInformative = new NotificationRecord(
                            NotificationKey.NewKey(),
                            "Remove Project Reference",
                            _luthetusCommonComponentRenderers.InformativeNotificationRendererType,
                            new Dictionary<string, object?>
                            {
                                {
                                    nameof(IInformativeNotificationRendererType.Message),
                                    $"Modified {treeViewCSharpProjectToProjectReference.Item.ModifyProjectNamespacePath.AbsolutePath.NameWithExtension} to have a reference to {treeViewCSharpProjectToProjectReference.Item.ReferenceProjectAbsolutePath.NameWithExtension}"
                                },
                            },
                            TimeSpan.FromSeconds(7),
                            true,
                            null);

                        dispatcher.Dispatch(new NotificationRegistry.RegisterAction(notificationInformative));

                        await onAfterCompletion.Invoke();
                    });

                await terminalSession.EnqueueCommandAsync(removeProjectToProjectReferenceTerminalCommand);
            });
    }

    public void PerformMoveProjectToSolutionFolderAction(
        TreeViewSolution treeViewSolution,
        TreeViewNamespacePath treeViewProjectToMove,
        string solutionFolderPath,
        TerminalSession terminalSession,
        IDispatcher dispatcher,
        Func<Task> onAfterCompletion)
    {
        _backgroundTaskService.Enqueue(
            BackgroundTaskKey.NewKey(),
            CommonBackgroundTaskWorker.Queue.Key,
            "Move Project to Solution Folder",
            () =>
            {
                var formattedCommand = DotNetCliFacts.FormatMoveProjectToSolutionFolder(
                    treeViewSolution.Item.NamespacePath.AbsolutePath.FormattedInput,
                    treeViewProjectToMove.Item.AbsolutePath.FormattedInput,
                    solutionFolderPath);

                var moveProjectToSolutionFolderTerminalCommand = new TerminalCommand(
                    TerminalCommandKey.NewKey(),
                    formattedCommand,
                    null,
                    CancellationToken.None,
                    async () =>
                    {
                        var notificationInformative = new NotificationRecord(
                            NotificationKey.NewKey(),
                            "Move Project To Solution Folder",
                            _luthetusCommonComponentRenderers.InformativeNotificationRendererType,
                            new Dictionary<string, object?>
                            {
                                {
                                    nameof(IInformativeNotificationRendererType.Message),
                                    $"Moved {treeViewProjectToMove.Item.AbsolutePath.NameWithExtension} to the Solution Folder path: {solutionFolderPath}"
                                },
                            },
                            TimeSpan.FromSeconds(7),
                            true,
                            null);

                        dispatcher.Dispatch(new NotificationRegistry.RegisterAction(notificationInformative));

                        await onAfterCompletion.Invoke();
                    });

                PerformRemoveCSharpProjectReferenceFromSolutionAction(
                    treeViewSolution,
                    treeViewProjectToMove,
                    terminalSession,
                    dispatcher,
                    async () => await terminalSession.EnqueueCommandAsync(moveProjectToSolutionFolderTerminalCommand));

                return Task.CompletedTask;
            });
    }

    public void PerformRemoveNuGetPackageReferenceFromProjectAction(
        NamespacePath modifyProjectNamespacePath,
        TreeViewCSharpProjectNugetPackageReference treeViewCSharpProjectNugetPackageReference,
        TerminalSession terminalSession,
        IDispatcher dispatcher,
        Func<Task> onAfterCompletion)
    {
        _backgroundTaskService.Enqueue(
            BackgroundTaskKey.NewKey(),
            CommonBackgroundTaskWorker.Queue.Key,
            "Remove NuGet Package Reference from Project",
            async () =>
            {
                var formattedCommand = DotNetCliFacts.FormatRemoveNugetPackageReferenceFromProject(
                    modifyProjectNamespacePath.AbsolutePath.FormattedInput,
                    treeViewCSharpProjectNugetPackageReference.Item.LightWeightNugetPackageRecord.Id);

                var removeNugetPackageReferenceFromProjectTerminalCommand = new TerminalCommand(
                    TerminalCommandKey.NewKey(),
                    formattedCommand,
                    null,
                    CancellationToken.None,
                    async () =>
                    {
                        var notificationInformative = new NotificationRecord(
                            NotificationKey.NewKey(),
                            "Remove Project Reference",
                            _luthetusCommonComponentRenderers.InformativeNotificationRendererType,
                            new Dictionary<string, object?>
                            {
                                {
                                    nameof(IInformativeNotificationRendererType.Message),
                                    $"Modified {modifyProjectNamespacePath.AbsolutePath.NameWithExtension} to NOT have a reference to {treeViewCSharpProjectNugetPackageReference.Item.LightWeightNugetPackageRecord.Id}"
                                },
                            },
                            TimeSpan.FromSeconds(7),
                            true,
                            null);

                        dispatcher.Dispatch(new NotificationRegistry.RegisterAction(notificationInformative));

                        await onAfterCompletion.Invoke();
                    });

                await terminalSession.EnqueueCommandAsync(removeNugetPackageReferenceFromProjectTerminalCommand);
            });
    }

    /// <summary>
    /// Looking into copying and pasting a directory
    /// https://stackoverflow.com/questions/58744/copy-the-entire-contents-of-a-directory-in-c-sharp
    /// </summary>
    public static DirectoryInfo CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
    {
        var newDirectoryInfo = target.CreateSubdirectory(source.Name);
        foreach (var fileInfo in source.GetFiles())
            fileInfo.CopyTo(Path.Combine(newDirectoryInfo.FullName, fileInfo.Name));

        foreach (var childDirectoryInfo in source.GetDirectories())
            CopyFilesRecursively(childDirectoryInfo, newDirectoryInfo);

        return newDirectoryInfo;
    }
}