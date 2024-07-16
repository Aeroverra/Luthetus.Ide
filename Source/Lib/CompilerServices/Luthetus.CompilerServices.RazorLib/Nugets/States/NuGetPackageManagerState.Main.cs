using System.Collections.Immutable;
using Fluxor;
using Luthetus.CompilerServices.DotNetSolution.Models.Project;
using Luthetus.CompilerServices.RazorLib.Nugets.Models;

namespace Luthetus.CompilerServices.RazorLib.Nugets.States;

[FeatureState]
public partial record NuGetPackageManagerState(
    IDotNetProject? SelectedProjectToModify,
    string NugetQuery,
    bool IncludePrerelease,
    ImmutableArray<NugetPackageRecord> QueryResultList)
{
    public NuGetPackageManagerState() : this(null, string.Empty, false, ImmutableArray<NugetPackageRecord>.Empty)
    {

    }
}