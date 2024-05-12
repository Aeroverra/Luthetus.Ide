﻿using Fluxor;
using Luthetus.Common.RazorLib.Keys.Models;
using Luthetus.Common.RazorLib.TreeViews.Models;
using Luthetus.Ide.RazorLib.Gits.Models;
using System.Collections.Immutable;

namespace Luthetus.Ide.RazorLib.Gits.States;

[FeatureState]
public partial record GitState(
    GitRepo? Repo,
    ImmutableList<GitFile> FileList,
    ImmutableDictionary<string, GitFile> StagedFileMap,
    ImmutableList<GitTask> ActiveTasks,
    string? Origin)
{
    public static readonly Key<TreeViewContainer> TreeViewGitChangesKey = Key<TreeViewContainer>.NewKey();

    public GitState()
        : this(null, ImmutableList<GitFile>.Empty, ImmutableDictionary<string, GitFile>.Empty, ImmutableList<GitTask>.Empty, null)
    {

    }
}