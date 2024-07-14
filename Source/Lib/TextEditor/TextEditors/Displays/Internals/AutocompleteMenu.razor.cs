using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Collections.Immutable;
using Luthetus.Common.RazorLib.Menus.Models;
using Luthetus.Common.RazorLib.Keyboards.Models;
using Luthetus.Common.RazorLib.Menus.Displays;
using Luthetus.TextEditor.RazorLib.Autocompletes.Models;
using Luthetus.TextEditor.RazorLib.TextEditors.Models;
using Luthetus.TextEditor.RazorLib.Cursors.Models;
using Luthetus.TextEditor.RazorLib.TextEditors.Models.Internals;
using Luthetus.TextEditor.RazorLib.Lexes.Models;
using Luthetus.TextEditor.RazorLib.Exceptions;

namespace Luthetus.TextEditor.RazorLib.TextEditors.Displays.Internals;

public partial class AutocompleteMenu : ComponentBase
{
    [Inject]
    private ITextEditorService TextEditorService { get; set; } = null!;
    [Inject]
    private IAutocompleteService AutocompleteService { get; set; } = null!;

    [CascadingParameter]
    public TextEditorRenderBatchValidated RenderBatch { get; set; } = null!;
    [CascadingParameter(Name = "TextEditorMenuShouldTakeFocusFunc")]
    public Func<bool> TextEditorMenuShouldTakeFocusFunc { get; set; } = null!;

    private ElementReference? _autocompleteMenuElementReference;
    private MenuDisplay? _autocompleteMenuComponent;

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (TextEditorMenuShouldTakeFocusFunc.Invoke())
            _autocompleteMenuComponent?.SetFocusToFirstOptionInMenuAsync();

        return base.OnAfterRenderAsync(firstRender);
    }

    private void HandleOnKeyDown(KeyboardEventArgs keyboardEventArgs)
    {
        if (KeyboardKeyFacts.MetaKeys.ESCAPE == keyboardEventArgs.Key)
		{
			TextEditorService.PostSimpleBatch(
				nameof(AutocompleteMenu),
				editContext =>
				{
					var viewModelModifier = editContext.GetViewModelModifier(RenderBatch.ViewModel.ViewModelKey);

					viewModelModifier.ViewModel = viewModelModifier.ViewModel with
					{
						MenuKind = MenuKind.None
					};

					return Task.CompletedTask;
				});
		}
    }

    private Task ReturnFocusToThisAsync()
    {
        try
        {
            TextEditorService.PostSimpleBatch(
				nameof(AutocompleteMenu),
				editContext =>
				{
					var viewModelModifier = editContext.GetViewModelModifier(RenderBatch.ViewModel.ViewModelKey);

					viewModelModifier.ViewModel = viewModelModifier.ViewModel with
					{
						MenuKind = MenuKind.None
					};

					return Task.CompletedTask;
				});
			return Task.CompletedTask;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private MenuRecord GetMenuRecord()
    {
        try
        {
            var cursorList = new TextEditorCursor[] { RenderBatch.ViewModel.PrimaryCursor }.ToImmutableArray();

            var primaryCursor = cursorList.First(x => x.IsPrimaryCursor);

            if (primaryCursor.ColumnIndex > 0)
            {
                var word = RenderBatch.Model.ReadPreviousWordOrDefault(
                    primaryCursor.LineIndex,
                    primaryCursor.ColumnIndex);

                List<MenuOptionRecord> menuOptionRecordsList = new();

                if (word is not null)
                {
                    var autocompleteWordsList = AutocompleteService.GetAutocompleteOptions(word);

                    var autocompleteEntryList = autocompleteWordsList
                        .Select(aw => new AutocompleteEntry(aw, AutocompleteEntryKind.Word, null))
                        .ToArray();

                    // (2023-08-09) Looking into using an ICompilerService for autocompletion.
                    {
                        var positionIndex = RenderBatch.Model.GetPositionIndex(primaryCursor);

                        var textSpan = new TextEditorTextSpan(
                            positionIndex,
                            positionIndex + 1,
                            0,
                            RenderBatch.Model.ResourceUri,
                            // TODO: RenderBatch.Model.GetAllText() probably isn't needed here. Maybe a useful optimization is to remove it somehow?
                            RenderBatch.Model.GetAllText());

                        var compilerServiceAutocompleteEntryList = RenderBatch.Model.CompilerService.GetAutocompleteEntries(
                            word,
                            textSpan);

                        if (compilerServiceAutocompleteEntryList.Any())
                        {
                            autocompleteEntryList = compilerServiceAutocompleteEntryList
                                .AddRange(autocompleteEntryList)
                                .ToArray();
                        }
                    }

                    menuOptionRecordsList = autocompleteEntryList.Select(entry => new MenuOptionRecord(
                        entry.DisplayName,
                        MenuOptionKind.Other,
                        () => SelectMenuOption(() =>
                        {
                            InsertAutocompleteMenuOption(word, entry, RenderBatch.ViewModel);
                            entry.SideEffectFunc?.Invoke();
                            return Task.CompletedTask;
                        }),
                        WidgetParameterMap: new Dictionary<string, object?>
                        {
                            {
                                nameof(AutocompleteEntry),
                                entry
                            }
                        }))
                    .ToList();
                }

                if (!menuOptionRecordsList.Any())
                    menuOptionRecordsList.Add(new MenuOptionRecord("No results", MenuOptionKind.Other));

                return new MenuRecord(menuOptionRecordsList.ToImmutableArray());
            }

            return new MenuRecord(new MenuOptionRecord[]
            {
                new("No results", MenuOptionKind.Other)
            }.ToImmutableArray());
        }
		// Catching 'InvalidOperationException' is for the currently occurring case: "Collection was modified; enumeration operation may not execute."
        catch (Exception e) when (e is LuthetusTextEditorException || e is InvalidOperationException)
        {
            return new MenuRecord(new MenuOptionRecord[]
            {
                new("No results", MenuOptionKind.Other)
            }.ToImmutableArray());
        }
    }

    private Task SelectMenuOption(Func<Task> menuOptionAction)
    {
        _ = Task.Run(async () =>
        {
            try
            {
				TextEditorService.PostSimpleBatch(
					nameof(AutocompleteMenu),
					editContext =>
					{
						var viewModelModifier = editContext.GetViewModelModifier(RenderBatch.ViewModel.ViewModelKey);
	
						viewModelModifier.ViewModel = viewModelModifier.ViewModel with
						{
							MenuKind = MenuKind.None
						};

						return Task.CompletedTask;
					});

                await menuOptionAction.Invoke().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }, CancellationToken.None);

        return Task.CompletedTask;
    }

    private Task InsertAutocompleteMenuOption(
        string word,
        AutocompleteEntry autocompleteEntry,
        TextEditorViewModel viewModel)
    {
        TextEditorService.PostSimpleBatch(
            nameof(InsertAutocompleteMenuOption),
            TextEditorService.ModelApi.InsertTextFactory(
                viewModel.ResourceUri,
                viewModel.ViewModelKey,
                autocompleteEntry.DisplayName.Substring(word.Length),
                CancellationToken.None));
		return Task.CompletedTask;
    }
}