﻿using Luthetus.Common.RazorLib.Keys.Models;
using Luthetus.TextEditor.RazorLib.TextEditors.Models;

namespace Luthetus.Ide.RazorLib.CompilerServices.States;

public partial record CompilerServiceEditorState
{
    public record SetTextEditorViewModelKeyAction(Key<TextEditorViewModel> TextEditorViewModelKey);
}
