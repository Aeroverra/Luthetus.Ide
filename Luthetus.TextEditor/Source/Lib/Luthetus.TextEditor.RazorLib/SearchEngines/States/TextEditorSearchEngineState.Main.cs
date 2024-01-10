﻿using Fluxor;
using Luthetus.Common.RazorLib.Keys.Models;
using Luthetus.TextEditor.RazorLib.SearchEngines.Models;
using System.Collections.Immutable;

namespace Luthetus.TextEditor.RazorLib.SearchEngines.States;

/// <summary>
/// Keep the <see cref="TextEditorSearchEngineState"/> as a class
/// as to avoid record value comparisons when Fluxor checks
/// if the <see cref="FeatureStateAttribute"/> has been replaced.
/// </summary>
[FeatureState]
public partial class TextEditorSearchEngineState
{
    public TextEditorSearchEngineState()
    {
        SearchEngineList = ImmutableList<ITextEditorSearchEngine>.Empty;
        SearchQuery = string.Empty;
    }

	public TextEditorSearchEngineState(
        ImmutableList<ITextEditorSearchEngine> searchEngineList,
        string searchQuery)
    {
        SearchEngineList = searchEngineList;
        SearchQuery = searchQuery;
    }

    public ImmutableList<ITextEditorSearchEngine> SearchEngineList { get; init; }
    public string SearchQuery { get; init; }
}