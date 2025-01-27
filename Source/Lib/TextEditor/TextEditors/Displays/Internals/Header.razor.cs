using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Collections.Immutable;
using Fluxor;
using Luthetus.Common.RazorLib.WatchWindows.Models;
using Luthetus.Common.RazorLib.Dialogs.Models;
using Luthetus.Common.RazorLib.WatchWindows.Displays;
using Luthetus.Common.RazorLib.Clipboards.Models;
using Luthetus.Common.RazorLib.Keys.Models;
using Luthetus.Common.RazorLib.Dynamics.Models;
using Luthetus.TextEditor.RazorLib.Commands.Models;
using Luthetus.TextEditor.RazorLib.TextEditors.Models;
using Luthetus.TextEditor.RazorLib.Cursors.Models;
using Luthetus.TextEditor.RazorLib.TextEditors.Models.Internals;
using Luthetus.TextEditor.RazorLib.Commands.Models.Defaults;
using Luthetus.TextEditor.RazorLib.Installations.Models;

namespace Luthetus.TextEditor.RazorLib.TextEditors.Displays.Internals;

public partial class Header : ComponentBase
{
    [Inject]
    private ITextEditorService TextEditorService { get; set; } = null!;
    [Inject]
    private IClipboardService ClipboardService { get; set; } = null!;
    [Inject]
    private IDialogService DialogService { get; set; } = null!;
    [Inject]
    private IDispatcher Dispatcher { get; set; } = null!;
    [Inject]
    private IServiceProvider ServiceProvider { get; set; } = null!;
    [Inject]
    private LuthetusTextEditorConfig TextEditorConfig { get; set; } = null!;

    [CascadingParameter]
    public TextEditorRenderBatchValidated RenderBatch { get; set; } = null!;

    [Parameter]
    public ImmutableArray<HeaderButtonKind>? HeaderButtonKinds { get; set; }

    private TextEditorCommandArgs ConstructCommandArgs(
        TextEditorModel textEditorModel,
        TextEditorViewModel viewModel)
    {
        var cursorSnapshotsList = new TextEditorCursor[] { viewModel.PrimaryCursor }.ToImmutableArray();
        var hasSelection = TextEditorSelectionHelper.HasSelectedText(cursorSnapshotsList.First(x => x.IsPrimaryCursor).Selection);

        return new TextEditorCommandArgs(
            textEditorModel.ResourceUri,
            viewModel.ViewModelKey,
            RenderBatch.ComponentData,
			TextEditorService,
            ServiceProvider,
            null);
    }

    private Task DoCopyOnClick(MouseEventArgs arg)
    {
        var model = RenderBatch.Model;
        var viewModel = RenderBatch.ViewModel;

        if (model is null || viewModel is null)
            return Task.CompletedTask;

        var commandArgs = ConstructCommandArgs(model, viewModel);

        TextEditorService.PostUnique(
            nameof(TextEditorCommandDefaultFacts.Copy),
            editContext =>
            {
                commandArgs.EditContext = editContext;
                return TextEditorCommandDefaultFacts.Copy.CommandFunc
                    .Invoke(commandArgs);
            });
        return Task.CompletedTask;
    }

    private Task DoCutOnClick(MouseEventArgs arg)
    {
        var model = RenderBatch.Model;
        var viewModel = RenderBatch.ViewModel;

        if (model is null || viewModel is null)
            return Task.CompletedTask;

        var commandArgs = ConstructCommandArgs(model, viewModel);

        TextEditorService.PostUnique(
            nameof(TextEditorCommandDefaultFacts.Cut),
            editContext =>
            {
                commandArgs.EditContext = editContext;
                return TextEditorCommandDefaultFacts.Cut.CommandFunc
                    .Invoke(commandArgs);
            });
        return Task.CompletedTask;
    }

    private Task DoPasteOnClick(MouseEventArgs arg)
    {
        var model = RenderBatch.Model;
        var viewModel = RenderBatch.ViewModel;

        if (model is null || viewModel is null)
            return Task.CompletedTask;

        var commandArgs = ConstructCommandArgs(model, viewModel);

        TextEditorService.PostUnique(
            nameof(TextEditorCommandDefaultFacts.PasteCommand),
            editContext =>
            {
                commandArgs.EditContext = editContext;
                return TextEditorCommandDefaultFacts.PasteCommand.CommandFunc
                    .Invoke(commandArgs);
            });
        return Task.CompletedTask;
    }

    private Task DoRedoOnClick(MouseEventArgs arg)
    {
        var model = RenderBatch.Model;
        var viewModel = RenderBatch.ViewModel;

        if (model is null || viewModel is null)
            return Task.CompletedTask;

        var commandArgs = ConstructCommandArgs(model, viewModel);

        TextEditorService.PostUnique(
            nameof(TextEditorCommandDefaultFacts.Redo),
            editContext =>
            {
                commandArgs.EditContext = editContext;
                return TextEditorCommandDefaultFacts.Redo.CommandFunc
                    .Invoke(commandArgs);
            });
        return Task.CompletedTask;
    }

    private Task DoSaveOnClick(MouseEventArgs arg)
    {
        var model = RenderBatch.Model;
        var viewModel = RenderBatch.ViewModel;

        if (model is null || viewModel is null)
            return Task.CompletedTask;

        var commandArgs = ConstructCommandArgs(model, viewModel);

        TextEditorService.PostUnique(
            nameof(TextEditorCommandDefaultFacts.TriggerSave),
            editContext =>
            {
                commandArgs.EditContext = editContext;
                return TextEditorCommandDefaultFacts.TriggerSave.CommandFunc
                    .Invoke(commandArgs);
            });
        return Task.CompletedTask;
    }

    private Task DoUndoOnClick(MouseEventArgs arg)
    {
        var model = RenderBatch.Model;
        var viewModel = RenderBatch.ViewModel;

        if (model is null || viewModel is null)
            return Task.CompletedTask;

        var commandArgs = ConstructCommandArgs(model, viewModel);

        TextEditorService.PostUnique(
            nameof(TextEditorCommandDefaultFacts.Undo),
            editContext =>
            {
                commandArgs.EditContext = editContext;
                return TextEditorCommandDefaultFacts.Undo.CommandFunc
                    .Invoke(commandArgs);
            });
        return Task.CompletedTask;
    }

    private Task DoSelectAllOnClick(MouseEventArgs arg)
    {
        var model = RenderBatch.Model;
        var viewModel = RenderBatch.ViewModel;

        if (model is null || viewModel is null)
            return Task.CompletedTask;

        var commandArgs = ConstructCommandArgs(model, viewModel);

        TextEditorService.PostUnique(
            nameof(TextEditorCommandDefaultFacts.SelectAll),
            editContext =>
            {
                commandArgs.EditContext = editContext;
                return TextEditorCommandDefaultFacts.SelectAll.CommandFunc
                    .Invoke(commandArgs);
            });
        return Task.CompletedTask;
    }

    private Task DoRemeasureOnClick(MouseEventArgs arg)
    {
        var model = RenderBatch.Model;
        var viewModel = RenderBatch.ViewModel;

        if (model is null || viewModel is null)
            return Task.CompletedTask;

        var commandArgs = ConstructCommandArgs(model, viewModel);

        TextEditorService.PostUnique(
            nameof(TextEditorCommandDefaultFacts.Remeasure),
            editContext =>
            {
                commandArgs.EditContext = editContext;
                return TextEditorCommandDefaultFacts.Remeasure.CommandFunc
                    .Invoke(commandArgs);
            });
        return Task.CompletedTask;
    }

    private void ShowWatchWindowDisplayDialogOnClick()
    {
        var model = RenderBatch.Model;

        if (model is null)
            return;

        var watchWindowObject = new WatchWindowObject(
            RenderBatch,
            typeof(TextEditorRenderBatchValidated),
            nameof(TextEditorRenderBatchValidated),
            true);

        var dialogRecord = new DialogViewModel(
            Key<IDynamicViewModel>.NewKey(),
            $"WatchWindow: {model.ResourceUri}",
            typeof(WatchWindowDisplay),
            new Dictionary<string, object?>
            {
                {
                    nameof(WatchWindowDisplay.WatchWindowObject),
                    watchWindowObject
                }
            },
            null,
			true,
			null);

        DialogService.RegisterDialogRecord(dialogRecord);
    }

    private Task DoRefreshOnClick()
    {
        var model = RenderBatch.Model;
        var viewModel = RenderBatch.ViewModel;

        if (model is null || viewModel is null)
            return Task.CompletedTask;

        var commandArgs = ConstructCommandArgs(model, viewModel);

        TextEditorService.PostUnique(
            nameof(TextEditorCommandDefaultFacts.Remeasure),
            editContext =>
            {
                commandArgs.EditContext = editContext;
                return TextEditorCommandDefaultFacts.Remeasure.CommandFunc
                    .Invoke(commandArgs);
            });
        return Task.CompletedTask;
    }

    /// <summary>
    /// disabled=@GetUndoDisabledAttribute()
    /// will toggle the attribute
    /// <br/><br/>
    /// disabled="@GetUndoDisabledAttribute()"
    /// will toggle the value of the attribute
    /// </summary>
    private bool GetUndoDisabledAttribute()
    {
        var model = RenderBatch.Model;
        var viewModel = RenderBatch.ViewModel;

        if (model is null || viewModel is null)
            return true;

        return !model.CanUndoEdit();
    }

    /// <summary>
    /// disabled=@GetRedoDisabledAttribute()
    /// will toggle the attribute
    /// <br/><br/>
    /// disabled="@GetRedoDisabledAttribute()"
    /// will toggle the value of the attribute
    /// </summary>
    private bool GetRedoDisabledAttribute()
    {
        var model = RenderBatch.Model;
        var viewModel = RenderBatch.ViewModel;

        if (model is null || viewModel is null)
            return true;

        return !model.CanRedoEdit();
    }
}