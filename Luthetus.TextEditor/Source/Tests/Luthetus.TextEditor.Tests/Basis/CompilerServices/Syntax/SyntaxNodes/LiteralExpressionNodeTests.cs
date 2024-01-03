﻿using Xunit;
using Luthetus.TextEditor.RazorLib.CompilerServices.Syntax.SyntaxNodes;
using Luthetus.TextEditor.RazorLib.CompilerServices.Syntax.SyntaxTokens;
using Luthetus.TextEditor.RazorLib.Lexes.Models;

namespace Luthetus.TextEditor.Tests.Basis.CompilerServices.Syntax.SyntaxNodes;

/// <summary>
/// <see cref="LiteralExpressionNode"/>
/// </summary>
public class LiteralExpressionNodeTests
{
    /// <summary>
    /// <see cref="LiteralExpressionNode(RazorLib.CompilerServices.Syntax.ISyntaxToken, TypeClauseNode)"/>
    /// <br/>----<br/>
    /// <see cref="LiteralExpressionNode.LiteralSyntaxToken"/>
    /// <see cref="LiteralExpressionNode.ResultTypeClauseNode"/>
    /// <see cref="LiteralExpressionNode.ChildBag"/>
    /// <see cref="LiteralExpressionNode.IsFabricated"/>
    /// <see cref="LiteralExpressionNode.SyntaxKind"/>
    /// </summary>
    [Fact]
	public void Constructor()
	{
        var sourceText = "3";

        var numericLiteralText = "3";
        int indexOfNumericLiteralText = sourceText.IndexOf(numericLiteralText);
        var numericLiteralToken = new NumericLiteralToken(new TextEditorTextSpan(
            indexOfNumericLiteralText,
            indexOfNumericLiteralText + numericLiteralText.Length,
            0,
            new ResourceUri("/unitTesting.txt"),
            sourceText));

        TypeClauseNode intTypeClauseNode;
        {
            var intTypeIdentifier = new KeywordToken(
                TextEditorTextSpan.FabricateTextSpan("int"),
                RazorLib.CompilerServices.Syntax.SyntaxKind.IntTokenKeyword);

            intTypeClauseNode = new TypeClauseNode(
                intTypeIdentifier,
                typeof(int),
                null);
        }

        var literalExpressionNode = new LiteralExpressionNode(
            numericLiteralToken,
            intTypeClauseNode);

        Assert.Equal(numericLiteralToken, literalExpressionNode.LiteralSyntaxToken);
        Assert.Equal(intTypeClauseNode, literalExpressionNode.ResultTypeClauseNode);

        Assert.Equal(2, literalExpressionNode.ChildBag.Length);
        Assert.Equal(numericLiteralToken, literalExpressionNode.ChildBag[0]);
        Assert.Equal(intTypeClauseNode, literalExpressionNode.ChildBag[1]);

        Assert.False(literalExpressionNode.IsFabricated);

        Assert.Equal(
            RazorLib.CompilerServices.Syntax.SyntaxKind.LiteralExpressionNode,
            literalExpressionNode.SyntaxKind);
	}
}
