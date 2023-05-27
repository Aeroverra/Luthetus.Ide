﻿using Luthetus.Ide.ClassLib.CompilerServices.Common.General;
using Luthetus.Ide.ClassLib.CompilerServices.Languages.CSharp.ParserCase;

namespace Luthetus.Ide.ClassLib.CompilerServices.Languages.CSharp.TextEditorCase;

public class SemanticModelResultCSharp
{
    public SemanticModelResultCSharp(
        string text,
        Parser parserSession,
        CompilationUnit compilationUnit)
    {
        Text = text;
        ParserSession = parserSession;
        CompilationUnit = compilationUnit;
    }

    public string Text { get; }
    public Parser ParserSession { get; }
    public CompilationUnit CompilationUnit { get; }
}