﻿using Luthetus.Common.RazorLib.WatchWindows.Models;

namespace Luthetus.Common.Tests.Basis.WatchWindows.Models;

/// <summary>
/// <see cref="TreeViewInterfaceImplementation"/>
/// </summary>
public class TreeViewInterfaceImplementationTests
{
    /// <summary>
    /// <see cref="TreeViewInterfaceImplementation(WatchWindowObject, bool, bool, RazorLib.ComponentRenderers.Models.ILuthetusCommonComponentRenderers)"/>
    /// </summary>
    [Fact]
    public void Constructor()
    {
        WatchWindowsTestsHelper.InitializeWatchWindowsTests(
            out var johnDoe,
            out var janeDoe,
            out var bobSmith,
            out var commonComponentRenderers);

        var firstRelative = johnDoe.Relatives.First();

        var interfaceImplementationWatchWindowObject = new WatchWindowObject(
            firstRelative,
            firstRelative.GetType(),
            "InterfaceImplementation",
            false);

        var isExpandable = true;
        var isExpanded = false;

        var treeViewInterfaceImplementation = new TreeViewInterfaceImplementation(
            interfaceImplementationWatchWindowObject,
            isExpandable,
            isExpanded,
            commonComponentRenderers);

        Assert.Equal(interfaceImplementationWatchWindowObject, treeViewInterfaceImplementation.Item);
        Assert.Equal(isExpandable, treeViewInterfaceImplementation.IsExpandable);
        Assert.Equal(isExpanded, treeViewInterfaceImplementation.IsExpanded);
    }

    /// <summary>
    /// <see cref="TreeViewInterfaceImplementation.GetTreeViewRenderer()"/>
    /// </summary>
    [Fact]
    public void GetTreeViewRenderer()
    {
        WatchWindowsTestsHelper.InitializeWatchWindowsTests(
            out var johnDoe,
            out var janeDoe,
            out var bobSmith,
            out var commonComponentRenderers);

        var firstRelative = johnDoe.Relatives.First();

        var interfaceImplementationWatchWindowObject = new WatchWindowObject(
            firstRelative,
            firstRelative.GetType(),
            "InterfaceImplementation",
            false);

        var isExpandable = true;
        var isExpanded = false;

        var treeViewInterfaceImplementation = new TreeViewInterfaceImplementation(
            interfaceImplementationWatchWindowObject,
            isExpandable,
            isExpanded,
            commonComponentRenderers);

        var treeViewRenderer = treeViewInterfaceImplementation.GetTreeViewRenderer();

        Assert.Equal(
            commonComponentRenderers.LuthetusCommonTreeViews.TreeViewInterfaceImplementationRenderer,
            treeViewRenderer.DynamicComponentType);

        Assert.NotNull(treeViewRenderer.DynamicComponentParameters);

        var parameter = treeViewRenderer.DynamicComponentParameters!.Single();

        Assert.Equal(nameof(TreeViewInterfaceImplementation), parameter.Key);
        Assert.Equal(treeViewInterfaceImplementation, parameter.Value);
    }
}