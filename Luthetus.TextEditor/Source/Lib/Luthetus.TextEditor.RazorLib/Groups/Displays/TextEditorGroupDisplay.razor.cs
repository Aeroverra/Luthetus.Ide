using Luthetus.Common.RazorLib.PolymorphicUis.Models;
using Microsoft.AspNetCore.Components;
using Fluxor;
using System.Collections.Immutable;
using Luthetus.TextEditor.RazorLib.Groups.States;
using Luthetus.TextEditor.RazorLib.Groups.Models;
using Luthetus.TextEditor.RazorLib.TextEditors.Models.Internals;
using Luthetus.Common.RazorLib.Keys.Models;
using Luthetus.Common.RazorLib.PolymorphicUis.Displays;
using Luthetus.Common.RazorLib.Dialogs.Models;
using Luthetus.TextEditor.RazorLib.TextEditors.Models.TextEditorServices;
using Luthetus.TextEditor.RazorLib.TextEditors.Models;
using Luthetus.TextEditor.RazorLib.TextEditors.States;
using Microsoft.JSInterop;

namespace Luthetus.TextEditor.RazorLib.Groups.Displays;

public partial class TextEditorGroupDisplay : ComponentBase, IDisposable
{
    [Inject]
    private IState<TextEditorGroupState> TextEditorGroupStateWrap { get; set; } = null!;
	[Inject]
    private IState<TextEditorViewModelState> TextEditorViewModelStateWrap { get; set; } = null!;
    [Inject]
    private ITextEditorService TextEditorService { get; set; } = null!;
	[Inject]
    private IDialogService DialogService { get; set; } = null!;
	[Inject]
    private IJSRuntime JsRuntime { get; set; } = null!;

    /// <summary>
    /// If the provided <see cref="TextEditorGroupKey"/> is registered using the
    /// <see cref="ITextEditorService"/>. Then this component will automatically update
    /// when the corresponding <see cref="TextEditorGroup"/> is replaced.
    /// <br/><br/>
    /// A <see cref="TextEditorGroupKey"/> which is NOT registered using the
    /// <see cref="ITextEditorService"/> can be passed in. Then if the <see cref="TextEditorGroupKey"/>
    /// ever gets registered then this Blazor Component will update accordingly.
    /// </summary>
    [Parameter, EditorRequired]
    public Key<TextEditorGroup> TextEditorGroupKey { get; set; } = Key<TextEditorGroup>.Empty;

    [Parameter]
    public string CssStyleString { get; set; } = string.Empty;
    [Parameter]
    public string CssClassString { get; set; } = string.Empty;
    /// <summary><see cref="HeaderButtonKindList"/> contains the enum value that represents a button displayed in the optional component: <see cref="TextEditorHeader"/>.</summary>
    [Parameter]
    public TextEditorViewModelDisplayOptions ViewModelDisplayOptions { get; set; } = new();

	private PolymorphicTabListDisplay? _polymorphicTabListDisplay;

	private string? _htmlId = null;
	private string HtmlId => _htmlId ??= $"luth_te_group_{TextEditorGroupKey.Guid}";

    protected override void OnInitialized()
    {
        TextEditorGroupStateWrap.StateChanged += TextEditorGroupWrapOnStateChanged;
        TextEditorViewModelStateWrap.StateChanged += TextEditorViewModelStateWrapOnStateChanged;

        base.OnInitialized();
    }

    private async void TextEditorGroupWrapOnStateChanged(object? sender, EventArgs e) =>
        await InvokeAsync(StateHasChanged);

	private async void TextEditorViewModelStateWrapOnStateChanged(object? sender, EventArgs e)
	{
		var localPolymorphicTabListDisplay = _polymorphicTabListDisplay;

		if (localPolymorphicTabListDisplay is not null)
			await _polymorphicTabListDisplay.NotifyStateChangedAsync();
	}

	private ImmutableArray<IPolymorphicTab> GetPolymphoricUiList(TextEditorGroup textEditorGroup)
	{
		var viewModelState = TextEditorViewModelStateWrap.Value;
		var polymphoricUiList = new List<IPolymorphicTab>();

		foreach (var viewModelKey in textEditorGroup.ViewModelKeyList)
		{
			var polymorphicUi = viewModelState.ViewModelPolymorphicUiList.FirstOrDefault(
				x => x.ViewModelKey == viewModelKey);

			if (polymorphicUi is not null)
			{
				polymorphicUi.TextEditorGroup = textEditorGroup;
				polymorphicUi.TextEditorService = TextEditorService;
				polymorphicUi.DialogService = DialogService;
				polymorphicUi.JsRuntime = JsRuntime;
				polymphoricUiList.Add(polymorphicUi);
			}
		}

		return polymphoricUiList.ToImmutableArray();
	}

    public void Dispose()
    {
        TextEditorGroupStateWrap.StateChanged -= TextEditorGroupWrapOnStateChanged;
		TextEditorViewModelStateWrap.StateChanged -= TextEditorViewModelStateWrapOnStateChanged;
    }
}