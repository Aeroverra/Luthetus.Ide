﻿using Luthetus.Common.RazorLib.Contexts.Models;
using Luthetus.Common.RazorLib.Panels.Models;
using Luthetus.Common.RazorLib.Panels.States;

namespace Luthetus.Common.Tests.Basis.Panels.States;

/// <summary>
/// <see cref="PanelsState"/>
/// </summary>
public class PanelsStateActionsTests
{
    /// <summary>
    /// <see cref="PanelsState.RegisterPanelGroupAction"/>
    /// </summary>
    [Fact]
    public void RegisterPanelGroupAction()
    {
        InitializePanelsStateActionsTests(out var panelGroup, out var panelTab);

        var registerPanelGroupAction = new PanelsState.RegisterPanelGroupAction(
            panelGroup);

        Assert.Equal(panelGroup, registerPanelGroupAction.PanelGroup);
    }

    /// <summary>
    /// <see cref="PanelsState.DisposePanelGroupAction"/>
    /// </summary>
    [Fact]
    public void DisposePanelGroupAction()
    {
        InitializePanelsStateActionsTests(out var panelGroup, out var panelTab);

        var disposePanelGroupAction = new PanelsState.DisposePanelGroupAction(
            panelGroup.Key);

        Assert.Equal(panelGroup.Key, disposePanelGroupAction.PanelGroupKey);
    }

    /// <summary>
    /// <see cref="PanelsState.RegisterPanelTabAction"/>
    /// </summary>
    [Fact]
    public void RegisterPanelTabAction()
    {
        InitializePanelsStateActionsTests(out var panelGroup, out var panelTab);

        // InsertAtIndexZero == false
        {
            var insertAtIndexZero = false;

            var registerPanelTabAction = new PanelsState.RegisterPanelTabAction(
                panelGroup.Key,
                panelTab,
                insertAtIndexZero);

            Assert.Equal(panelGroup.Key, registerPanelTabAction.PanelGroupKey);
            Assert.Equal(panelTab, registerPanelTabAction.PanelTab);
            Assert.Equal(insertAtIndexZero, registerPanelTabAction.InsertAtIndexZero);
        }

        // InsertAtIndexZero == true
        {
            var insertAtIndexZero = true;

            var registerPanelTabAction = new PanelsState.RegisterPanelTabAction(
                panelGroup.Key,
                panelTab,
                insertAtIndexZero);

            Assert.Equal(panelGroup.Key, registerPanelTabAction.PanelGroupKey);
            Assert.Equal(panelTab, registerPanelTabAction.PanelTab);
            Assert.Equal(insertAtIndexZero, registerPanelTabAction.InsertAtIndexZero);
        }
    }

    /// <summary>
    /// <see cref="PanelsState.DisposePanelTabAction"/>
    /// </summary>
    [Fact]
    public void DisposePanelTabAction()
    {
        InitializePanelsStateActionsTests(out var panelGroup, out var panelTab);

        var disposePanelTabAction = new PanelsState.DisposePanelTabAction(
            panelGroup.Key,
            panelTab.Key);

        Assert.Equal(panelGroup.Key, disposePanelTabAction.PanelGroupKey);
        Assert.Equal(panelTab.Key, disposePanelTabAction.PanelTabKey);
    }

    /// <summary>
    /// <see cref="PanelsState.SetActivePanelTabAction"/>
    /// </summary>
    [Fact]
    public void SetActivePanelTabAction()
    {
        InitializePanelsStateActionsTests(out var panelGroup, out var panelTab);

        var setActivePanelTabAction = new PanelsState.SetActivePanelTabAction(
            panelGroup.Key,
            panelTab.Key);

        Assert.Equal(panelGroup.Key, setActivePanelTabAction.PanelGroupKey);
        Assert.Equal(panelTab.Key, setActivePanelTabAction.PanelTabKey);
    }

    /// <summary>
    /// <see cref="PanelsState.SetPanelTabAsActiveByContextRecordKeyAction"/>
    /// </summary>
    [Fact]
    public void SetPanelTabAsActiveByContextRecordKeyAction()
    {
        InitializePanelsStateActionsTests(out var panelGroup, out var panelTab);

        var setPanelTabAsActiveByContextRecordKeyAction = new PanelsState.SetPanelTabAsActiveByContextRecordKeyAction(
            ContextFacts.SolutionExplorerContext.ContextKey);

        Assert.Equal(
            ContextFacts.SolutionExplorerContext.ContextKey,
            setPanelTabAsActiveByContextRecordKeyAction.ContextRecordKey);
    }

    /// <summary>
    /// <see cref="PanelsState.SetDragEventArgsAction"/>
    /// </summary>
    [Fact]
    public void SetDragEventArgsAction()
    {
        InitializePanelsStateActionsTests(out var panelGroup, out var panelTab);

        var setDragEventArgsAction = new PanelsState.SetDragEventArgsAction(
            (panelTab, panelGroup));

        Assert.NotNull(setDragEventArgsAction.DragEventArgs);

        Assert.Equal(panelTab, setDragEventArgsAction.DragEventArgs!.Value.PanelTab);
        Assert.Equal(panelGroup, setDragEventArgsAction.DragEventArgs.Value.PanelGroup);
    }

    private void InitializePanelsStateActionsTests(
        out PanelGroup samplePanelGroup,
        out IPanelTab samplePanelTab)
    {
        throw new NotImplementedException("Test was broken on (2024-04-08)");
        //     samplePanelGroup = new PanelGroup(
        //             PanelFacts.LeftPanelRecordKey,
        //             Key<Panel>.Empty,
        //             new ElementDimensions(),
        //             ImmutableArray<IPanelTab>.Empty);

        //     var leftPanelGroupWidth = samplePanelGroup.ElementDimensions.DimensionAttributeList
        //         .Single(da => da.DimensionAttributeKind == DimensionAttributeKind.Width);

        //     leftPanelGroupWidth.DimensionUnitList.AddRange(new[]
        //     {
        //         new DimensionUnit
        //         {
        //             Value = 33.3333,
        //             DimensionUnitKind = DimensionUnitKind.Percentage
        //         },
        //         new DimensionUnit
        //         {
        //             Value = ResizableColumn.RESIZE_HANDLE_WIDTH_IN_PIXELS / 2,
        //             DimensionUnitKind = DimensionUnitKind.Pixels,
        //             DimensionOperatorKind = DimensionOperatorKind.Subtract
        //         }
        //     });

        //     samplePanelTab = new Panel(
        //"Solution Explorer",
        //Key<Panel>.NewKey(),
        //Key<IDynamicViewModel>.NewKey(),
        //ContextFacts.SolutionExplorerContext.ContextKey,
        //         typeof(IconCSharpClass),
        //         new());
    }
}