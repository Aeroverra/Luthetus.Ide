using Fluxor;
using Luthetus.Ide.RazorLib.ContextCase.Models;
using Luthetus.Ide.RazorLib.ContextCase.States;
using Luthetus.Ide.RazorLib.JavaScriptObjectsCase.Models;
using Microsoft.AspNetCore.Components;
using System.Collections.Immutable;

namespace Luthetus.Ide.RazorLib.ContextCase.Displays;

public partial class ContextBoundaryOverlay : ComponentBase
{
    [Inject]
    private IDispatcher Dispatcher { get; set; } = null!;

    [Parameter, EditorRequired]
    public ContextRecord ContextRecord { get; set; } = null!;
    [Parameter, EditorRequired]
    public ImmutableArray<ContextRecord> ContextBoundaryHeirarchy { get; set; }
    [Parameter, EditorRequired]
    public MeasuredHtmlElementDimensions MeasuredHtmlElementDimensions { get; set; } = null!;

    private string GetCssStyleString()
    {
        var width = $"width: {MeasuredHtmlElementDimensions.WidthInPixels}px;";
        var height = $"height: {MeasuredHtmlElementDimensions.HeightInPixels}px;";
        var left = $"left: {MeasuredHtmlElementDimensions.LeftInPixels}px;";
        var top = $"top: {MeasuredHtmlElementDimensions.TopInPixels}px;";
        var zIndex = $"z-index: {MeasuredHtmlElementDimensions.ZIndex};";

        return $"{width} {height} {left} {top} {zIndex}";
    }

    private void DispatchSetInspectionTargetActionOnClick()
    {
        Dispatcher.Dispatch(new ContextRegistry.SetInspectionTargetAction(
            ContextBoundaryHeirarchy));
    }
}