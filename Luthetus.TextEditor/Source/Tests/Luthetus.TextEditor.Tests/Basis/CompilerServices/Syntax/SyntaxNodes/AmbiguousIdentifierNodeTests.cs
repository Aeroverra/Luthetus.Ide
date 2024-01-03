﻿using Xunit;
using Luthetus.TextEditor.RazorLib.CompilerServices.Syntax.SyntaxNodes;
using Luthetus.TextEditor.RazorLib.CompilerServices.Syntax.SyntaxTokens;
using Luthetus.TextEditor.RazorLib.Lexes.Models;

namespace Luthetus.TextEditor.Tests.Basis.CompilerServices.Syntax.SyntaxNodes;

/// <summary>
/// <see cref="AmbiguousIdentifierNode"/>
/// </summary>
public class AmbiguousIdentifierNodeTests
{
    /// <summary>
    /// <see cref="AmbiguousIdentifierNode(IdentifierToken)"/>
    /// <br/>----<br/>
    /// <see cref="AmbiguousIdentifierNode.IdentifierToken"/>
	/// <see cref="AmbiguousIdentifierNode.ChildBag"/>
	/// <see cref="AmbiguousIdentifierNode.IsFabricated"/>
	/// <see cref="AmbiguousIdentifierNode.SyntaxKind"/>
    /// </summary>
    [Fact]
	public void Constructor()
	{
		var typeIdentifier = "SomeUndefinedType";
        var sourceText = $@"{typeIdentifier} MyMethod()
{{
}}";

		var indexOfTypeIdentifierInclusive = sourceText.IndexOf(typeIdentifier);

		var identifierToken = new IdentifierToken(new TextEditorTextSpan(
            indexOfTypeIdentifierInclusive,
            indexOfTypeIdentifierInclusive + typeIdentifier.Length,
			0,
			new ResourceUri("/unitTesting.txt"),
			sourceText));

		var ambiguousIdentifierNode = new AmbiguousIdentifierNode(identifierToken);

		Assert.Equal(identifierToken, ambiguousIdentifierNode.IdentifierToken);
		
		Assert.Single(ambiguousIdentifierNode.ChildBag);
		Assert.Equal(identifierToken, ambiguousIdentifierNode.ChildBag.Single());

		Assert.False(ambiguousIdentifierNode.IsFabricated);

		Assert.Equal(
			RazorLib.CompilerServices.Syntax.SyntaxKind.AmbiguousIdentifierNode,
			ambiguousIdentifierNode.SyntaxKind);
	}
}
