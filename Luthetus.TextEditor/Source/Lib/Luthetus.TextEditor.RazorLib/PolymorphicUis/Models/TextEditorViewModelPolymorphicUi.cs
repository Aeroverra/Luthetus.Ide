using Luthetus.Common.RazorLib.Dialogs.Models;
using Luthetus.Common.RazorLib.PolymorphicUis.Displays;
using Luthetus.TextEditor.RazorLib.TextEditors.Displays;
using Luthetus.TextEditor.RazorLib.TextEditors.Models;
using Luthetus.TextEditor.RazorLib.TextEditors.Models.TextEditorServices;
using Luthetus.TextEditor.RazorLib.TextEditors.Models.Internals;
using Luthetus.TextEditor.RazorLib.Groups.Models;
using Luthetus.TextEditor.RazorLib.TextEditors.Displays;
using Luthetus.Common.RazorLib.PolymorphicUis.Models;
using Luthetus.Common.RazorLib.Dimensions.Models;
using Luthetus.Common.RazorLib.Keys.Models;
using Luthetus.Common.RazorLib.JavaScriptObjects.Models;
using Microsoft.JSInterop;
using System.Collections.Immutable;

namespace Luthetus.TextEditor.RazorLib.PolymorphicUis.Models;

public record TextEditorViewModelPolymorphicUi : IPolymorphicTab, IPolymorphicDialog, IPolymorphicDraggable
{
	public TextEditorViewModelPolymorphicUi(
		Key<TextEditorViewModel> viewModelKey)
	{
		ViewModelKey = viewModelKey;
		DialogElementDimensions = DialogConstructDefaultElementDimensions();
	}

	/// <summary>
	/// TODO: Aquire access to the TextEditorService via the constructor...
	/// ...This hack was used instead, because adding the TextEditorService
	/// to the view model registration logic seems like an odd thing to do.
	/// This needs to be looked into more.
	///
	/// The hack is that: the UI when it sees an instance of this type,
	/// it will set this type's TextEditorService property.
	/// </summary>
	public ITextEditorService? TextEditorService { get; set; }
	public IJSRuntime? JsRuntime { get; set; }
	public IDialogService DialogService { get; set; }

	public Key<TextEditorViewModel> ViewModelKey { get; }
	public TextEditorGroup TextEditorGroup { get; set; }

	public Key<IPolymorphicUiRecord> PolymorphicUiKey { get; } = Key<IPolymorphicUiRecord>.NewKey();
	public string? CssClass { get; }
	public string? CssStyle { get; }
	public string Title => GetTitle();
	public Type RendererType { get; } = typeof(PolymorphicTabDisplay);

	private readonly TextEditorViewModelDisplayOptions _textEditorViewModelDisplayOptions = new()
	{
		IncludeHeaderHelperComponent = false,
	};

	private Key<DialogRecord> _dialogKey = Key<DialogRecord>.NewKey();

	public Dictionary<string, object?>? TabParameterMap => new Dictionary<string, object?>
	{
		{
			nameof(PolymorphicTabDisplay.Tab),
			this
		},
		{
			nameof(PolymorphicTabDisplay.IsBeingDragged),
			true
		}
	};

	public ElementDimensions DialogElementDimensions { get; }
    public bool DialogIsMinimized { get; set; }
    public bool DialogIsMaximized { get; set; }
    public bool DialogIsResizable { get; set; }
    public string DialogFocusPointHtmlElementId => $"luth_dialog-focus-point_{PolymorphicUiKey.Guid}";

	public Type DraggableRendererType => RendererType;
	public Dictionary<string, object?> DraggableParameterMap => TabParameterMap;
	public ElementDimensions DraggableElementDimensions => DialogElementDimensions;
	public ImmutableArray<IPolymorphicDropzone> DropzoneList { get; set; } = ImmutableArray<IPolymorphicDropzone>.Empty;

    public ElementDimensions DialogConstructDefaultElementDimensions()
	{
		var elementDimensions = new ElementDimensions
        {
            ElementPositionKind = ElementPositionKind.Fixed
        };

        // Width
        {
            var width = elementDimensions.DimensionAttributeList.Single(
                x => x.DimensionAttributeKind == DimensionAttributeKind.Width);

            width.DimensionUnitList.Add(new DimensionUnit
            {
                Value = 60,
                DimensionUnitKind = DimensionUnitKind.ViewportWidth
            });
        }

        // Height
        {
            var height = elementDimensions.DimensionAttributeList.Single(
                x => x.DimensionAttributeKind == DimensionAttributeKind.Height);

            height.DimensionUnitList.Add(new DimensionUnit
            {
                Value = 60,
                DimensionUnitKind = DimensionUnitKind.ViewportHeight
            });
        }

        // Left
        {
            var left = elementDimensions.DimensionAttributeList.Single(
                x => x.DimensionAttributeKind == DimensionAttributeKind.Left);

            left.DimensionUnitList.Add(new DimensionUnit
            {
                Value = 20,
                DimensionUnitKind = DimensionUnitKind.ViewportWidth
            });
        }

        // Top
        {
            var top = elementDimensions.DimensionAttributeList.Single(
                x => x.DimensionAttributeKind == DimensionAttributeKind.Top);

            top.DimensionUnitList.Add(new DimensionUnit
            {
                Value = 20,
                DimensionUnitKind = DimensionUnitKind.ViewportHeight
            });
        }

        return elementDimensions;
	}

	public Task OnDragStopAsync(IPolymorphicDropzone? dropzone)
	{
		if (dropzone is not TextEditorViewModelPolymorphicDropzone textEditorViewModelPolymorphicDropzone)
			return Task.CompletedTask;

		if (textEditorViewModelPolymorphicDropzone.TextEditorGroupKey is null)
		{
			TextEditorService.GroupApi.RemoveViewModel(TextEditorGroup.GroupKey, ViewModelKey);

			var dialogRecord = new DialogRecord(
    			_dialogKey,
    			GetTitle(),
    			typeof(TextEditorViewModelDisplay),
    			new Dictionary<string, object?>()
				{
					{
						nameof(TextEditorViewModelDisplay.TextEditorViewModelKey),
						ViewModelKey
					},
					{
						nameof(TextEditorViewModelDisplay.ViewModelDisplayOptions),
						_textEditorViewModelDisplayOptions
					}
				},
    			null)
			{
				IsResizable = true
			};

			DialogService.RegisterDialogRecord(dialogRecord);
		}

		return Task.CompletedTask;
	}

	public async Task<ImmutableArray<IPolymorphicDropzone>> GetDropzonesAsync()
	{
		if (TextEditorService is null)
			return ImmutableArray<IPolymorphicDropzone>.Empty;

		var dropzoneList = new List<IPolymorphicDropzone>();

		var measuredHtmlElementDimensions = await JsRuntime.InvokeAsync<MeasuredHtmlElementDimensions>(
            "luthetusIde.measureElementById",
            $"luth_te_group_{TextEditorGroup.GroupKey.Guid}");
	
		measuredHtmlElementDimensions = measuredHtmlElementDimensions with
		{
			ZIndex = 1,
		};

		var elementDimensions = new ElementDimensions();

		elementDimensions.ElementPositionKind = ElementPositionKind.Fixed;

		// Width
		{
			var widthDimensionAttribute = elementDimensions.DimensionAttributeList.First(
                x => x.DimensionAttributeKind == DimensionAttributeKind.Width);

			widthDimensionAttribute.DimensionUnitList.Clear();
            widthDimensionAttribute.DimensionUnitList.Add(new DimensionUnit
            {
                Value = measuredHtmlElementDimensions.WidthInPixels,
                DimensionUnitKind = DimensionUnitKind.Pixels
            });
		}

		// Height
		{
			var heightDimensionAttribute = elementDimensions.DimensionAttributeList.First(
                x => x.DimensionAttributeKind == DimensionAttributeKind.Height);

			heightDimensionAttribute.DimensionUnitList.Clear();
            heightDimensionAttribute.DimensionUnitList.Add(new DimensionUnit
            {
                Value = measuredHtmlElementDimensions.HeightInPixels,
                DimensionUnitKind = DimensionUnitKind.Pixels
            });
		}

		// Left
		{
			var leftDimensionAttribute = elementDimensions.DimensionAttributeList.First(
                x => x.DimensionAttributeKind == DimensionAttributeKind.Left);

            leftDimensionAttribute.DimensionUnitList.Clear();
            leftDimensionAttribute.DimensionUnitList.Add(new DimensionUnit
            {
                Value = measuredHtmlElementDimensions.LeftInPixels,
                DimensionUnitKind = DimensionUnitKind.Pixels
            });
		}

		// Top
		{
			var topDimensionAttribute = elementDimensions.DimensionAttributeList.First(
                x => x.DimensionAttributeKind == DimensionAttributeKind.Top);

            topDimensionAttribute.DimensionUnitList.Clear();
            topDimensionAttribute.DimensionUnitList.Add(new DimensionUnit
            {
                Value = measuredHtmlElementDimensions.TopInPixels,
                DimensionUnitKind = DimensionUnitKind.Pixels
            });
		}

		dropzoneList.Add(new TextEditorViewModelPolymorphicDropzone(
			measuredHtmlElementDimensions,
			elementDimensions,
			TextEditorGroup.GroupKey));

		// Fallback dropzone
		{
			var fallbackElementDimensions = new ElementDimensions();

			fallbackElementDimensions.ElementPositionKind = ElementPositionKind.Fixed;

			// Width
			{
				var widthDimensionAttribute = fallbackElementDimensions.DimensionAttributeList.First(
	                x => x.DimensionAttributeKind == DimensionAttributeKind.Width);
	
				widthDimensionAttribute.DimensionUnitList.Clear();
	            widthDimensionAttribute.DimensionUnitList.Add(new DimensionUnit
	            {
	                Value = 100,
	                DimensionUnitKind = DimensionUnitKind.ViewportWidth
	            });
			}
	
			// Height
			{
				var heightDimensionAttribute = fallbackElementDimensions.DimensionAttributeList.First(
	                x => x.DimensionAttributeKind == DimensionAttributeKind.Height);
	
				heightDimensionAttribute.DimensionUnitList.Clear();
	            heightDimensionAttribute.DimensionUnitList.Add(new DimensionUnit
	            {
	                Value = 100,
	                DimensionUnitKind = DimensionUnitKind.ViewportHeight
	            });
			}
	
			// Left
			{
				var leftDimensionAttribute = fallbackElementDimensions.DimensionAttributeList.First(
	                x => x.DimensionAttributeKind == DimensionAttributeKind.Left);
	
	            leftDimensionAttribute.DimensionUnitList.Clear();
	            leftDimensionAttribute.DimensionUnitList.Add(new DimensionUnit
	            {
	                Value = 0,
	                DimensionUnitKind = DimensionUnitKind.Pixels
	            });
			}
	
			// Top
			{
				var topDimensionAttribute = fallbackElementDimensions.DimensionAttributeList.First(
	                x => x.DimensionAttributeKind == DimensionAttributeKind.Top);
	
	            topDimensionAttribute.DimensionUnitList.Clear();
	            topDimensionAttribute.DimensionUnitList.Add(new DimensionUnit
	            {
	                Value = 0,
	                DimensionUnitKind = DimensionUnitKind.Pixels
	            });
			}
	
			dropzoneList.Add(new TextEditorViewModelPolymorphicDropzone(
				new MeasuredHtmlElementDimensions(0, 0, 0, 0, 0),
				fallbackElementDimensions,
				null));
		}

		var result = dropzoneList.ToImmutableArray();
		DropzoneList = result;
		return result;
	}

	private string GetTitle()
	{
		if (TextEditorService is null)
			return "TextEditorService was null";

		var model = TextEditorService.ViewModelApi.GetModelOrDefault(ViewModelKey);
		var viewModel = TextEditorService.ViewModelApi.GetOrDefault(ViewModelKey);

		if (viewModel is null)
        {
            return "ViewModel not found";
        }
		else if (model is null)
		{
            return "Model not found";
		}
		else
		{
			var displayName = viewModel.GetTabDisplayNameFunc?.Invoke(model)
				?? model.ResourceUri.Value;

			if (model.IsDirty)
				displayName += '*';

			return displayName;
		}
	}

	public Task TabSetAsActiveAsync()
	{
		TextEditorService.GroupApi.SetActiveViewModel(TextEditorGroup.GroupKey, ViewModelKey);
		return Task.CompletedTask;
	}

	public string TabGetDynamicCss()
	{
		return TextEditorGroup.ActiveViewModelKey == ViewModelKey
			? "luth_active"
		    : string.Empty;
	}

	public Task TabCloseAsync()
	{
		TextEditorService.GroupApi.RemoveViewModel(TextEditorGroup.GroupKey, ViewModelKey);
		return Task.CompletedTask;
	}
}