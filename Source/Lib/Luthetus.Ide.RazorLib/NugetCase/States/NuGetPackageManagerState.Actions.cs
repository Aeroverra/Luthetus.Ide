﻿using Luthetus.CompilerServices.Lang.DotNetSolution;
using Luthetus.Ide.RazorLib.NugetCase.Models;
using System.Collections.Immutable;

namespace Luthetus.Ide.RazorLib.NugetCase.States;

public partial record NuGetPackageManagerState
{
    public record SetSelectedProjectToModifyAction(IDotNetProject? SelectedProjectToModify);
    public record SetNugetQueryAction(string NugetQuery);
    public record SetIncludePrereleaseAction(bool IncludePrerelease);
    public record SetMostRecentQueryResultAction(ImmutableArray<NugetPackageRecord> QueryResult);
}