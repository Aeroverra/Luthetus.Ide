﻿using Luthetus.Common.RazorLib.ComponentRenderers.Models;
using Luthetus.Common.RazorLib.Namespaces.Models;
using Luthetus.Common.RazorLib.WatchWindows.Models;
using Luthetus.Common.RazorLib.FileSystems.Models;
using Luthetus.Common.RazorLib.TreeViews.Models;
using Luthetus.Common.RazorLib.Keys.Models;
using Luthetus.Ide.RazorLib.ComponentRenderers.Models;
using Luthetus.TextEditor.RazorLib.TextEditors.Models;

namespace Luthetus.Ide.RazorLib.TreeViewImplementations.Models;

public class TreeViewNamespacePath : TreeViewWithType<NamespacePath>
{
    public TreeViewNamespacePath(
            NamespacePath namespacePath,
            ILuthetusIdeComponentRenderers ideComponentRenderers,
            ILuthetusCommonComponentRenderers commonComponentRenderers,
            IFileSystemProvider fileSystemProvider,
            IEnvironmentProvider environmentProvider,
            bool isExpandable,
            bool isExpanded)
        : base(namespacePath, isExpandable, isExpanded)
    {
        IdeComponentRenderers = ideComponentRenderers;
        CommonComponentRenderers = commonComponentRenderers;
        FileSystemProvider = fileSystemProvider;
        EnvironmentProvider = environmentProvider;
    }

    public ILuthetusIdeComponentRenderers IdeComponentRenderers { get; }
    public ILuthetusCommonComponentRenderers CommonComponentRenderers { get; }
    public IFileSystemProvider FileSystemProvider { get; }
    public IEnvironmentProvider EnvironmentProvider { get; }

    public override bool Equals(object? obj)
    {
        if (obj is not TreeViewNamespacePath treeViewSolutionExplorer)
            return false;

        return treeViewSolutionExplorer.Item.AbsolutePath.FormattedInput ==
               Item.AbsolutePath.FormattedInput;
    }

    public override int GetHashCode() => Item.AbsolutePath.FormattedInput.GetHashCode();

    public override TreeViewRenderer GetTreeViewRenderer()
    {
        return new TreeViewRenderer(
            IdeComponentRenderers.LuthetusIdeTreeViews.TreeViewNamespacePathRendererType,
            new Dictionary<string, object?>
            {
                {
                    nameof(ITreeViewNamespacePathRendererType.NamespacePath),
                    Item
                },
            });
    }

    public override async Task LoadChildBagAsync()
    {
        try
        {
            var newChildBag = new List<TreeViewNoType>();

            if (Item.AbsolutePath.IsDirectory)
            {
                newChildBag = await this.DirectoryLoadChildrenAsync();
            }
            else
            {
                switch (Item.AbsolutePath.ExtensionNoPeriod)
                {
                    case ExtensionNoPeriodFacts.DOT_NET_SOLUTION:
                        return;
                    case ExtensionNoPeriodFacts.C_SHARP_PROJECT:
                        newChildBag = await this.CSharpProjectLoadChildrenAsync();
                        break;
                    case ExtensionNoPeriodFacts.RAZOR_MARKUP:
                        newChildBag = await this.RazorMarkupLoadChildrenAsync();
                        break;
                }
            }

            var oldChildrenMap = ChildBag.ToDictionary(child => child);

            foreach (var newChild in newChildBag)
            {
                if (oldChildrenMap.TryGetValue(newChild, out var oldChild))
                {
                    newChild.IsExpanded = oldChild.IsExpanded;
                    newChild.IsExpandable = oldChild.IsExpandable;
                    newChild.IsHidden = oldChild.IsHidden;
                    newChild.Key = oldChild.Key;
                    newChild.ChildBag = oldChild.ChildBag;
                }
            }

            for (int i = 0; i < newChildBag.Count; i++)
            {
                var newChild = newChildBag[i];

                newChild.IndexAmongSiblings = i;
                newChild.Parent = this;
                newChild.TreeViewChangedKey = Key<TreeViewChanged>.NewKey();
            }

            ChildBag = newChildBag;
        }
        catch (Exception exception)
        {
            ChildBag = new List<TreeViewNoType>
            {
                new TreeViewException(exception, false, false, CommonComponentRenderers)
                {
                    Parent = this,
                    IndexAmongSiblings = 0,
                }
            };
        }

        TreeViewChangedKey = Key<TreeViewChanged>.NewKey();
    }

    /// <summary>
    /// This method is called on each child when loading children for a parent node.
    /// This method allows for code-behinds
    /// </summary>
    public override void RemoveRelatedFilesFromParent(List<TreeViewNoType> siblingsAndSelfTreeViews)
    {
        if (Item.AbsolutePath.ExtensionNoPeriod.EndsWith(ExtensionNoPeriodFacts.RAZOR_MARKUP))
            TreeViewHelper.RazorMarkupFindRelatedFiles(this, siblingsAndSelfTreeViews);
    }
}