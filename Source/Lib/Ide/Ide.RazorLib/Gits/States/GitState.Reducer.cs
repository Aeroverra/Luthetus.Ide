﻿using Fluxor;
using Luthetus.Ide.RazorLib.Gits.Models;
using System.Collections.Immutable;

namespace Luthetus.Ide.RazorLib.Gits.States;

public partial record GitState
{
    public class Reducer
    {
        [ReducerMethod]
        public static GitState ReduceSetGitFileListAction(
            GitState inState,
            SetFileListAction setGitFileListAction)
        {
            if (inState.Repo != setGitFileListAction.Repo)
            {
                // Git folder was changed while the text was being parsed,
                // throw away the result since it is thereby invalid.
                return inState;
            }

            return inState with
            {
                UntrackedFileList = setGitFileListAction.UntrackedFileList,
                StagedFileList = setGitFileListAction.StagedFileList,
                UnstagedFileList = setGitFileListAction.UnstagedFileList,
            };
        }

        [ReducerMethod]
        public static GitState ReduceSetGitOriginAction(
            GitState inState,
            SetOriginAction setOriginAction)
        {
            if (inState.Repo != setOriginAction.Repo)
            {
                // Git folder was changed while the text was being parsed,
                // throw away the result since it is thereby invalid.
                return inState;
            }

            return inState with
            {
                Origin = setOriginAction.Origin
            };
        }
        
        [ReducerMethod]
        public static GitState ReduceSetBranchAction(
            GitState inState,
            SetBranchAction setBranchAction)
        {
            if (inState.Repo != setBranchAction.Repo)
            {
                // Git folder was changed while the text was being parsed,
                // throw away the result since it is thereby invalid.
                return inState;
            }

            return inState with
            {
                Branch = setBranchAction.Branch
            };
        }
        
        [ReducerMethod]
        public static GitState ReduceSetGitFolderAction(
            GitState inState,
            SetRepoAction setRepoAction)
        {
            return inState with
            {
                Repo = setRepoAction.Repo,
                UntrackedFileList = ImmutableList<GitFile>.Empty,
                StagedFileList = ImmutableList<GitFile>.Empty,
                UnstagedFileList = ImmutableList<GitFile>.Empty,
                SelectedFileList = ImmutableList<GitFile>.Empty,
                ActiveTasks = ImmutableList<GitTask>.Empty,
            };
        }

        [ReducerMethod]
        public static GitState ReduceSetGitStateWithAction(
            GitState inState,
            WithAction withAction)
        {
            return withAction.WithFunc.Invoke(inState);
        }
    }
}