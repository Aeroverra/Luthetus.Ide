using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Fluxor;
using System.Collections.Immutable;
using System.Text;
using Luthetus.Common.RazorLib.Commands.Models;
using Luthetus.Common.RazorLib.Menus.Models;
using Luthetus.Common.RazorLib.Dropdowns.Models;
using Luthetus.Common.RazorLib.Keys.Models;
using Luthetus.Common.RazorLib.Dimensions.Models;
using Luthetus.Common.RazorLib.BackgroundTasks.Models;
using Luthetus.Common.RazorLib.TreeViews.Models;
using Luthetus.Common.RazorLib.Dynamics.Models;
using Luthetus.Ide.RazorLib.CommandLines.Models;
using Luthetus.Ide.RazorLib.Terminals.Models;
using Luthetus.Ide.RazorLib.Terminals.States;
using Luthetus.Ide.RazorLib.TestExplorers.Models;
using Luthetus.Ide.RazorLib.TestExplorers.States;
using Luthetus.Ide.RazorLib.DotNetSolutions.Models;
using Luthetus.Ide.RazorLib.DotNetSolutions.Models.Internals;

namespace Luthetus.Ide.RazorLib.DotNetSolutions.Displays.Internals;

public partial class SolutionVisualizationContextMenu : ComponentBase
{
	[Parameter, EditorRequired]
    public MouseEventArgs MouseEventArgs { get; set; } = null!;
	[Parameter, EditorRequired]
    public SolutionVisualizationModel SolutionVisualizationModel { get; set; } = null!;

    public static readonly Key<DropdownRecord> ContextMenuEventDropdownKey = Key<DropdownRecord>.NewKey();

	private MenuRecord? _menuRecord = null;

    protected override async Task OnInitializedAsync()
    {
        // Usage of 'OnInitializedAsync' lifecycle method ensure the context menu is only rendered once.
		// Otherwise, one might have the context menu's options change out from under them.
        _menuRecord = await GetMenuRecord(MouseEventArgs).ConfigureAwait(false);
		await InvokeAsync(StateHasChanged);

        await base.OnInitializedAsync();
    }

    private async Task<MenuRecord> GetMenuRecord(MouseEventArgs mouseEventArgs)
    {
        var menuRecordsList = new List<MenuOptionRecord>();

		var localSolutionVisualizationModel = SolutionVisualizationModel;

		// TODO: Have to convert to relative coordinates
		foreach (var drawing in localSolutionVisualizationModel.SolutionVisualizationDrawingList)
		{
			var lowerX = drawing.CenterX - drawing.Radius;
			var upperX = drawing.CenterX + drawing.Radius;

			if (lowerX <= mouseEventArgs.ClientX &&
				upperX >= mouseEventArgs.ClientX)
			{
				var lowerY = drawing.CenterY - drawing.Radius;
				var upperY = drawing.CenterY + drawing.Radius;

				if (lowerY <= mouseEventArgs.ClientY &&
					upperY >= mouseEventArgs.ClientY)
				{
				}
			}
		}

		menuRecordsList.Add(new MenuOptionRecord(
		    "Settings",
		    MenuOptionKind.Other,
		    WidgetRendererType: typeof(SolutionVisualizationSettingsDisplay),
			WidgetParameterMap: new()
			{
				{
					nameof(SolutionVisualizationSettingsDisplay.SolutionVisualizationModel),
					SolutionVisualizationModel
				}
			}));

        if (!menuRecordsList.Any())
            return MenuRecord.Empty;

        return new MenuRecord(menuRecordsList.ToImmutableArray());
    }

    public static string GetContextMenuCssStyleString(
		MouseEventArgs mouseEventArgs,
        IDialog dialogRecord)
    {
        if (mouseEventArgs is null)
            return "display: none;";

        if (dialogRecord.DialogIsMaximized)
        {
            return
                $"left: {mouseEventArgs.ClientX.ToCssValue()}px;" +
                " " +
                $"top: {mouseEventArgs.ClientY.ToCssValue()}px;";
        }
            
        var dialogLeftDimensionAttribute = dialogRecord
            .DialogElementDimensions
            .DimensionAttributeList
            .First(x => x.DimensionAttributeKind == DimensionAttributeKind.Left);

        var contextMenuLeftDimensionAttribute = new DimensionAttribute
        {
            DimensionAttributeKind = DimensionAttributeKind.Left
        };

        contextMenuLeftDimensionAttribute.DimensionUnitList.Add(new DimensionUnit
        {
            DimensionUnitKind = DimensionUnitKind.Pixels,
            Value = mouseEventArgs.ClientX
        });

        foreach (var dimensionUnit in dialogLeftDimensionAttribute.DimensionUnitList)
        {
            contextMenuLeftDimensionAttribute.DimensionUnitList.Add(new DimensionUnit
            {
                Purpose = dimensionUnit.Purpose,
                Value = dimensionUnit.Value,
                DimensionOperatorKind = DimensionOperatorKind.Subtract,
                DimensionUnitKind = dimensionUnit.DimensionUnitKind
            });
        }

        var dialogTopDimensionAttribute = dialogRecord
            .DialogElementDimensions
            .DimensionAttributeList
            .First(x => x.DimensionAttributeKind == DimensionAttributeKind.Top);

        var contextMenuTopDimensionAttribute = new DimensionAttribute
        {
            DimensionAttributeKind = DimensionAttributeKind.Top
        };

        contextMenuTopDimensionAttribute.DimensionUnitList.Add(new DimensionUnit
        {
            DimensionUnitKind = DimensionUnitKind.Pixels,
            Value = mouseEventArgs.ClientY
        });

        foreach (var dimensionUnit in dialogTopDimensionAttribute.DimensionUnitList)
        {
            contextMenuTopDimensionAttribute.DimensionUnitList.Add(new DimensionUnit
            {
                Purpose = dimensionUnit.Purpose,
                Value = dimensionUnit.Value,
                DimensionOperatorKind = DimensionOperatorKind.Subtract,
                DimensionUnitKind = dimensionUnit.DimensionUnitKind
            });
        }

        return $"{contextMenuLeftDimensionAttribute.StyleString} {contextMenuTopDimensionAttribute.StyleString}";
    }
}