﻿using System.Collections.Immutable;
using static Luthetus.Ide.RazorLib.DotNetSolutions.States.DotNetSolutionState;
using Luthetus.Ide.RazorLib.TreeViewImplementations.Models;
using Luthetus.Common.RazorLib.TreeViews.Models;
using Luthetus.Common.RazorLib.Namespaces.Models;
using Luthetus.CompilerServices.Lang.DotNetSolution.CSharp;
using Luthetus.TextEditor.RazorLib.Lexes.Models;
using Luthetus.TextEditor.RazorLib.TextEditors.States;
using Luthetus.Common.RazorLib.FileSystems.Models;
using Luthetus.Common.RazorLib.Keys.Models;
using Luthetus.Ide.RazorLib.WebsiteProjectTemplates.Models;
using Luthetus.CompilerServices.Lang.DotNetSolution.Obsolete.RewriteForImmutability;
using Luthetus.CompilerServices.Lang.DotNetSolution.Obsolete;

namespace Luthetus.Ide.RazorLib.DotNetSolutions.States;

public partial class DotNetSolutionSync
{
    private async Task AddExistingProjectToSolutionAsync(
        Key<DotNetSolutionModel> dotNetSolutionModelKey,
        string projectTemplateShortName,
        string cSharpProjectName,
        IAbsolutePath cSharpProjectAbsolutePath,
        IEnvironmentProvider environmentProvider)
    {
        var inDotNetSolutionModel = _dotNetSolutionStateWrap.Value.DotNetSolutionsBag.FirstOrDefault(
            x => x.DotNetSolutionModelKey == dotNetSolutionModelKey);

        if (inDotNetSolutionModel is null)
            return;

        var projectTypeGuid = WebsiteProjectTemplateFacts.GetProjectTypeGuid(
            projectTemplateShortName);

        var relativePathFromSlnToProject = PathHelper.GetRelativeFromTwoAbsolutes(
            inDotNetSolutionModel.NamespacePath.AbsolutePath,
            cSharpProjectAbsolutePath,
            environmentProvider);

        var projectIdGuid = Guid.NewGuid();

        var cSharpProject = new CSharpProject(
            cSharpProjectName,
            projectTypeGuid,
            relativePathFromSlnToProject,
            projectIdGuid);

        cSharpProject.SetAbsolutePath(cSharpProjectAbsolutePath);

        var dotNetSolutionBuilder = inDotNetSolutionModel.AddDotNetProject(
            cSharpProject,
            environmentProvider);

        var outDotNetSolutionModel = dotNetSolutionBuilder.Build();

        await _fileSystemProvider.File.WriteAllTextAsync(
            inDotNetSolutionModel.NamespacePath.AbsolutePath.FormattedInput,
            inDotNetSolutionModel.SolutionFileContents);

        var solutionTextEditorModel = _textEditorService.Model.FindOrDefaultByResourceUri(
            new ResourceUri(inDotNetSolutionModel.NamespacePath.AbsolutePath.FormattedInput));

        if (solutionTextEditorModel is not null)
        {
            Dispatcher.Dispatch(new TextEditorModelState.ReloadAction(
                solutionTextEditorModel.ResourceUri,
                inDotNetSolutionModel.SolutionFileContents,
                DateTime.UtcNow));
        }

        // TODO: Putting a hack for now to overwrite if somehow model was registered already
        Dispatcher.Dispatch(ConstructModelReplacement(
            outDotNetSolutionModel.DotNetSolutionModelKey,
            outDotNetSolutionModel));

        await SetDotNetSolutionTreeViewAsync(outDotNetSolutionModel.DotNetSolutionModelKey);
    }

    private async Task SetDotNetSolutionAsync(IAbsolutePath inSolutionAbsolutePath)
    {
        var dotNetSolutionAbsolutePathString = inSolutionAbsolutePath.FormattedInput;

        var content = await _fileSystemProvider.File.ReadAllTextAsync(
            dotNetSolutionAbsolutePathString,
            CancellationToken.None);

        var solutionAbsolutePath = new AbsolutePath(
            dotNetSolutionAbsolutePathString,
            false,
            _environmentProvider);

        var solutionNamespacePath = new NamespacePath(
            string.Empty,
            solutionAbsolutePath);

        var dotNetSolution = DotNetSolutionLexer.Lex(
            content,
            solutionNamespacePath,
            _environmentProvider);

        // TODO: If somehow model was registered already this won't write the state
        Dispatcher.Dispatch(new RegisterAction(dotNetSolution, this));

        Dispatcher.Dispatch(new WithAction(
            inDotNetSolutionState => inDotNetSolutionState with
            {
                DotNetSolutionModelKey = dotNetSolution.DotNetSolutionModelKey
            }));

        // TODO: Putting a hack for now to overwrite if somehow model was registered already
        Dispatcher.Dispatch(ConstructModelReplacement(
            dotNetSolution.DotNetSolutionModelKey,
            dotNetSolution));

        await SetDotNetSolutionTreeViewAsync(dotNetSolution.DotNetSolutionModelKey);
    }

    private async Task SetDotNetSolutionTreeViewAsync(Key<DotNetSolutionModel> dotNetSolutionModelKey)
    {
        var dotNetSolutionState = _dotNetSolutionStateWrap.Value;

        var dotNetSolutionModel = dotNetSolutionState.DotNetSolutionsBag.FirstOrDefault(
            x => x.DotNetSolutionModelKey == dotNetSolutionModelKey);

        if (dotNetSolutionModel is null)
            return;

        var rootNode = new TreeViewSolution(
            dotNetSolutionModel,
            _ideComponentRenderers,
            _commonComponentRenderers,
            _fileSystemProvider,
            _environmentProvider,
            true,
            true);

        await rootNode.LoadChildBagAsync();

        if (!_treeViewService.TryGetTreeViewState(TreeViewSolutionExplorerStateKey, out _))
        {
            _treeViewService.RegisterTreeViewState(new TreeViewContainer(
                TreeViewSolutionExplorerStateKey,
                rootNode,
                rootNode,
                ImmutableList<TreeViewNoType>.Empty));
        }
        else
        {
            _treeViewService.SetRoot(TreeViewSolutionExplorerStateKey, rootNode);
            _treeViewService.SetActiveNode(TreeViewSolutionExplorerStateKey, rootNode);
        }

        if (dotNetSolutionModel is null)
            return;

        Dispatcher.Dispatch(ConstructModelReplacement(
            dotNetSolutionModel.DotNetSolutionModelKey,
            dotNetSolutionModel));
    }
}