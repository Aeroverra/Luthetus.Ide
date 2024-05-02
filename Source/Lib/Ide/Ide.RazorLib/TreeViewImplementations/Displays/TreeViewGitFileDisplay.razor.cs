using Luthetus.Ide.RazorLib.ComponentRenderers.Models;
using Luthetus.Ide.RazorLib.TreeViewImplementations.Models;
using Microsoft.AspNetCore.Components;

namespace Luthetus.Ide.RazorLib.TreeViewImplementations.Displays;

public partial class TreeViewGitFileDisplay : ComponentBase, ITreeViewGitFileRendererType
{
    [Parameter, EditorRequired]
    public TreeViewGitFile TreeViewGitFile { get; set; } = null!;
}