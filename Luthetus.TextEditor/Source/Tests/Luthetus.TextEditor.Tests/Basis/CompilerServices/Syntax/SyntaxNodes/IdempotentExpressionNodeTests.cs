﻿using Xunit;
using Luthetus.TextEditor.RazorLib.CompilerServices.Syntax.SyntaxNodes;
using Luthetus.TextEditor.RazorLib.CompilerServices.Syntax.SyntaxTokens;
using Luthetus.CompilerServices.Lang.FSharp.FSharp.Facts;
using Luthetus.TextEditor.RazorLib.Lexes.Models;

namespace Luthetus.TextEditor.Tests.Basis.CompilerServices.Syntax.SyntaxNodes;

/// <summary>
/// <see cref="IdempotentExpressionNode"/>
/// </summary>
public class IdempotentExpressionNodeTests
{
    /// <summary>
    /// <see cref="IdempotentExpressionNode.IdempotentExpressionNode"/>
    /// <br/>----<br/>
    /// <see cref="IdempotentExpressionNode.ResultTypeClauseNode"/>
    /// <see cref="IdempotentExpressionNode.ChildBag"/>
    /// <see cref="IdempotentExpressionNode.IsFabricated"/>
    /// <see cref="IdempotentExpressionNode.SyntaxKind"/>
    /// </summary>
    [Fact]
	public void Constructor()
	{
        var sourceText = "()";
        _ = sourceText; // Suppress unused variable

        var voidTypeIdentifier = new KeywordToken(
            TextEditorTextSpan.FabricateTextSpan("void"),
                RazorLib.CompilerServices.Syntax.SyntaxKind.VoidTokenKeyword);

        var voidTypeClauseNode = new TypeClauseNode(
            voidTypeIdentifier,
            typeof(void),
            null);

        var idempotentExpressionNode = new IdempotentExpressionNode(voidTypeClauseNode);

        Assert.Equal(voidTypeClauseNode, idempotentExpressionNode.ResultTypeClauseNode);

        Assert.Single(idempotentExpressionNode.ChildBag);
        Assert.Equal(voidTypeClauseNode, idempotentExpressionNode.ChildBag.Single());

        Assert.False(idempotentExpressionNode.IsFabricated);

        Assert.Equal(
            RazorLib.CompilerServices.Syntax.SyntaxKind.IdempotentExpressionNode,
            idempotentExpressionNode.SyntaxKind);
	}
}