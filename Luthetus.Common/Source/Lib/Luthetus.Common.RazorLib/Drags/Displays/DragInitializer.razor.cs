using Fluxor;
using Fluxor.Blazor.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Luthetus.Common.RazorLib.Reactives.Models;

namespace Luthetus.Common.RazorLib.Drags.Displays;

public partial class DragInitializer : FluxorComponent
{
    [Inject]
    private IState<DragState> DragStateWrap { get; set; } = null!;
    [Inject]
    private IDispatcher Dispatcher { get; set; } = null!;

    private string StyleCss => DragStateWrap.Value.ShouldDisplay
        ? string.Empty
        : "display: none;";

    /// <summary>
    /// Preferably the throttling logic here would be moved out of the drag initializer itself so one can choose to add it themselves, or take the full stream.
    /// </summary>
    private IThrottle _throttleDispatchSetDragStateActionOnMouseMove = new Throttle(IThrottle.DefaultThrottleTimeSpan);

    private DragState.WithAction ConstructClearDragStateAction()
    {
        return new DragState.WithAction(inState => inState with
        {
            ShouldDisplay = false,
            MouseEventArgs = null,
			PolymorphicDraggable = null,
        });
    }

    private async Task DispatchSetDragStateActionOnMouseMoveAsync(MouseEventArgs mouseEventArgs)
    {
        _throttleDispatchSetDragStateActionOnMouseMove.PushEvent(_ =>
        {
            if ((mouseEventArgs.Buttons & 1) != 1)
            {
                Dispatcher.Dispatch(ConstructClearDragStateAction());
            }
            else
            {
                Dispatcher.Dispatch(new DragState.WithAction(inState => inState with
                {
                    ShouldDisplay = true,
                    MouseEventArgs = mouseEventArgs,
                }));
            }

            return Task.CompletedTask;
        });
    }

    private Task DispatchSetDragStateActionOnMouseUp()
    {
		var dragState = DragStateWrap.Value;

        _throttleDispatchSetDragStateActionOnMouseMove.PushEvent(async _ =>
        {
            Dispatcher.Dispatch(ConstructClearDragStateAction());
        });

		return Task.CompletedTask;
    }
}