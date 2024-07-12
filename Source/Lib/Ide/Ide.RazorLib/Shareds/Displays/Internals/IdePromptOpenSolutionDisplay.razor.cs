using Microsoft.AspNetCore.Components;
using Luthetus.Common.RazorLib.FileSystems.Models;
using Luthetus.Ide.RazorLib.BackgroundTasks.Models;

namespace Luthetus.Ide.RazorLib.Shareds.Displays.Internals;

public partial class IdePromptOpenSolutionDisplay : ComponentBase
{
    [Inject]
    private IdeBackgroundTaskApi IdeBackgroundTaskApi { get; set; } = null!;

    [Parameter, EditorRequired]
    public IAbsolutePath AbsolutePath { get; set; } = null!;

    private void OpenSolutionOnClick()
    {
        IdeBackgroundTaskApi.DotNetSolution.SetDotNetSolution(AbsolutePath);
    }
}