﻿using Xunit;
using Luthetus.TextEditor.RazorLib.SearchEngines.States;
using Luthetus.TextEditor.RazorLib.SearchEngines.Models;
using System.Collections.Immutable;
using Luthetus.Common.RazorLib.Keys.Models;

namespace Luthetus.TextEditor.Tests.Basis.SearchEngines.States;

/// <summary>
/// <see cref="TextEditorSearchEngineState"/>
/// </summary>
public class TextEditorSearchEngineStateMainTests
{
    /// <summary>
    /// <see cref="TextEditorSearchEngineState()"/>
    /// <br/>----<br/>
    /// <see cref="TextEditorSearchEngineState.SearchEngineBag"/>
    /// <see cref="TextEditorSearchEngineState.ActiveSearchEngineKey"/>
    /// <see cref="TextEditorSearchEngineState.SearchQuery"/>
	/// <see cref="TextEditorSearchEngineState.GetActiveSearchEngineOrDefault()"/>
    /// </summary>
    [Fact]
	public void Constructor_A()
	{
		var searchEngineState = new TextEditorSearchEngineState();

        Assert.Equal(ImmutableList<ITextEditorSearchEngine>.Empty, searchEngineState.SearchEngineBag);
        Assert.Equal(Key<ITextEditorSearchEngine>.Empty, searchEngineState.ActiveSearchEngineKey);
        Assert.Equal(string.Empty, searchEngineState.SearchQuery);
        Assert.Null(searchEngineState.GetActiveSearchEngineOrDefault());
	}

    /// <summary>
    /// <see cref="TextEditorSearchEngineState(ImmutableList{ITextEditorSearchEngine}, Key{ITextEditorSearchEngine}, string)"/>
    /// <br/>----<br/>
    /// <see cref="TextEditorSearchEngineState.SearchEngineBag"/>
    /// <see cref="TextEditorSearchEngineState.ActiveSearchEngineKey"/>
    /// <see cref="TextEditorSearchEngineState.SearchQuery"/>
	/// <see cref="TextEditorSearchEngineState.GetActiveSearchEngineOrDefault()"/>
    /// </summary>
    [Fact]
	public void Constructor_B()
	{
		var searchEngine = new SearchEngineOverRegisteredViewModels();
		var searchEngineBag = new ITextEditorSearchEngine[] { searchEngine }.ToImmutableList();
		var searchEngineKey = searchEngine.SearchEngineKey;
        var searchQuery = "AlphabetSoup";

        var searchEngineState = new TextEditorSearchEngineState(
            searchEngineBag,
            searchEngineKey,
            searchQuery);

        Assert.Equal(searchEngineBag, searchEngineState.SearchEngineBag);
        Assert.Equal(searchEngineKey, searchEngineState.ActiveSearchEngineKey);
        Assert.Equal(searchQuery, searchEngineState.SearchQuery);
        Assert.Equal(searchEngine, searchEngineState.GetActiveSearchEngineOrDefault());
    }
}