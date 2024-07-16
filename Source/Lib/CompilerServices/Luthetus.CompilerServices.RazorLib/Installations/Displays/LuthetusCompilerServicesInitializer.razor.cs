using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Fluxor;
using Luthetus.Common.RazorLib.Panels.States;
using Luthetus.Common.RazorLib.BackgroundTasks.Models;
using Luthetus.Common.RazorLib.Keys.Models;
using Luthetus.Common.RazorLib.Panels.Models;
using Luthetus.Common.RazorLib.Dynamics.Models;
using Luthetus.Common.RazorLib.Contexts.Models;
using Luthetus.Common.RazorLib.Dialogs.Models;
using Luthetus.CompilerServices.RazorLib.DotNetSolutions.Displays;
using Luthetus.CompilerServices.RazorLib.Nugets.Displays;

namespace Luthetus.CompilerServices.RazorLib.Installations.Displays;

public partial class LuthetusCompilerServicesInitializer : ComponentBase
{
	[Inject]
    private IBackgroundTaskService BackgroundTaskService { get; set; } = null!;
    [Inject]
    private IDialogService DialogService { get; set; } = null!;
    [Inject]
    private IJSRuntime JsRuntime { get; set; } = null!;
    [Inject]
    private IState<PanelState> PanelStateWrap { get; set; } = null!;
	[Inject]
    private IDispatcher Dispatcher { get; set; } = null!;

	protected override async Task OnAfterRenderAsync(bool firstRender)
    {
	    // TODO: This needs to be moved to 'OnInitializedAsync' otherwise...
    	//       ...if one refreshes the application then this code, as it is now,
    	//       will not run a second time.
        if (firstRender)
        {
            BackgroundTaskService.Enqueue(
                Key<IBackgroundTask>.NewKey(),
                ContinuousBackgroundTaskWorker.GetQueueKey(),
                nameof(LuthetusCompilerServicesInitializer),
                async () =>
                {
                    InitializePanelTabs();
                });
        }

        await base.OnAfterRenderAsync(firstRender);
    }
    
    private void InitializePanelTabs()
    {
        InitializeLeftPanelTabs();
        InitializeRightPanelTabs();
        InitializeBottomPanelTabs();
    }
    
    private void InitializeLeftPanelTabs()
    {
    	var leftPanel = PanelFacts.GetTopLeftPanelGroup(PanelStateWrap.Value);
        leftPanel.Dispatcher = Dispatcher;
        
        // solutionExplorerPanel
        var solutionExplorerPanel = new Panel(
			"Solution Explorer",
			Key<Panel>.NewKey(),
			Key<IDynamicViewModel>.NewKey(),
			ContextFacts.SolutionExplorerContext.ContextKey,
			typeof(SolutionExplorerDisplay),
			null,
            Dispatcher,
            DialogService,
            JsRuntime);
        Dispatcher.Dispatch(new PanelState.RegisterPanelAction(solutionExplorerPanel));
        Dispatcher.Dispatch(new PanelState.RegisterPanelTabAction(leftPanel.Key, solutionExplorerPanel, false));
    }
    
    private void InitializeRightPanelTabs()
    {
    	var rightPanel = PanelFacts.GetTopRightPanelGroup(PanelStateWrap.Value);
        rightPanel.Dispatcher = Dispatcher;
    }
    
    private void InitializeBottomPanelTabs()
    {
    	var bottomPanel = PanelFacts.GetBottomPanelGroup(PanelStateWrap.Value);
        bottomPanel.Dispatcher = Dispatcher;
        
        // nuGetPanel
        var nuGetPanel = new Panel(
			"NuGet",
			Key<Panel>.NewKey(),
			Key<IDynamicViewModel>.NewKey(),
			ContextFacts.NuGetPackageManagerContext.ContextKey,
            typeof(NuGetPackageManager),
            null,
            Dispatcher,
            DialogService,
            JsRuntime);
        Dispatcher.Dispatch(new PanelState.RegisterPanelAction(nuGetPanel));
        Dispatcher.Dispatch(new PanelState.RegisterPanelTabAction(bottomPanel.Key, nuGetPanel, false));
    }
}