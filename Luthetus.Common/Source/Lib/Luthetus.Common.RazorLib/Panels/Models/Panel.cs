using Luthetus.Common.RazorLib.Contexts.Models;
using Luthetus.Common.RazorLib.Dimensions.Models;
using Luthetus.Common.RazorLib.Keys.Models;
using Luthetus.Common.RazorLib.Dialogs.Models;
using Luthetus.Common.RazorLib.JavaScriptObjects.Models;
using Luthetus.Common.RazorLib.Panels.States;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Web;
using System.Collections.Immutable;
using Fluxor;
using Luthetus.Common.RazorLib.Dynamics.Models;
using Luthetus.Common.RazorLib.Tabs.Displays;

namespace Luthetus.Common.RazorLib.Panels.Models;

public record Panel : IPanelTab, IDialog, IDrag
{
	public Panel(
		string title,
		Key<Panel> key,
		Key<IDynamicViewModel> dynamicViewModelKey,
		Key<ContextRecord> contextRecordKey,
		Type componentType,
		Dictionary<string, object?>? componentParameterMap)
	{
		Title = title;
		Key = key;
		DynamicViewModelKey = dynamicViewModelKey;
		ContextRecordKey = contextRecordKey;
		ComponentType = componentType;
		ComponentParameterMap = componentParameterMap;
	}

    public string Title { get; }
	public Key<Panel> Key { get; }
	public Key<IDynamicViewModel> DynamicViewModelKey { get; }
    public Key<ContextRecord> ContextRecordKey { get; }
	public IDispatcher Dispatcher { get; set; }
    public IDialogService DialogService { get; set; }
    public IJSRuntime JsRuntime { get; set; }
	public Type ComponentType { get; }
	public Dictionary<string, object?>? ComponentParameterMap { get; set; }
	public string? CssClass { get; set; }
	public string? CssStyle { get; set; }
	public ITabGroup? TabGroup { get; set; }

    public bool DialogIsMinimized { get; set; }
    public bool DialogIsMaximized { get; set; }
    public bool DialogIsResizable { get; set; }
    public string DialogFocusPointHtmlElementId { get; set; }
    public ElementDimensions DialogElementDimensions { get; set; } = new();

	public ImmutableArray<IDropzone> DropzoneList { get; set; } = new();

	public Type DragComponentType { get; } = typeof(TabDisplay);

	public Dictionary<string, object?>? DragComponentParameterMap => new()
	{
		{
			nameof(TabDisplay.Tab),
			this
		},
		{
			nameof(TabDisplay.IsBeingDragged),
			true
		}
	};

	public string? DragCssClass { get; set; }
	public string? DragCssStyle { get; set; }
	public ElementDimensions DragElementDimensions { get; set; } = new();

	public IDialog SetParameterMap(Dictionary<string, object?>? componentParameterMap)
	{
		ComponentParameterMap = componentParameterMap;
		return this;
	}

	public IDialog SetIsMaximized(bool isMaximized)
	{
		DialogIsMaximized = isMaximized;
		return this;
	}

    public async Task OnDragStartAsync()
    {
		var dropzoneList = new List<IDropzone>();
		AddFallbackDropzone(dropzoneList);

		var panelGroupHtmlIdTupleList = new (Key<PanelGroup> PanelGroupKey, string HtmlElementId)[]
		{
			(PanelFacts.LeftPanelRecordKey, "luth_ide_panel_left_tabs"),
			(PanelFacts.RightPanelRecordKey, "luth_ide_panel_right_tabs"),
			(PanelFacts.BottomPanelRecordKey, "luth_ide_panel_bottom_tabs"),
		};

		foreach (var panelGroupHtmlIdTuple in panelGroupHtmlIdTupleList)
		{
			var measuredHtmlElementDimensions = await JsRuntime.InvokeAsync<MeasuredHtmlElementDimensions>(
				"luthetusIde.measureElementById",
				panelGroupHtmlIdTuple.HtmlElementId);

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

			dropzoneList.Add(new PanelGroupDropzone(
				measuredHtmlElementDimensions,
				panelGroupHtmlIdTuple.PanelGroupKey,
				elementDimensions));
		}

		DropzoneList = dropzoneList.ToImmutableArray();
	}

    public Task OnDragEndAsync(MouseEventArgs mouseEventArgs, IDropzone? dropzone)
    {
		if (TabGroup is not PanelGroup panelGroup)
			return Task.CompletedTask;

		if (dropzone is not PanelGroupDropzone panelGroupDropzone)
			return Task.CompletedTask;

		if (panelGroupDropzone.PanelGroupKey == Key<PanelGroup>.Empty)
		{
			Dispatcher.Dispatch(new PanelsState.DisposePanelTabAction(
				panelGroup.Key,
				Key));

			DialogService.RegisterDialogRecord(this);
		}
		else
		{
			Dispatcher.Dispatch(new PanelsState.DisposePanelTabAction(
				panelGroup.Key,
				Key));

			var verticalHalfwayPoint = dropzone.MeasuredHtmlElementDimensions.TopInPixels +
				(dropzone.MeasuredHtmlElementDimensions.HeightInPixels / 2);

			var insertAtIndexZero = mouseEventArgs.ClientY < verticalHalfwayPoint
				? true
				: false;

			Dispatcher.Dispatch(new PanelsState.RegisterPanelTabAction(
				panelGroupDropzone.PanelGroupKey,
				this,
				insertAtIndexZero));
		}

		return Task.CompletedTask;
	}

	private void AddFallbackDropzone(List<IDropzone> dropzoneList)
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

		dropzoneList.Add(new PanelGroupDropzone(
			new MeasuredHtmlElementDimensions(0, 0, 0, 0, 0),
			Key<PanelGroup>.Empty,
			fallbackElementDimensions)
		{
			CssClass = "luth_dropzone-fallback"
		});
	}
}
