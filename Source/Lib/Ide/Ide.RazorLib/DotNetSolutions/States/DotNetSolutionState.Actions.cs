using Luthetus.Common.RazorLib.Keys.Models;
using Luthetus.CompilerServices.Lang.DotNetSolution.Models;
using Luthetus.Ide.RazorLib.DotNetSolutions.Models;

namespace Luthetus.Ide.RazorLib.DotNetSolutions.States;

public partial record DotNetSolutionState
{
    public record RegisterAction(
    	DotNetSolutionModel DotNetSolutionModel,
    	DotNetSolutionIdeApi DotNetSolutionApi);
    	
    public record DisposeAction(
	    Key<DotNetSolutionModel> DotNetSolutionModelKey,
	    DotNetSolutionIdeApi DotNetSolutionApi);
    
	public record StateHasChanged;
}