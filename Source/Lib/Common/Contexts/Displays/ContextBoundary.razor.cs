using System.Collections.Immutable;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Fluxor;
using Luthetus.Common.RazorLib.Commands.Models;
using Luthetus.Common.RazorLib.Contexts.States;
using Luthetus.Common.RazorLib.Contexts.Models;
using Luthetus.Common.RazorLib.Keymaps.Models;
using Luthetus.Common.RazorLib.Keys.Models;
using Luthetus.Common.RazorLib.Outlines.States;

namespace Luthetus.Common.RazorLib.Contexts.Displays;

public partial class ContextBoundary : ComponentBase
{
    [Inject]
    private IDispatcher Dispatcher { get; set; } = null!;
    /// <summary>
    /// Warning: Do not take lightly a future decision to have this type
    ///          inherit FluxorComponent without noting that this injection will
    ///          cause re-renders whenever the context state is changed.
    /// </summary>
    [Inject]
    private IState<ContextState> ContextStateWrap { get; set; } = null!;
    [Inject]
    private IState<OutlineState> OutlineStateWrap { get; set; } = null!;

    [CascadingParameter]
    public ContextBoundary? ParentContextBoundary { get; set; }

    [Parameter, EditorRequired]
    public ContextRecord ContextRecord { get; set; } = null!;
    [Parameter, EditorRequired]
    public RenderFragment ChildContent { get; set; } = null!;

    [Parameter]
    public string ClassCssString { get; set; } = null!;
    [Parameter]
    public string StyleCssString { get; set; } = null!;
    [Parameter]
    public int TabIndex { get; set; } = -1;
    
    private int _shouldRemoveOutlineCount;

    public void DispatchSetActiveContextStatesAction(List<Key<ContextRecord>> contextRecordKeyList)
    {
        contextRecordKeyList.Add(ContextRecord.ContextKey);

        if (ParentContextBoundary is not null)
            ParentContextBoundary.DispatchSetActiveContextStatesAction(contextRecordKeyList);
        else
            Dispatcher.Dispatch(new ContextState.SetFocusedContextHeirarchyAction(new(contextRecordKeyList.ToImmutableArray())));
    }

    public void HandleOnFocusIn(bool shouldShowOutline)
    {
    	// TODO: There is a worry that the onfocusin could be redundantly setting the already
    	//       existing 'FocusedContextHeirarchy' again, enough to where an optimization
    	//       that checks to see if it already is the 'FocusedContextHeirarchy' would be of
    	//       notable benefit.
    	//
    	if (ContextStateWrap.Value.FocusedContextHeirarchy.NearestAncestorKey != ContextRecord.ContextKey &&
    	    shouldShowOutline)
    	{
    	    Dispatcher.Dispatch(new OutlineState.SetOutlineAction(
    	    	ContextRecord.ContextElementId,
    	    	null,
    	    	true));
    	    	
    	    _shouldRemoveOutlineCount = 1;
    	}
    	
        DispatchSetActiveContextStatesAction(new());
    }
    
    public void HandleOnFocusOut()
    {
    	Dispatcher.Dispatch(new OutlineState.SetOutlineAction(
	    	null,
	    	null,
	    	false));
    }
    
    private void HandleOnClick()
    {
    	if (OutlineStateWrap.Value.ElementId == ContextRecord.ContextElementId &&
    		_shouldRemoveOutlineCount == 0)
    	{
    		Dispatcher.Dispatch(new OutlineState.SetOutlineAction(
		    	null,
		    	null,
		    	false));
    	}
    	else if (_shouldRemoveOutlineCount == 1)
    	{
    		_shouldRemoveOutlineCount--;
    	}
    }

    public async Task HandleOnKeyDownAsync(KeyboardEventArgs keyboardEventArgs)
    {
        if (keyboardEventArgs.Key == "Shift" ||
            keyboardEventArgs.Key == "Control" ||
            keyboardEventArgs.Key == "Alt" ||
            keyboardEventArgs.Key == "Meta")
        {
            return;
        }

        await HandleKeymapArgumentAsync(keyboardEventArgs.ToKeymapArgument()).ConfigureAwait(false);
    }

    public async Task HandleKeymapArgumentAsync(KeymapArgument keymapArgument)
    {
        var success = ContextRecord.Keymap.Map.TryGetValue(keymapArgument, out var command);

        if (success && command is not null)
            await command.CommandFunc(new CommonCommandArgs()).ConfigureAwait(false);
        else if (ParentContextBoundary is not null)
            await ParentContextBoundary.HandleKeymapArgumentAsync(keymapArgument).ConfigureAwait(false);
    }

    public ImmutableArray<Key<ContextRecord>> GetContextBoundaryHeirarchy(List<Key<ContextRecord>> contextRecordKeyList)
    {
        contextRecordKeyList.Add(ContextRecord.ContextKey);

        if (ParentContextBoundary is not null)
            return ParentContextBoundary.GetContextBoundaryHeirarchy(contextRecordKeyList);
        else
            return contextRecordKeyList.ToImmutableArray();
    }
}