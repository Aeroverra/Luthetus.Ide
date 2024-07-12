using Fluxor;
using System.Collections.Immutable;
using Luthetus.Common.RazorLib.TreeViews.Models;
using Luthetus.Common.RazorLib.Keys.Models;
using Luthetus.TextEditor.RazorLib.TextEditors.Models;
using Luthetus.CompilerServices.Lang.DotNetSolution.Models;
using Luthetus.Ide.RazorLib.BackgroundTasks.Models;
using Luthetus.Ide.RazorLib.InputFiles.Models;

namespace Luthetus.Ide.RazorLib.DotNetSolutions.States;

[FeatureState]
public partial record DotNetSolutionState(
    Key<DotNetSolutionModel>? DotNetSolutionModelKey,
    int IsExecutingAsyncTaskLinks)
{
    public static readonly Key<TreeViewContainer> TreeViewSolutionExplorerStateKey = Key<TreeViewContainer>.NewKey();

    private DotNetSolutionState() : this(Key<DotNetSolutionModel>.Empty, 0)
    {
    }

    public ImmutableList<DotNetSolutionModel> DotNetSolutionsList { get; init; } = ImmutableList<DotNetSolutionModel>.Empty;

    public DotNetSolutionModel? DotNetSolutionModel => DotNetSolutionsList.FirstOrDefault(x =>
        x.Key == DotNetSolutionModelKey);

    public static void ShowInputFile(IdeBackgroundTaskApi ideBackgroundTaskApi)
    {
        ideBackgroundTaskApi.InputFile.RequestInputFileStateForm(
            "Solution Explorer",
            absolutePath =>
            {
                if (absolutePath is not null)
                    ideBackgroundTaskApi.DotNetSolution.SetDotNetSolution(absolutePath);

				return Task.CompletedTask;
            },
            absolutePath =>
            {
                if (absolutePath is null || absolutePath.IsDirectory)
                    return Task.FromResult(false);

                return Task.FromResult(
                    absolutePath.ExtensionNoPeriod == ExtensionNoPeriodFacts.DOT_NET_SOLUTION);
            },
            new[]
            {
                new InputFilePattern(
                    ".NET Solution",
                    absolutePath => absolutePath.ExtensionNoPeriod == ExtensionNoPeriodFacts.DOT_NET_SOLUTION)
            }.ToImmutableArray());
    }
}