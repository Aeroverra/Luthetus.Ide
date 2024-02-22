﻿using System.Collections.Immutable;
using Luthetus.CompilerServices.Lang.DotNetSolution.Models.Project;
using Luthetus.Ide.RazorLib.Nugets.Models;
using Luthetus.Ide.RazorLib.Nugets.States;

namespace Luthetus.Ide.Tests.Basis.Nugets.States;

/// <summary>
/// <see cref="NuGetPackageManagerState"/>
/// </summary>
public class NuGetPackageManagerStateMainTests
{
    /// <summary>
    /// <see cref="NuGetPackageManagerState(IDotNetProject?, string, bool, ImmutableArray{NugetPackageRecord})"/>
    /// <br/>----<br/>
    /// <see cref="NuGetPackageManagerState.SelectedProjectToModify"/>
    /// <see cref="NuGetPackageManagerState.NugetQuery"/>
    /// <see cref="NuGetPackageManagerState.IncludePrerelease"/>
    /// <see cref="NuGetPackageManagerState.QueryResultList"/>
    /// </summary>
    [Fact]
    public void Constructor()
    {
        throw new NotImplementedException();
    }
}