using Luthetus.Common.RazorLib.Dimensions.Models;
using Luthetus.Common.RazorLib.Keys.Models;
using Luthetus.Common.RazorLib.PolymorphicViewModels.Models;

namespace Luthetus.Common.RazorLib.Dialogs.Models;

public record DialogViewModel : IDialogViewModel
{
	public DialogViewModel(
		Key<IDialogViewModel> key,
		string title,
		Type rendererType,
		Dictionary<string, object?>? parameterMap,
		string cssClassString,
		bool isResizable,
		IPolymorphicViewModel? polymorphicViewModel)
	{
		Key = key;
		Title = title;
		RendererType = rendererType;
		ParameterMap = parameterMap;
		CssClassString = cssClassString;
		IsResizable = isResizable;
		PolymorphicViewModel = polymorphicViewModel;
	}

	public IPolymorphicViewModel? PolymorphicViewModel { get; init; }
	public Key<IDialogViewModel> Key { get; init; }
	public Type RendererType { get; init; }
	public Dictionary<string, object?>? ParameterMap { get; init; }
	public ElementDimensions ElementDimensions { get; init; }
    public string Title { get; init; }
    public bool IsMinimized { get; init; }
    public bool IsMaximized { get; init; }
    public bool IsResizable { get; init; }
    public string CssClassString { get; init; }

    public virtual string FocusPointHtmlElementId => $"luth_dialog-focus-point_{Key.Guid}";

	public IDialogViewModel SetParameterMap(Dictionary<string, object?>? parameterMap)
	{
		return this with { ParameterMap = parameterMap };
	}

	public IDialogViewModel SetTitle(string title)
	{
		return this with { Title = title };
	}

	public IDialogViewModel SetIsMinimized(bool isMinimized)
	{
		return this with { IsMinimized = isMinimized };
	}

	public IDialogViewModel SetIsMaximized(bool isMaximized)
	{
		return this with { IsMaximized = isMaximized };
	}

	public IDialogViewModel SetIsResizable(bool isResizable)
	{
		return this with { IsResizable = isResizable };
	}

	public IDialogViewModel SetCssClassString(string cssClassString)
	{
		return this with { CssClassString = cssClassString };
	}
}
