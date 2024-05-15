using Luthetus.Ide.RazorLib.Gits.Models;
using System.Collections.Immutable;

namespace Luthetus.Ide.RazorLib.Gits.States;

public partial record GitState
{
    public record SetRepoAction(GitRepo? Repo);

    /// <summary>
    /// If the expected path is not the actual path, then the git file list will NOT be changed.
    /// </summary>
    public record SetFileListAction(
        GitRepo Repo,
        ImmutableList<GitFile> UntrackedFileList,
        ImmutableList<GitFile> StagedFileList,
        ImmutableList<GitFile> UnstagedFileList);

    public record SetOriginAction(GitRepo Repo, string Origin);
    public record SetBranchAction(GitRepo Repo, string Branch);

    public record SetSelectedFileListAction(
        Func<ImmutableDictionary<string, GitFile>,
            ImmutableDictionary<string, GitFile>> SetSelectedFileListFunc);

    public record RefreshAction;
    public record WithAction(Func<GitState, GitState> WithFunc);
}