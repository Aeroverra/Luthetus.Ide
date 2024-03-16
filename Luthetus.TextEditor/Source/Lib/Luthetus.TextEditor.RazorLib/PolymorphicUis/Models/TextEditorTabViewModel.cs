using Luthetus.Common.RazorLib.Tabs.Displays;
using Luthetus.Common.RazorLib.PolymorphicViewModels.Models;
using Luthetus.Common.RazorLib.Tabs.Models;
using Luthetus.Common.RazorLib.Dialogs.Models;
using Luthetus.TextEditor.RazorLib.TextEditors.Displays;
using Luthetus.TextEditor.RazorLib.TextEditors.Models;
using Luthetus.TextEditor.RazorLib.TextEditors.Models.TextEditorServices;
using Luthetus.TextEditor.RazorLib.TextEditors.Models.Internals;
using Luthetus.TextEditor.RazorLib.Groups.Models;
using Luthetus.TextEditor.RazorLib.TextEditors.Displays;
using Luthetus.Common.RazorLib.Dimensions.Models;
using Luthetus.Common.RazorLib.Keys.Models;
using Luthetus.Common.RazorLib.JavaScriptObjects.Models;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Web;
using System.Collections.Immutable;

namespace Luthetus.TextEditor.RazorLib.PolymorphicUis.Models;

public partial record TextEditorTabViewModel : ITabViewModel
{
	public TextEditorTabViewModel(
		Key<TextEditorViewModel> viewModelKey,
		TextEditorGroup textEditorGroup,
		ITextEditorService textEditorService,
		IPolymorphicViewModel? polymorphicViewModel)
	{
		ViewModelKey = viewModelKey;
		TextEditorGroup = textEditorGroup;
		TextEditorService = textEditorService;
		PolymorphicViewModel = polymorphicViewModel;
	}

	public Key<TextEditorViewModel> ViewModelKey { get; }
	public TextEditorGroup TextEditorGroup { get; }
	public ITextEditorService TextEditorService { get; }

	public IPolymorphicViewModel? PolymorphicViewModel { get; }
	public Key<ITabViewModel> Key { get; }
	public string Title { get; init; }

	public Dictionary<string, object?>? TabParameterMap => new Dictionary<string, object?>
	{
		{
			nameof(TabDisplay.TabViewModel),
			this
		},
		{
			nameof(TabDisplay.IsBeingDragged),
			true
		}
	};

	public bool GetIsActive()
	{
		return TextEditorGroup.ActiveViewModelKey == ViewModelKey;
	}

	public Task OnClickAsync(MouseEventArgs mouseEventArgs)
	{
		if (!GetIsActive())
			TextEditorService.GroupApi.SetActiveViewModel(TextEditorGroup.GroupKey, ViewModelKey);
		
		return Task.CompletedTask;
	}

	public string GetDynamicCss()
	{
		return string.Empty;
	}

	public Task CloseAsync()
	{
		TextEditorService.GroupApi.RemoveViewModel(TextEditorGroup.GroupKey, ViewModelKey);
		return Task.CompletedTask;
	}
}
