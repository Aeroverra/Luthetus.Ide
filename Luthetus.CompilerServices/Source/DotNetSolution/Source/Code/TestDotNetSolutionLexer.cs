﻿using Luthetus.TextEditor.RazorLib.CompilerServices.Syntax.SyntaxTokens;
using Luthetus.TextEditor.RazorLib.CompilerServices;
using Luthetus.TextEditor.RazorLib.CompilerServices.GenericLexer.Decoration;
using Luthetus.TextEditor.RazorLib.CompilerServices.Syntax;
using Luthetus.TextEditor.RazorLib.Lexes.Models;
using Luthetus.CompilerServices.Lang.DotNetSolution.Facts;
using System.Collections.Immutable;

namespace Luthetus.CompilerServices.Lang.DotNetSolution.Code;

/// <summary>
/// As of (2023-09-21) I'm treating all of the .sln as a KeywordToken and IdentifierToken pair.
/// </summary>
public class TestDotNetSolutionLexer : ILexer
{
    private readonly StringWalker _stringWalker;
    private readonly List<ISyntaxToken> _syntaxTokens = new();
    private readonly LuthetusDiagnosticBag _diagnosticBag = new();

    public TestDotNetSolutionLexer(ResourceUri resourceUri, string sourceText)
    {
        _stringWalker = new(resourceUri, sourceText);
    }

    public ImmutableArray<ISyntaxToken> SyntaxTokens => _syntaxTokens.ToImmutableArray();
    public ImmutableArray<TextEditorDiagnostic> DiagnosticsBag => _diagnosticBag.ToImmutableArray();

    public void Lex()
    {
        while (!_stringWalker.IsEof)
        {
            if (_stringWalker.CheckForSubstring(LexSolutionFacts.Header.FORMAT_VERSION_START_TOKEN))
                LexHeaderFormatVersion();
            else if (_stringWalker.CheckForSubstring(LexSolutionFacts.Header.HASHTAG_VISUAL_STUDIO_VERSION_START_TOKEN))
                LexHashtagVisualStudioVersion();
            else if (_stringWalker.CheckForSubstring(LexSolutionFacts.Header.EXACT_VISUAL_STUDIO_VERSION_START_TOKEN))
                LexExactVisualStudioVersion();
            else if (_stringWalker.CheckForSubstring(LexSolutionFacts.Header.MINIMUM_VISUAL_STUDIO_VERSION_START_TOKEN))
                LexMinimumVisualStudioVersion();
            else if (_stringWalker.CheckForSubstring(LexSolutionFacts.Project.PROJECT_DEFINITION_START_TOKEN))
                LexProjectDefinitionEntry();
            else if (_stringWalker.CheckForSubstring(LexSolutionFacts.Global.START_TOKEN))
                LexGlobal();

            _ = _stringWalker.ReadCharacter();
        }

        var endOfFileTextSpan = new TextEditorTextSpan(
            _stringWalker.PositionIndex,
            _stringWalker.PositionIndex,
            (byte)GenericDecorationKind.None,
            _stringWalker.ResourceUri,
            _stringWalker.SourceText);

        _syntaxTokens.Add(new EndOfFileToken(endOfFileTextSpan));
    }

    private void LexHeaderFormatVersion()
    {
        var startingPosition = _stringWalker.PositionIndex;

        _ = _stringWalker.ReadRange(LexSolutionFacts.Header.FORMAT_VERSION_START_TOKEN.Length);

        var formatVersionTextSpan = new TextEditorTextSpan(startingPosition, _stringWalker);
        _syntaxTokens.Add(new KeywordToken(formatVersionTextSpan, SyntaxKind.UnrecognizedTokenKeyword));

        _ = _stringWalker.ReadWhitespace();

        var numericLiteralToken = _stringWalker.ReadUnsignedNumericLiteral();
        // Taking the numericLiteralToken's TextSpan and constructing an identifier token is odd.
        // The .sln file doesn't have clear token structure. As of (2023-09-21) I'm treating
        // all of the .sln as a KeywordToken and IdentifierToken pair.
        var identifierToken = new IdentifierToken(numericLiteralToken.TextSpan);

        _syntaxTokens.Add(identifierToken);
    }

    private void LexHashtagVisualStudioVersion()
    {
        var startingPosition = _stringWalker.PositionIndex;

        _ = _stringWalker.ReadRange(LexSolutionFacts.Header.HASHTAG_VISUAL_STUDIO_VERSION_START_TOKEN.Length);

        var vSVersionTextSpan = new TextEditorTextSpan(startingPosition, _stringWalker);
        _syntaxTokens.Add(new KeywordToken(vSVersionTextSpan, SyntaxKind.UnrecognizedTokenKeyword));

        _ = _stringWalker.ReadWhitespace();

        var numericLiteralToken = _stringWalker.ReadUnsignedNumericLiteral();
        // Taking the numericLiteralToken's TextSpan and constructing an identifier token is odd.
        // The .sln file doesn't have clear token structure. As of (2023-09-21) I'm treating
        // all of the .sln as a KeywordToken and IdentifierToken pair.
        var identifierToken = new IdentifierToken(numericLiteralToken.TextSpan);

        _syntaxTokens.Add(identifierToken);
    }

    private void LexExactVisualStudioVersion()
    {
        var stringStartingPosition = _stringWalker.PositionIndex;

        _ = _stringWalker.ReadRange(LexSolutionFacts.Header.EXACT_VISUAL_STUDIO_VERSION_START_TOKEN.Length);

        var versionStringTextSpan = new TextEditorTextSpan(stringStartingPosition, _stringWalker);
        _syntaxTokens.Add(new KeywordToken(versionStringTextSpan, SyntaxKind.UnrecognizedTokenKeyword));

        _ = _stringWalker.ReadWhitespace();

        var versionIdentifierStartingPosition = _stringWalker.PositionIndex;

        while (!_stringWalker.IsEof)
        {
            if (!char.IsDigit(_stringWalker.CurrentCharacter) &&
                _stringWalker.CurrentCharacter != '.')
            {
                break;
            }

            _ = _stringWalker.ReadCharacter();
        }

        var versionIdentifierTextSpan = new TextEditorTextSpan(versionIdentifierStartingPosition, _stringWalker);
        _syntaxTokens.Add(new IdentifierToken(versionIdentifierTextSpan));
    }

    private void LexMinimumVisualStudioVersion()
    {
        var stringStartingPosition = _stringWalker.PositionIndex;

        _ = _stringWalker.ReadRange(LexSolutionFacts.Header.MINIMUM_VISUAL_STUDIO_VERSION_START_TOKEN.Length);

        var versionStringTextSpan = new TextEditorTextSpan(stringStartingPosition, _stringWalker);
        _syntaxTokens.Add(new KeywordToken(versionStringTextSpan, SyntaxKind.UnrecognizedTokenKeyword));

        _ = _stringWalker.ReadWhitespace();

        var versionIdentifierStartingPosition = _stringWalker.PositionIndex;

        while (!_stringWalker.IsEof)
        {
            if (!char.IsDigit(_stringWalker.CurrentCharacter) &&
                _stringWalker.CurrentCharacter != '.')
            {
                break;
            }

            _ = _stringWalker.ReadCharacter();
        }

        var versionIdentifierTextSpan = new TextEditorTextSpan(versionIdentifierStartingPosition, _stringWalker);
        _syntaxTokens.Add(new IdentifierToken(versionIdentifierTextSpan));
    }

    public void LexProjectDefinitionEntry()
    {
        var startPosition = _stringWalker.PositionIndex;
        _ = _stringWalker.ReadRange(LexSolutionFacts.Project.PROJECT_DEFINITION_START_TOKEN.Length);

        var textSpan = new TextEditorTextSpan(startPosition, _stringWalker);
        _syntaxTokens.Add(new KeywordToken(textSpan, SyntaxKind.UnrecognizedTokenKeyword));

        while (!_stringWalker.IsEof)
        {
            if (_stringWalker.CurrentCharacter == '"')
            {
                if (_stringWalker.NextCharacter == '{')
                    LexGuid();
                else
                    LexString();
            }
            else if (_stringWalker.CheckForSubstring(LexSolutionFacts.Project.PROJECT_DEFINITION_END_TOKEN))
            {
                startPosition = _stringWalker.PositionIndex;
                _stringWalker.ReadRange(LexSolutionFacts.Project.PROJECT_DEFINITION_END_TOKEN.Length);

                textSpan = new TextEditorTextSpan(startPosition, _stringWalker);
                _syntaxTokens.Add(new KeywordToken(textSpan, SyntaxKind.UnrecognizedTokenKeyword));
                break;
            }

            _ = _stringWalker.ReadCharacter();
        }
    }

    public void LexGuid()
    {
        // "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}"
        // ^
        _ = _stringWalker.ReadCharacter();
        _ = _stringWalker.ReadCharacter();

        // "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}"
        //   ^
        var startPosition = _stringWalker.PositionIndex;
        _ = _stringWalker.ReadUntil('}');

        // "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}"
        //                                       ^
        var guidTextSpan = new TextEditorTextSpan(startPosition, _stringWalker);

        // guidTextSpan.GetText() == "FAE04EC0-301F-11D3-BF4B-00C04F79EFBC"
        _syntaxTokens.Add(new IdentifierToken(guidTextSpan));

        _ = _stringWalker.ReadCharacter();
        _ = _stringWalker.ReadCharacter();

        // "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}"
        //                                         ^
    }

    public void LexString()
    {
        // "ConsoleApp2"
        // ^
        _ = _stringWalker.ReadCharacter();

        // "ConsoleApp2"
        //  ^
        var startPosition = _stringWalker.PositionIndex;
        _ = _stringWalker.ReadUntil('"');

        // "ConsoleApp2"
        //             ^
        var textSpan = new TextEditorTextSpan(startPosition, _stringWalker);

        // textSpan.GetText() == "ConsoleApp2"
        _syntaxTokens.Add(new IdentifierToken(textSpan));

        _ = _stringWalker.ReadCharacter();

        // "ConsoleApp2"
        //              ^
    }

    public void LexGlobal()
    {
        var startPosition = _stringWalker.PositionIndex;
        _ = _stringWalker.ReadRange(LexSolutionFacts.Global.START_TOKEN.Length);

        var textSpan = new TextEditorTextSpan(startPosition, _stringWalker);
        _syntaxTokens.Add(new KeywordToken(textSpan, SyntaxKind.UnrecognizedTokenKeyword));

        bool BreakPredicate() => _stringWalker.CheckForSubstring(LexSolutionFacts.Global.END_TOKEN);

        while (!_stringWalker.IsEof)
        {
            if (BreakPredicate())
            {
                startPosition = _stringWalker.PositionIndex;
                _stringWalker.ReadRange(LexSolutionFacts.Global.END_TOKEN.Length);

                textSpan = new TextEditorTextSpan(startPosition, _stringWalker);
                _syntaxTokens.Add(new KeywordToken(textSpan, SyntaxKind.UnrecognizedTokenKeyword));
                break;
            }
            else if (_stringWalker.CheckForSubstring(LexSolutionFacts.GlobalSection.START_TOKEN))
            {
                LexGlobalSection(BreakPredicate);
            }

            _ = _stringWalker.ReadCharacter();
        }
    }

    public void LexGlobalSection(Func<bool> outerLoopBreakPredicate)
    {
        var startPosition = _stringWalker.PositionIndex;
        _ = _stringWalker.ReadRange(LexSolutionFacts.GlobalSection.START_TOKEN.Length);

        var textSpan = new TextEditorTextSpan(startPosition, _stringWalker);
        _syntaxTokens.Add(new KeywordToken(textSpan, SyntaxKind.UnrecognizedTokenKeyword));

        bool BreakPredicate() => _stringWalker.CheckForSubstring(LexSolutionFacts.GlobalSection.END_TOKEN);

        while (!_stringWalker.IsEof)
        {
            if (BreakPredicate())
            {
                startPosition = _stringWalker.PositionIndex;
                _stringWalker.ReadRange(LexSolutionFacts.GlobalSection.END_TOKEN.Length);

                textSpan = new TextEditorTextSpan(startPosition, _stringWalker);
                _syntaxTokens.Add(new KeywordToken(textSpan, SyntaxKind.UnrecognizedTokenKeyword));
                break;
            }
            else if (outerLoopBreakPredicate.Invoke())
            {
                break;
            }
            else if (_stringWalker.CurrentCharacter == '(')
            {
                _ = _stringWalker.ReadCharacter();
                var globalSectionParameterStartPosition = _stringWalker.PositionIndex;
                var globalSectionParameter = _stringWalker.ReadUntil(')');

                textSpan = new TextEditorTextSpan(globalSectionParameterStartPosition, _stringWalker);
                _syntaxTokens.Add(new KeywordToken(textSpan, SyntaxKind.UnrecognizedTokenKeyword));

                // START_TOKEN_ORDER: 'preSolution' OR 'postSolution'
                {
                    _ = _stringWalker.ReadUntil('=');

                    if (_stringWalker.IsEof)
                        break;

                    _ = _stringWalker.ReadCharacter();
                    _ = _stringWalker.ReadWhitespace();

                    var startOrderTuple = _stringWalker.ReadWordTuple();
                    _syntaxTokens.Add(new KeywordToken(startOrderTuple.textSpan, SyntaxKind.UnrecognizedTokenKeyword));
                }

                LexPropertyNameAndValuePairs(() => BreakPredicate() || outerLoopBreakPredicate.Invoke());
            }

            _ = _stringWalker.ReadCharacter();
        }
    }

    private void LexPropertyNameAndValuePairs(Func<bool> outerLoopBreakPredicate)
    {
        while (!_stringWalker.IsEof)
        {
            _ = _stringWalker.ReadWhitespace();

            if (outerLoopBreakPredicate.Invoke())
            {
                _stringWalker.BacktrackCharacter();
                break;
            }

            var propertyNameStartPosition = _stringWalker.PositionIndex;
            var name = _stringWalker.ReadUntil('=');

            var nameNoWhitespace = name.TrimEnd();

            var nameTrailingWhitespaceCount = name.Length - nameNoWhitespace.Length;

            var propertyNameTextSpan = new TextEditorTextSpan(propertyNameStartPosition, _stringWalker);
            propertyNameTextSpan = propertyNameTextSpan with
            {
                EndingIndexExclusive = propertyNameTextSpan.EndingIndexExclusive - nameTrailingWhitespaceCount
            };

            _syntaxTokens.Add(new IdentifierToken(propertyNameTextSpan));

            if (_stringWalker.IsEof)
                return;

            _ = _stringWalker.ReadCharacter();
            _ = _stringWalker.ReadWhitespace();

            var propertyValueStartPosition = _stringWalker.PositionIndex;
            var value = _stringWalker.ReadLine();

            var valueNoWhitespace = value.TrimEnd();

            var valueTrailingWhitespaceCount = value.Length - valueNoWhitespace.Length;

            var propertyValueTextSpan = new TextEditorTextSpan(propertyValueStartPosition, _stringWalker);

            propertyValueTextSpan = propertyValueTextSpan with
            {
                EndingIndexExclusive = propertyValueTextSpan.EndingIndexExclusive - valueTrailingWhitespaceCount
            };

            _syntaxTokens.Add(new IdentifierToken(propertyValueTextSpan));
        }
    }
}