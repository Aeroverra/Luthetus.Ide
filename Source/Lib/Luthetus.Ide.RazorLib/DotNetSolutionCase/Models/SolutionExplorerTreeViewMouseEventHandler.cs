﻿using Fluxor;
using Luthetus.Common.RazorLib.Commands.Models;
using Luthetus.Common.RazorLib.TreeView.Models;
using Luthetus.Ide.RazorLib.EditorCase.States;
using Luthetus.Ide.RazorLib.TreeViewImplementationsCase.Models;

namespace Luthetus.Ide.RazorLib.DotNetSolutionCase.Models;

public class SolutionExplorerTreeViewMouseEventHandler : TreeViewMouseEventHandler
{
    private readonly EditorSync _editorSync;

    public SolutionExplorerTreeViewMouseEventHandler(
        EditorSync editorSync,
        ITreeViewService treeViewService)
        : base(treeViewService)
    {
        _editorSync = editorSync;
    }

    public override void OnDoubleClick(TreeViewCommandParameter treeViewCommandParameter)
    {
        base.OnDoubleClick(treeViewCommandParameter);

        if (treeViewCommandParameter.TargetNode is not TreeViewNamespacePath treeViewNamespacePath)
            return;

        _editorSync.Dispatcher.Dispatch(new EditorState.OpenInEditorAction(
            _editorSync,
            treeViewNamespacePath.Item.AbsolutePath,
            true));
    }
}