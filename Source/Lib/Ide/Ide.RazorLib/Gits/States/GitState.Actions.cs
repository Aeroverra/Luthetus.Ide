﻿using Luthetus.Common.RazorLib.FileSystems.Models;
using Luthetus.Ide.RazorLib.Gits.Models;
using System.Collections.Immutable;

namespace Luthetus.Ide.RazorLib.Gits.States;

public partial record GitState
{
    /// <summary>
    /// If the expected path is not the actual path, then the git file list will NOT be changed.
    /// </summary>
    public record SetGitFileListAction(IAbsolutePath ExpectedGitFolderAbsolutePath, ImmutableList<GitFile> GitFileList);
    public record SetGitOriginAction(IAbsolutePath ExpectedGitFolderAbsolutePath, string Origin);
    public record SetGitFolderAction(IAbsolutePath GitFolderAbsolutePath);
    public record SetSelectedGitFileListAction(Func<ImmutableDictionary<string, GitFile>, ImmutableDictionary<string, GitFile>> SetSelectedGitFileListFunc);
    public record SetGitStateWithAction(Func<GitState, GitState> GitStateWithFunc);
}