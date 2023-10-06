﻿using Luthetus.Common.RazorLib.Keymaps.Models;
using Luthetus.TextEditor.RazorLib.Commands.Models;
using System.Collections.Immutable;
using System.Text;

namespace Luthetus.TextEditor.RazorLib.Keymaps.Models.Vims;

public static class SyntaxRepeatVim
{
    public static bool TryLex(KeymapArgument keymapArgument, bool hasTextSelection, out VimGrammarToken? vimGrammarToken)
    {
        if (keymapArgument.Code is null)
        {
            vimGrammarToken = null;
            return false;
        }

        var possibleDigit = keymapArgument.Code.Last();

        if (char.IsNumber(possibleDigit) && possibleDigit != '0')
        {
            vimGrammarToken = new VimGrammarToken(VimGrammarKind.Repeat, keymapArgument);
            return true;
        }

        vimGrammarToken = null;
        return false;
    }

    public static bool TryParse(TextEditorKeymapVim textEditorKeymapVim,
        ImmutableArray<VimGrammarToken> sentenceSnapshotBag,
        int indexInSentence,
        KeymapArgument keymapArgument,
        bool hasTextSelection,
        out CommandTextEditor? textEditorCommand)
    {
        int modifiedIndexInSentence = indexInSentence;

        var numberBuilder = new StringBuilder();

        for (int i = indexInSentence; i < sentenceSnapshotBag.Length; i++)
        {
            var currentToken = sentenceSnapshotBag[i];

            if (currentToken.VimGrammarKind == VimGrammarKind.Repeat)
            {
                numberBuilder.Append(currentToken.KeymapArgument.Code.Last());
                modifiedIndexInSentence++;
            }
        }

        var intValue = int.Parse(numberBuilder.ToString());

        var success = VimSentence.TryParseNextToken(
            textEditorKeymapVim,
            sentenceSnapshotBag,
            modifiedIndexInSentence,
            keymapArgument,
            hasTextSelection,
            out var innerTextEditorCommand);

        if (success && innerTextEditorCommand is not null)
        {
            var textEditorCommandDisplayName = $"Vim::Repeat(count: {intValue}, arg: {innerTextEditorCommand.DisplayName})";

            // Repeat the inner TextEditorCommand using a for loop
            textEditorCommand = new CommandTextEditor(
                async textEditorCommandParameter =>
                {
                    for (int index = 0; index < intValue; index++)
                    {
                        await innerTextEditorCommand.DoAsyncFunc.Invoke(textEditorCommandParameter);
                    }
                },
                true,
                textEditorCommandDisplayName,
                textEditorCommandDisplayName);
        }
        else
        {
            textEditorCommand = null;
        }

        return success;
    }
}