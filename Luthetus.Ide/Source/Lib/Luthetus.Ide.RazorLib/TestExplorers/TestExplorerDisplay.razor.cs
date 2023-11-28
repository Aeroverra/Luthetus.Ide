using Microsoft.AspNetCore.Components;
using System.Collections.Immutable;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using Luthetus.Ide.RazorLib.Terminals.States;
using Luthetus.Common.RazorLib.Keys.Models;
using Luthetus.Ide.RazorLib.CommandLines.Models;
using Luthetus.Ide.RazorLib.Terminals.Models;
using Luthetus.Ide.RazorLib.InputFiles.States;
using Luthetus.Ide.RazorLib.InputFiles.Models;

namespace Luthetus.Ide.RazorLib.TestExplorers;

public partial class TestExplorerDisplay : FluxorComponent
{
	[Inject]
    private IState<TerminalSessionState> TerminalSessionsStateWrap { get; set; } = null!;
	[Inject]
    private InputFileSync InputFileSync { get; set; } = null!;

	private const string DOTNET_TEST_LIST_TESTS_COMMAND = "dotnet test -t";

	private string _directoryNameForTestDiscovery = string.Empty;

    public Key<TerminalCommand> DotNetTestListTestsTerminalCommandKey { get; } = Key<TerminalCommand>.NewKey();
    public CancellationTokenSource DotNetTestListTestsCancellationTokenSource { get; set; } = new();

    private FormattedCommand FormattedCommand => DotNetCliCommandFormatter.FormatDotNetTestListTests();

	private async Task StartDotNetTestListTestsCommandOnClick()
    {
        var localFormattedCommand = FormattedCommand;

        var dotNetTestListTestsCommand = new TerminalCommand(
                DotNetTestListTestsTerminalCommandKey,
                localFormattedCommand,
                _directoryNameForTestDiscovery,
                DotNetTestListTestsCancellationTokenSource.Token,
                () => Task.CompletedTask);

            var generalTerminalSession = TerminalSessionsStateWrap.Value.TerminalSessionMap[
                TerminalSessionFacts.GENERAL_TERMINAL_SESSION_KEY];

            await generalTerminalSession
                .EnqueueCommandAsync(dotNetTestListTestsCommand);
    }

	private void RequestInputFileForTestDiscovery()
    {
        InputFileSync.RequestInputFileStateForm("Directory for Test Discovery",
            async afp =>
            {
                if (afp is null)
                    return;

                _directoryNameForTestDiscovery = afp.Value;

                await InvokeAsync(StateHasChanged);
            },
            afp =>
            {
                if (afp is null || !afp.IsDirectory)
                    return Task.FromResult(false);

                return Task.FromResult(true);
            },
            new[]
            {
                new InputFilePattern("Directory", afp => afp.IsDirectory)
            }.ToImmutableArray());
    }
}
 
    