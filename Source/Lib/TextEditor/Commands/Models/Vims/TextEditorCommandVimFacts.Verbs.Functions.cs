using Microsoft.Extensions.DependencyInjection;
using Luthetus.Common.RazorLib.Keys.Models;
using Luthetus.Common.RazorLib.Clipboards.Models;
using Luthetus.TextEditor.RazorLib.Commands.Models.Defaults;
using Luthetus.TextEditor.RazorLib.Cursors.Models;
using Luthetus.TextEditor.RazorLib.Keymaps.Models;
using Luthetus.TextEditor.RazorLib.Keymaps.Models.Vims;
using Luthetus.TextEditor.RazorLib.TextEditors.Models;

namespace Luthetus.TextEditor.RazorLib.Commands.Models.Vims;

public static partial class TextEditorCommandVimFacts
{
    public static partial class Verbs
    {
        public static Task DeleteLine(
        	IEditContext editContext,
	        TextEditorModelModifier modelModifier,
	        TextEditorViewModelModifier viewModelModifier,
	        CursorModifierBagTextEditor cursorModifierBag,
        	TextEditorCommandArgs commandArgs)
        {
            return TextEditorCommandDefaultFunctions.CutAsync(
                editContext,
		        modelModifier,
		        viewModelModifier,
		        cursorModifierBag,
		        commandArgs.ServiceProvider.GetRequiredService<IClipboardService>());
        }

        public static async Task ChangeLine(
        	IEditContext editContext,
	        TextEditorModelModifier modelModifier,
	        TextEditorViewModelModifier viewModelModifier,
	        CursorModifierBagTextEditor cursorModifierBag,
        	TextEditorCommandArgs commandArgs)
        {
            var activeKeymap = commandArgs.ComponentData.Options.Keymap ?? TextEditorKeymapFacts.DefaultKeymap;
            if (activeKeymap is not TextEditorKeymapVim keymapVim)
                return;

            await TextEditorCommandDefaultFunctions.CutAsync(
	                editContext,
			        modelModifier,
			        viewModelModifier,
			        cursorModifierBag,
			        commandArgs.ServiceProvider.GetRequiredService<IClipboardService>())
				.ConfigureAwait(false);

            keymapVim.ActiveVimMode = VimMode.Insert;
        }

        public static async Task DeleteMotion(
        	IEditContext editContext,
	        TextEditorModelModifier modelModifier,
	        TextEditorViewModelModifier viewModelModifier,
	        CursorModifierBagTextEditor cursorModifierBag,
        	TextEditorCommandArgs commandArgs)
        {
            var textEditorCommandArgsForMotion = new TextEditorCommandArgs(
                modelModifier.ResourceUri,
                viewModelModifier.ViewModel.ViewModelKey,
				commandArgs.ComponentData,
                commandArgs.TextEditorService,
                commandArgs.ServiceProvider,
                editContext);
                
            var primaryCursorModifier = editContext.GetPrimaryCursorModifier(cursorModifierBag);

            var inCursor = primaryCursorModifier.ToCursor();

            var motionResult = await VimMotionResult.GetResultAsync(
                modelModifier,
                primaryCursorModifier,
                async () => 
                {
                    await commandArgs.InnerCommand.CommandFunc
                    	.Invoke(textEditorCommandArgsForMotion)
                    	.ConfigureAwait(false);
                }).ConfigureAwait(false);

            primaryCursorModifier.LineIndex = inCursor.LineIndex;
            primaryCursorModifier.ColumnIndex = inCursor.ColumnIndex;
            primaryCursorModifier.PreferredColumnIndex = inCursor.ColumnIndex;

            var cursorForDeletion = new TextEditorCursor(
                motionResult.LowerPositionIndexCursor.LineIndex,
                motionResult.LowerPositionIndexCursor.ColumnIndex,
                true);

            var cursorModifierBagForDeletion = new CursorModifierBagTextEditor(
                Key<TextEditorViewModel>.Empty,
                new List<TextEditorCursorModifier> { new(cursorForDeletion) });

            await editContext.TextEditorService.ModelApi.DeleteTextByRangeUnsafe(
            		editContext,
                    modelModifier,
                    cursorModifierBagForDeletion,
                    motionResult.PositionIndexDisplacement,
                    CancellationToken.None)
				.ConfigureAwait(false);
        }

        public static async Task ChangeMotion(
        	IEditContext editContext,
	        TextEditorModelModifier modelModifier,
	        TextEditorViewModelModifier viewModelModifier,
	        CursorModifierBagTextEditor cursorModifierBag,
        	TextEditorCommandArgs commandArgs)
        {
            var activeKeymap = commandArgs.ComponentData.Options.Keymap ?? TextEditorKeymapFacts.DefaultKeymap;
            if (activeKeymap is not TextEditorKeymapVim keymapVim)
                return;

            var deleteMotion = DeleteMotionCommandConstructor(commandArgs.InnerCommand);

            await deleteMotion.CommandFunc.Invoke(commandArgs).ConfigureAwait(false);
            keymapVim.ActiveVimMode = VimMode.Insert;
        }

        public static async Task ChangeSelection(
        	IEditContext editContext,
	        TextEditorModelModifier modelModifier,
	        TextEditorViewModelModifier viewModelModifier,
	        CursorModifierBagTextEditor cursorModifierBag,
        	TextEditorCommandArgs commandArgs)
        {
            var activeKeymap = commandArgs.ComponentData.Options.Keymap ?? TextEditorKeymapFacts.DefaultKeymap;
            if (activeKeymap is not TextEditorKeymapVim keymapVim)
                return;

            await TextEditorCommandDefaultFacts.Cut.CommandFunc.Invoke(commandArgs).ConfigureAwait(false);
            keymapVim.ActiveVimMode = VimMode.Insert;
        }

        public static async Task YankAsync(
        	IEditContext editContext,
	        TextEditorModelModifier modelModifier,
	        TextEditorViewModelModifier viewModelModifier,
	        CursorModifierBagTextEditor cursorModifierBag,
        	TextEditorCommandArgs commandArgs)
        {
            await TextEditorCommandDefaultFacts.Copy.CommandFunc.Invoke(commandArgs).ConfigureAwait(false);
            await TextEditorCommandDefaultFacts.ClearTextSelection.CommandFunc.Invoke(commandArgs).ConfigureAwait(false);
        }

        public static void NewLineBelow(
        	IEditContext editContext,
	        TextEditorModelModifier modelModifier,
	        TextEditorViewModelModifier viewModelModifier,
	        CursorModifierBagTextEditor cursorModifierBag,
        	TextEditorCommandArgs commandArgs)
        {
            var activeKeymap = commandArgs.ComponentData.Options.Keymap ?? TextEditorKeymapFacts.DefaultKeymap;
            if (activeKeymap is not TextEditorKeymapVim keymapVim)
                return;

            TextEditorCommandDefaultFunctions.NewLineBelow(
	            	editContext,
			        modelModifier,
			        viewModelModifier,
			        cursorModifierBag,
			        commandArgs)
		        .ConfigureAwait(false);
		        
            keymapVim.ActiveVimMode = VimMode.Insert;
        }

        public static void NewLineAbove(
        	IEditContext editContext,
	        TextEditorModelModifier modelModifier,
	        TextEditorViewModelModifier viewModelModifier,
	        CursorModifierBagTextEditor cursorModifierBag,
        	TextEditorCommandArgs commandArgs)
        {
            var activeKeymap = commandArgs.ComponentData.Options.Keymap ?? TextEditorKeymapFacts.DefaultKeymap;
            if (activeKeymap is not TextEditorKeymapVim keymapVim)
                return;

            TextEditorCommandDefaultFunctions.NewLineAbove(
	            	editContext,
			        modelModifier,
			        viewModelModifier,
			        cursorModifierBag,
			        commandArgs)
		        .ConfigureAwait(false);
		        
            keymapVim.ActiveVimMode = VimMode.Insert;
        }
    }
}