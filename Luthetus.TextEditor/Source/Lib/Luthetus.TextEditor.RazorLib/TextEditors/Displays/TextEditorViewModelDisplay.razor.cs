﻿using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Web;
using System.Collections.Immutable;
using Luthetus.Common.RazorLib.JavaScriptObjects.Models;
using Luthetus.Common.RazorLib.Reactives.Models;
using Luthetus.TextEditor.RazorLib.Autocompletes.Models;
using Luthetus.TextEditor.RazorLib.Commands.Models;
using Luthetus.TextEditor.RazorLib.ComponentRenderers.Models;
using Luthetus.TextEditor.RazorLib.Options.Models;
using Luthetus.TextEditor.RazorLib.TextEditors.States;
using Luthetus.TextEditor.RazorLib.TextEditors.Models;
using Luthetus.TextEditor.RazorLib.Options.States;
using Luthetus.TextEditor.RazorLib.Cursors.Models;
using Luthetus.TextEditor.RazorLib.Installations.Models;
using Luthetus.TextEditor.RazorLib.Keymaps.Models;
using Luthetus.TextEditor.RazorLib.TextEditors.Displays.Internals;
using Luthetus.TextEditor.RazorLib.TextEditors.Models.Internals;
using Luthetus.TextEditor.RazorLib.Keymaps.Models.Defaults;
using Luthetus.TextEditor.RazorLib.Lexes.Models;
using Luthetus.TextEditor.RazorLib.Commands.Models.Defaults;
using Luthetus.Common.RazorLib.BackgroundTasks.Models;
using Luthetus.Common.RazorLib.RenderStates.Models;
using Luthetus.Common.RazorLib.Keyboards.Models;
using Luthetus.Common.RazorLib.Keymaps.Models;
using Luthetus.Common.RazorLib.Clipboards.Models;
using Luthetus.Common.RazorLib.Keys.Models;
using Luthetus.Common.RazorLib.Dimensions.Models;
using Luthetus.TextEditor.RazorLib.TextEditors.Models.TextEditorServices;
using Luthetus.TextEditor.RazorLib.TextEditors.Models.TextEditorModels;
using Luthetus.Common.Tests.Basis.Reactives.Models;
using System.Security.Cryptography;
using Luthetus.Common.RazorLib.Commands.Models;
using Microsoft.AspNetCore.Components.Forms;
using System.Threading;

namespace Luthetus.TextEditor.RazorLib.TextEditors.Displays;

public partial class TextEditorViewModelDisplay : ComponentBase, IDisposable
{
    [Inject]
    protected IState<TextEditorModelState> TextEditorModelStateWrap { get; set; } = null!;
    [Inject]
    protected IState<TextEditorViewModelState> TextEditorViewModelsStateWrap { get; set; } = null!;
    [Inject]
    protected IState<TextEditorOptionsState> TextEditorOptionsStateWrap { get; set; } = null!;
    [Inject]
    private IDispatcher Dispatcher { get; set; } = null!;
    [Inject]
    private IServiceProvider ServiceProvider { get; set; } = null!;
    [Inject]
    protected ITextEditorService TextEditorService { get; set; } = null!;
    [Inject]
    private IAutocompleteIndexer AutocompleteIndexer { get; set; } = null!;
    [Inject]
    private IJSRuntime JsRuntime { get; set; } = null!;
    [Inject]
    private IClipboardService ClipboardService { get; set; } = null!;
    [Inject]
    private IBackgroundTaskService BackgroundTaskService { get; set; } = null!;
    [Inject]
    private ILuthetusTextEditorComponentRenderers LuthetusTextEditorComponentRenderers { get; set; } = null!;
    [Inject]
    private LuthetusTextEditorConfig TextEditorConfig { get; set; } = null!;

    [Parameter, EditorRequired]
    public Key<TextEditorViewModel> TextEditorViewModelKey { get; set; } = Key<TextEditorViewModel>.Empty;
    
    [Parameter]
    public TextEditorViewModelDisplayOptions ViewModelDisplayOptions { get; set; } = new();

    private readonly Guid _textEditorHtmlElementId = Guid.NewGuid();
    private readonly IThrottle _throttleApplySyntaxHighlighting = new Throttle(TimeSpan.FromMilliseconds(500));
    private readonly TimeSpan _onMouseOutTooltipDelay = TimeSpan.FromMilliseconds(1_000);
    private readonly TimeSpan _mouseStoppedMovingDelay = TimeSpan.FromMilliseconds(400);
    private readonly ThrottleController _throttleControllerUiEvents = new();
    /// <summary>Using this lock in order to avoid the Dispose implementation decrementing when it shouldn't</summary>
    private readonly object _linkedViewModelLock = new();

    /// <summary>This accounts for one who might hold down Left Mouse Button from outside the TextEditorDisplay's content div then move their mouse over the content div while holding the Left Mouse Button down.</summary>
    private bool _thinksLeftMouseButtonIsDown;
    private bool _thinksTouchIsOccurring;
    private TouchEventArgs? _previousTouchEventArgs = null;
    private DateTime? _touchStartDateTime = null;
    private BodySection? _bodySectionComponent;
    private MeasureCharacterWidthAndRowHeight? _measureCharacterWidthAndRowHeightComponent;
    private Task _mouseStoppedMovingTask = Task.CompletedTask;
    private CancellationTokenSource _mouseStoppedMovingCancellationTokenSource = new();
    private Task _onMouseOutTooltipTask = Task.CompletedTask;
    private CancellationTokenSource _onMouseOutTooltipCancellationTokenSource = new();
    private TooltipViewModel? _tooltipViewModel;
    private bool _userMouseIsInside;
    private TextEditorRenderBatch _storedRenderBatch = null!;
    private TextEditorRenderBatch? _previousRenderBatch;
    private TextEditorViewModel? _linkedViewModel;

    private CursorDisplay? CursorDisplay => _bodySectionComponent?.CursorDisplayComponent;
    private string MeasureCharacterWidthAndRowHeightElementId => $"luth_te_measure-character-width-and-row-height_{_textEditorHtmlElementId}";
    private string ContentElementId => $"luth_te_text-editor-content_{_textEditorHtmlElementId}";
    private string ProportionalFontMeasurementsContainerElementId => $"luth_te_text-editor-proportional-font-measurement-container_{_textEditorHtmlElementId}";

    protected override async Task OnParametersSetAsync()
    {
        HandleTextEditorViewModelKeyChange();

        await base.OnParametersSetAsync().ConfigureAwait(false);
    }

    protected override void OnInitialized()
    {
        ConstructRenderBatch();

        TextEditorViewModelsStateWrap.StateChanged += GeneralOnStateChangedEventHandler;
        TextEditorOptionsStateWrap.StateChanged += GeneralOnStateChangedEventHandler;

        base.OnInitialized();
    }

    protected override bool ShouldRender()
    {
        var shouldRender = base.ShouldRender();

        if (_linkedViewModel is null)
            HandleTextEditorViewModelKeyChange();

        if (shouldRender)
            ConstructRenderBatch();

        if (_storedRenderBatch?.ViewModel is not null && _storedRenderBatch?.Options is not null)
        {
            var isFirstDisplay = _storedRenderBatch.ViewModel.DisplayTracker.ConsumeIsFirstDisplay();

            var previousOptionsRenderStateKey = _previousRenderBatch?.Options?.RenderStateKey ?? Key<RenderState>.Empty;
            var currentOptionsRenderStateKey = _storedRenderBatch.Options.RenderStateKey;

            if (previousOptionsRenderStateKey != currentOptionsRenderStateKey || isFirstDisplay)
            {
                QueueRemeasureBackgroundTask(
                    _storedRenderBatch,
                    MeasureCharacterWidthAndRowHeightElementId,
                    _measureCharacterWidthAndRowHeightComponent?.CountOfTestCharacters ?? 0,
                    CancellationToken.None);
            }

            if (isFirstDisplay)
                QueueCalculateVirtualizationResultBackgroundTask(_storedRenderBatch);
        }

        return shouldRender;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JsRuntime.InvokeVoidAsync(
                    "luthetusTextEditor.preventDefaultOnWheelEvents",
                    ContentElementId)
                .ConfigureAwait(false);

            QueueRemeasureBackgroundTask(
                _storedRenderBatch,
                MeasureCharacterWidthAndRowHeightElementId,
                _measureCharacterWidthAndRowHeightComponent?.CountOfTestCharacters ?? 0,
                CancellationToken.None);

            QueueCalculateVirtualizationResultBackgroundTask(_storedRenderBatch);
        }

        if (_storedRenderBatch?.ViewModel is not null && _storedRenderBatch.ViewModel.ShouldSetFocusAfterNextRender)
        {
            _storedRenderBatch.ViewModel.ShouldSetFocusAfterNextRender = false;
            await FocusTextEditorAsync().ConfigureAwait(false);
        }

        await base.OnAfterRenderAsync(firstRender).ConfigureAwait(false);
    }

    public TextEditorModel? GetModel() => TextEditorService.ViewModelApi.GetModelOrDefault(TextEditorViewModelKey);

    public TextEditorViewModel? GetViewModel() => TextEditorViewModelsStateWrap.Value.ViewModelList.FirstOrDefault(
        x => x.ViewModelKey == TextEditorViewModelKey);

    public TextEditorOptions? GetOptions() => TextEditorOptionsStateWrap.Value.Options;

    private void ConstructRenderBatch()
    {
        var renderBatch = new TextEditorRenderBatch(
            GetModel(),
            GetViewModel(),
            GetOptions(),
            TextEditorRenderBatch.DEFAULT_FONT_FAMILY,
            TextEditorOptionsState.DEFAULT_FONT_SIZE_IN_PIXELS);

        if (!string.IsNullOrWhiteSpace(renderBatch.Options?.CommonOptions?.FontFamily))
        {
            renderBatch = renderBatch with
            {
                FontFamily = renderBatch.Options!.CommonOptions!.FontFamily
            };
        }

        if (renderBatch.Options!.CommonOptions?.FontSizeInPixels is not null)
        {
            renderBatch = renderBatch with
            {
                FontSizeInPixels = renderBatch.Options!.CommonOptions.FontSizeInPixels
            };
        }

        _previousRenderBatch = _storedRenderBatch;
        _storedRenderBatch = renderBatch;
    }

    private async void GeneralOnStateChangedEventHandler(object? sender, EventArgs e) =>
        await InvokeAsync(StateHasChanged).ConfigureAwait(false);

    private void HandleTextEditorViewModelKeyChange()
    {
        lock (_linkedViewModelLock)
        {
            var localTextEditorViewModelKey = TextEditorViewModelKey;

            // Don't use the method 'GetViewModel()'. The logic here needs to be transactional, the TextEditorViewModelKey must not change.
            var nextViewModel = TextEditorViewModelsStateWrap.Value.ViewModelList.FirstOrDefault(
                x => x.ViewModelKey == localTextEditorViewModelKey);

            Key<TextEditorViewModel> nextViewModelKey;

            if (nextViewModel is null)
                nextViewModelKey = Key<TextEditorViewModel>.Empty;
            else
                nextViewModelKey = nextViewModel.ViewModelKey;

            var linkedViewModelKey = _linkedViewModel?.ViewModelKey ?? Key<TextEditorViewModel>.Empty;

            var viewKeyChanged = nextViewModelKey != linkedViewModelKey;

            if (viewKeyChanged)
            {
                _linkedViewModel?.DisplayTracker.DecrementLinks(TextEditorModelStateWrap);
                nextViewModel?.DisplayTracker.IncrementLinks(TextEditorModelStateWrap);

                _linkedViewModel = nextViewModel;

                if (nextViewModel is not null)
                    nextViewModel.PrimaryCursor.ShouldRevealCursor = true;
            }
        }
    }

    public async Task FocusTextEditorAsync()
    {
        if (CursorDisplay is not null)
            await CursorDisplay.FocusAsync().ConfigureAwait(false);
    }

    private bool CheckIfKeyboardEventArgsIsNoise(KeyboardEventArgs keyboardEventArgs)
    {
        if (keyboardEventArgs.Key == "Shift" ||
            keyboardEventArgs.Key == "Control" ||
            keyboardEventArgs.Key == "Alt" ||
            keyboardEventArgs.Key == "Meta")
        {
            return true;
        }

        if (keyboardEventArgs.CtrlKey && keyboardEventArgs.AltKey)
        {
            // TODO: This if is a hack to fix the keybind: { Ctrl + Alt + S } causing...
            // ...an 's' to be written out when using Vim keymap.
            return true;
        }

        return false;
    }

    private bool CheckIfKeyboardEventArgsMapsToCommand(
        KeyboardEventArgs keyboardEventArgs,
        bool hasSelection,
        out Key<KeymapLayer> layerKey,
        out KeymapArgument keymapArgument,
        out bool success,
        out CommandNoType command)
    {
        layerKey = ((ITextEditorKeymap)TextEditorService.OptionsStateWrap.Value.Options.Keymap!).GetLayer(hasSelection);

        keymapArgument = keyboardEventArgs.ToKeymapArgument() with
        {
            LayerKey = layerKey
        };

        success = TextEditorService.OptionsStateWrap.Value.Options.Keymap!.Map.TryGetValue(
            keymapArgument,
            out command);

        if (!success && keymapArgument.LayerKey != TextEditorKeymapDefaultFacts.DefaultLayer.Key)
        {
            _ = TextEditorService.OptionsStateWrap.Value.Options.Keymap!.Map.TryGetValue(
                keymapArgument with
                {
                    LayerKey = TextEditorKeymapDefaultFacts.DefaultLayer.Key,
                },
                out command);
        }

        if (KeyboardKeyFacts.WhitespaceCodes.ENTER_CODE == keyboardEventArgs.Code && keyboardEventArgs.ShiftKey)
            command = TextEditorCommandDefaultFacts.NewLineBelow;

        return command is not null;
    }

    private bool CheckIfKeyboardEventArgsMapsToMovement(KeyboardEventArgs keyboardEventArgs, CommandNoType command)
    {
        return KeyboardKeyFacts.IsMovementKey(keyboardEventArgs.Key) && command is null;
    }
    
    private void HandleOnKeyDown(KeyboardEventArgs keyboardEventArgs)
    {
        if (CheckIfKeyboardEventArgsIsNoise(keyboardEventArgs))
            return;

        // TODO: I need to figure out how to ensure a TextEditorModel which is available from...
        // ...within a 'TextEditorService.Post' invocation via closure is not used when one...
        // ...actually meant to use their 'modelModifier'.
        var resourceUri = GetModel()?.ResourceUri;
        var viewModelKey = GetViewModel()?.ViewModelKey;

        if (resourceUri is null || viewModelKey is null)
            return;

        _throttleControllerUiEvents.FireAndForget(new ThrottleEvent<(Key<TextEditorViewModel> viewModelKey, List<KeyboardEventArgs> keyboardEventsList)>(
            nameof(HandleContentOnMouseMove),
            TimeSpan.FromMilliseconds(25),
            (viewModelKey.Value, new List<KeyboardEventArgs> { keyboardEventArgs }),
            (throttleEvent, throttleDelayCancellationToken) =>
            {
                TextEditorService.Post(
                    nameof(HandleOnKeyDown),
                    async editContext =>
                    {
                        var modelModifier = editContext.GetModelModifier(resourceUri);
                        var viewModelModifier = editContext.GetViewModelModifier(viewModelKey.Value);

                        if (modelModifier is null || viewModelModifier is null)
                            return;

                        var cursorModifierBag = editContext.GetCursorModifierBag(viewModelModifier.ViewModel);
                        var primaryCursorModifier = editContext.GetPrimaryCursorModifier(cursorModifierBag);

                        if (primaryCursorModifier is null)
                            return;

                        var hasSelection = TextEditorSelectionHelper.HasSelectedText(primaryCursorModifier);

                        CheckIfKeyboardEventArgsMapsToCommand(
                            keyboardEventArgs,
                            hasSelection,
                            out var layerKey,
                            out var keymapArgument,
                            out var success,
                            out var command);

                        if (CheckIfKeyboardEventArgsMapsToMovement(keyboardEventArgs, command))
                        {
                            if ((KeyboardKeyFacts.MovementKeys.ARROW_DOWN == keyboardEventArgs.Key ||
                                    KeyboardKeyFacts.MovementKeys.ARROW_UP == keyboardEventArgs.Key) &&
                                CursorDisplay is not null &&
                                CursorDisplay.MenuKind == TextEditorMenuKind.AutoCompleteMenu)
                            {
                                await CursorDisplay.SetFocusToActiveMenuAsync().ConfigureAwait(false);
                            }
                            else
                            {
                                await TextEditorService.ViewModelApi.MoveCursorFactory(
                                        keyboardEventArgs,
                                        modelModifier.ResourceUri,
                                        viewModelModifier.ViewModel.ViewModelKey)
                                    .Invoke(editContext)
                                    .ConfigureAwait(false);

                                CursorDisplay?.SetShouldDisplayMenuAsync(TextEditorMenuKind.None);
                            }
                        }
                        else if (KeyboardKeyFacts.CheckIsContextMenuEvent(keyboardEventArgs))
                        {
                            CursorDisplay?.SetShouldDisplayMenuAsync(TextEditorMenuKind.ContextMenu);
                        }
                        else
                        {
                            if (command is not null)
                            {
                                await command.CommandFunc.Invoke(new TextEditorCommandArgs(
                                        modelModifier.ResourceUri,
                                        viewModelModifier.ViewModel.ViewModelKey,
                                        hasSelection,
                                        ClipboardService,
                                        TextEditorService,
                                        HandleMouseStoppedMovingEventAsync,
                                        JsRuntime,
                                        Dispatcher,
                                        ServiceProvider,
                                        TextEditorConfig))
                                    .ConfigureAwait(false);
                            }
                            else
                            {
                                if (!IsAutocompleteMenuInvoker(keyboardEventArgs))
                                {
                                    if (!KeyboardKeyFacts.IsMetaKey(keyboardEventArgs)
                                        || KeyboardKeyFacts.MetaKeys.ESCAPE == keyboardEventArgs.Key ||
                                            KeyboardKeyFacts.MetaKeys.BACKSPACE == keyboardEventArgs.Key ||
                                            KeyboardKeyFacts.MetaKeys.DELETE == keyboardEventArgs.Key)
                                    {
                                        CursorDisplay?.SetShouldDisplayMenuAsync(TextEditorMenuKind.None);
                                    }
                                }

                                _tooltipViewModel = null;

                                await TextEditorService.ModelApi.HandleKeyboardEventFactory(
                                        resourceUri,
                                        viewModelKey.Value,
                                        keyboardEventArgs,
                                        CancellationToken.None)
                                    .Invoke(editContext)
                                    .ConfigureAwait(false);
                            }
                        }

                        if (keyboardEventArgs.Key != "Shift" &&
                            keyboardEventArgs.Key != "Control" &&
                            keyboardEventArgs.Key != "Alt")
                        {
                            if (command is null ||
                                command is TextEditorCommand commandTextEditor &&
                                commandTextEditor.ShouldScrollCursorIntoView)
                            {
                                primaryCursorModifier.ShouldRevealCursor = true;
                            }
                        }

                        var cursorDisplay = CursorDisplay;

                        if (cursorDisplay is not null)
                        {
                            if (throttleEvent is ThrottleEvent<List<KeyboardEventArgs>> throttleEventKeyboardEventArgsList)
                            {
                                if (throttleEventKeyboardEventArgsList.Item.Count > 1)
                                {
                                    var afterOnKeyDownRangeAsyncFactory = ViewModelDisplayOptions.AfterOnKeyDownRangeAsyncFactory ?? HandleAfterOnKeyDownRangeAsyncFactory;

                                    await afterOnKeyDownRangeAsyncFactory.Invoke(
                                            modelModifier.ResourceUri,
                                            viewModelModifier.ViewModel.ViewModelKey,
                                            throttleEventKeyboardEventArgsList.Item,
                                            cursorDisplay.SetShouldDisplayMenuAsync)
                                        .Invoke(editContext)
                                        .ConfigureAwait(false);
                                }
                                else
                                {
                                    var afterOnKeyDownAsyncFactory = ViewModelDisplayOptions.AfterOnKeyDownAsyncFactory ?? HandleAfterOnKeyDownAsyncFactory;

                                    await afterOnKeyDownAsyncFactory.Invoke(
                                            modelModifier.ResourceUri,
                                            viewModelModifier.ViewModel.ViewModelKey,
                                            keyboardEventArgs,
                                            cursorDisplay.SetShouldDisplayMenuAsync)
                                        .Invoke(editContext)
                                        .ConfigureAwait(false);
                                }
                            }
                        }
                    });

                return Task.CompletedTask;
            },
            tuple =>
            {
                if (tuple.OldEvent is ThrottleEvent<(Key<TextEditorViewModel> viewModelKey, List<KeyboardEventArgs> keyboardEventsList)> oldEventWithType &&
                    tuple.RecentEvent is ThrottleEvent<(Key<TextEditorViewModel> viewModelKey, List<KeyboardEventArgs> keyboardEventsList)> recentEventWithType)
                {
                    // Avoid taking keypresses such as 'Backspace' and treating them as text.
                    if (recentEventWithType.Item.keyboardEventsList.Last().Key.Length != 1)
                        return null;

                    // Avoid external state mutations with local variables.
                    var oldEventWithTypeViewModelKey = oldEventWithType.Item.viewModelKey;
                    var recentEventWithTypeViewModelKey = recentEventWithType.Item.viewModelKey;
                    var viewModelKey = oldEventWithTypeViewModelKey;

                    if (oldEventWithTypeViewModelKey != recentEventWithTypeViewModelKey)
                        return null;

                    var badViewModel = TextEditorService.ViewModelApi.GetOrDefault(viewModelKey);

                    if (badViewModel is null)
                        return null;

                    var hasSelection = TextEditorSelectionHelper.HasSelectedText(badViewModel.PrimaryCursor.Selection);

                    Key<KeymapLayer> layerKey;
                    KeymapArgument keymapArgument;

                    var oldEventIsCommand = CheckIfKeyboardEventArgsMapsToCommand(
                        keyboardEventArgs,
                        hasSelection,
                        out layerKey,
                        out keymapArgument,
                        out var oldEventSuccess,
                        out var oldEventCommand);
                    
                    var recentEventIsCommand = CheckIfKeyboardEventArgsMapsToCommand(
                        keyboardEventArgs,
                        hasSelection,
                        out layerKey,
                        out keymapArgument,
                        out var recentEventSuccess,
                        out var recentEventCommand);
                    
                    var oldEventIsMovement = CheckIfKeyboardEventArgsMapsToMovement(keyboardEventArgs, oldEventCommand);
                    var recentEventIsMovement = CheckIfKeyboardEventArgsMapsToMovement(keyboardEventArgs, recentEventCommand);

                    if (oldEventIsMovement && recentEventIsMovement)
                    {
                        // TODO: Batch 'movement'
                    }
                    else if (KeyboardKeyFacts.CheckIsContextMenuEvent(keyboardEventArgs))
                    {
                        // TODO: Decide what 'context menu' means in the context of 'batching'
                    }
                    else
                    {
                        if (oldEventCommand is not null && recentEventCommand is not null)
                        {
                            // TODO: Decide what 'command' means in the context of 'batching'
                        }
                        else
                        {
                            if (!IsAutocompleteMenuInvoker(keyboardEventArgs))
                            {
                                if (!KeyboardKeyFacts.IsMetaKey(keyboardEventArgs)
                                    || KeyboardKeyFacts.MetaKeys.ESCAPE == keyboardEventArgs.Key ||
                                        KeyboardKeyFacts.MetaKeys.BACKSPACE == keyboardEventArgs.Key ||
                                        KeyboardKeyFacts.MetaKeys.DELETE == keyboardEventArgs.Key)
                                {
                                    // TODO: Decide what '...SetShouldDisplayMenuAsync...;' means in the context of 'batching'
                                    CursorDisplay?.SetShouldDisplayMenuAsync(TextEditorMenuKind.None);
                                }
                            }

                            _tooltipViewModel = null;

                            oldEventWithType.Item.keyboardEventsList.AddRange(recentEventWithType.Item.keyboardEventsList);

                            TextEditorService.Post(
                                "batch_" + nameof(HandleOnKeyDown),
                                editContext =>
                                {
                                    var modelModifier = editContext.GetModelModifierByViewModelKey(viewModelKey);
                                    var viewModelModifier = editContext.GetViewModelModifier(viewModelKey);

                                    if (modelModifier is null || viewModelModifier is null)
                                        return Task.CompletedTask;

                                    var cursorModifierBag = editContext.GetCursorModifierBag(viewModelModifier.ViewModel);
                                    var primaryCursorModifier = editContext.GetPrimaryCursorModifier(cursorModifierBag);

                                    if (cursorModifierBag is null || primaryCursorModifier is null)
                                        return Task.CompletedTask;

                                    modelModifier.EditByInsertion(
                                        string.Join(string.Empty, oldEventWithType.Item.keyboardEventsList.Select(x => x.Key)),
                                        cursorModifierBag,
                                        CancellationToken.None);

                                    return Task.CompletedTask;
                                });
                            
                            return (IThrottleEvent?)oldEventWithType;
                        }
                    }
                }

                return null;
            }));
    }

    private void HandleOnContextMenuAsync()
    {
        CursorDisplay?.SetShouldDisplayMenuAsync(TextEditorMenuKind.ContextMenu);
    }

    private void HandleContentOnDoubleClick(MouseEventArgs mouseEventArgs)
    {
        var modelResourceUri = GetModel()?.ResourceUri;
        var viewModelKey = GetViewModel()?.ViewModelKey;

        if (modelResourceUri is null || viewModelKey is null)
            return;

        _throttleControllerUiEvents.FireAndForget(new ThrottleEvent<byte>(
            nameof(HandleContentOnDoubleClick),
            TimeSpan.FromMilliseconds(25),
            0,
            (throttleEvent, _) =>
            {
                TextEditorService.Post(
                    nameof(HandleContentOnDoubleClick),
                    async editContext =>
                    {
                        var modelModifier = editContext.GetModelModifier(modelResourceUri, true);
                        var viewModelModifier = editContext.GetViewModelModifier(viewModelKey.Value);

                        if (modelModifier is null || viewModelModifier is null)
                            return;

                        var cursorModifierBag = editContext.GetCursorModifierBag(viewModelModifier.ViewModel);
                        var primaryCursorModifier = editContext.GetPrimaryCursorModifier(cursorModifierBag);

                        if (cursorModifierBag is null || primaryCursorModifier is null)
                            return;

                        var hasSelectedText = TextEditorSelectionHelper.HasSelectedText(primaryCursorModifier);

                        if ((mouseEventArgs.Buttons & 1) != 1 && hasSelectedText)
                            return; // Not pressing the left mouse button so assume ContextMenu is desired result.

                        if (mouseEventArgs.ShiftKey)
                            return; // Do not expand selection if user is holding shift

                        var rowAndColumnIndex = await CalculateRowAndColumnIndex(mouseEventArgs).ConfigureAwait(false);

                        var lowerColumnIndexExpansion = modelModifier.GetColumnIndexOfCharacterWithDifferingKind(
                            rowAndColumnIndex.rowIndex,
                            rowAndColumnIndex.columnIndex,
                            true);

                        lowerColumnIndexExpansion = lowerColumnIndexExpansion == -1
                            ? 0
                            : lowerColumnIndexExpansion;

                        var higherColumnIndexExpansion = modelModifier.GetColumnIndexOfCharacterWithDifferingKind(
                            rowAndColumnIndex.rowIndex,
                            rowAndColumnIndex.columnIndex,
                            false);

                        higherColumnIndexExpansion = higherColumnIndexExpansion == -1
                                ? modelModifier.GetLengthOfRow(rowAndColumnIndex.rowIndex)
                                : higherColumnIndexExpansion;

                        // Move user's cursor position to the higher expansion
                        {
                            primaryCursorModifier.RowIndex = rowAndColumnIndex.rowIndex;
                            primaryCursorModifier.ColumnIndex = higherColumnIndexExpansion;
                            primaryCursorModifier.PreferredColumnIndex = rowAndColumnIndex.columnIndex;
                        }

                        // Set text selection ending to higher expansion
                        {
                            var cursorPositionOfHigherExpansion = modelModifier.GetPositionIndex(
                                rowAndColumnIndex.rowIndex,
                                higherColumnIndexExpansion);

                            primaryCursorModifier.SelectionEndingPositionIndex = cursorPositionOfHigherExpansion;
                        }

                        // Set text selection anchor to lower expansion
                        {
                            var cursorPositionOfLowerExpansion = modelModifier.GetPositionIndex(
                                rowAndColumnIndex.rowIndex,
                                lowerColumnIndexExpansion);

                            primaryCursorModifier.SelectionAnchorPositionIndex = cursorPositionOfLowerExpansion;
                        }
                    });

                return Task.CompletedTask;
            },
            tuple => tuple.RecentEvent));
    }

    private void HandleContentOnMouseDown(MouseEventArgs mouseEventArgs)
    {
        var modelResourceUri = GetModel()?.ResourceUri;
        var viewModelKey = GetViewModel()?.ViewModelKey;

        if (modelResourceUri is null || viewModelKey is null)
            return;

        _throttleControllerUiEvents.FireAndForget(new ThrottleEvent<byte>(
            nameof(HandleContentOnMouseDown),
            TimeSpan.FromMilliseconds(25),
            0,
            (throttleEvent, _) =>
            {
                TextEditorService.Post(
                    nameof(HandleContentOnMouseDown),
                    async editContext =>
                    {
                        var modelModifier = editContext.GetModelModifier(modelResourceUri, true);
                        var viewModelModifier = editContext.GetViewModelModifier(viewModelKey.Value);

                        if (modelModifier is null || viewModelModifier is null)
                            return;

                        var cursorModifierBag = editContext.GetCursorModifierBag(viewModelModifier.ViewModel);
                        var primaryCursorModifier = editContext.GetPrimaryCursorModifier(cursorModifierBag);

                        if (cursorModifierBag is null || primaryCursorModifier is null)
                            return;

                        var hasSelectedText = TextEditorSelectionHelper.HasSelectedText(primaryCursorModifier);

                        if ((mouseEventArgs.Buttons & 1) != 1 && hasSelectedText)
                            return; // Not pressing the left mouse button so assume ContextMenu is desired result.

                        CursorDisplay?.SetShouldDisplayMenuAsync(TextEditorMenuKind.None, false);

                        // Remember the current cursor position prior to doing anything
                        var inRowIndex = primaryCursorModifier.RowIndex;
                        var inColumnIndex = primaryCursorModifier.ColumnIndex;

                        // Move the cursor position
                        var rowAndColumnIndex = await CalculateRowAndColumnIndex(mouseEventArgs).ConfigureAwait(false);
                        primaryCursorModifier.RowIndex = rowAndColumnIndex.rowIndex;
                        primaryCursorModifier.ColumnIndex = rowAndColumnIndex.columnIndex;
                        primaryCursorModifier.PreferredColumnIndex = rowAndColumnIndex.columnIndex;

                        CursorDisplay?.PauseBlinkAnimation();

                        var cursorPositionIndex = modelModifier.GetPositionIndex(new TextEditorCursor(
                            rowAndColumnIndex.rowIndex,
                            rowAndColumnIndex.columnIndex,
                            true));

                        if (mouseEventArgs.ShiftKey)
                        {
                            if (!hasSelectedText)
                            {
                                // If user does not yet have a selection then place the text selection anchor were they were
                                primaryCursorModifier.SelectionAnchorPositionIndex = modelModifier
                                    .GetPositionIndex(inRowIndex, inColumnIndex);
                            }

                            // If user ALREADY has a selection then do not modify the text selection anchor
                        }
                        else
                        {
                            primaryCursorModifier.SelectionAnchorPositionIndex = cursorPositionIndex;
                        }

                        primaryCursorModifier.SelectionEndingPositionIndex = cursorPositionIndex;

                        _thinksLeftMouseButtonIsDown = true;
                    });


                return Task.CompletedTask;
            },
            tuple => tuple.RecentEvent));
    }

    /// <summary>OnMouseUp is un-necessary</summary>
    private void HandleContentOnMouseMove(MouseEventArgs mouseEventArgs)
    {
        _userMouseIsInside = true;
        var localThinksLeftMouseButtonIsDown = _thinksLeftMouseButtonIsDown;

        // MouseStoppedMovingEvent
        {
            if (_tooltipViewModel is not null && _onMouseOutTooltipTask.IsCompleted)
            {
                var onMouseOutTooltipCancellationToken = _onMouseOutTooltipCancellationTokenSource.Token;

                _onMouseOutTooltipTask = Task.Run(async () =>
                {
                    await Task.Delay(_onMouseOutTooltipDelay, onMouseOutTooltipCancellationToken).ConfigureAwait(false);

                    if (!onMouseOutTooltipCancellationToken.IsCancellationRequested)
                    {
                        _tooltipViewModel = null;
                        await InvokeAsync(StateHasChanged).ConfigureAwait(false);
                    }
                });
            }

            _mouseStoppedMovingCancellationTokenSource.Cancel();
            _mouseStoppedMovingCancellationTokenSource = new();

            var cancellationToken = _mouseStoppedMovingCancellationTokenSource.Token;

            _mouseStoppedMovingTask = Task.Run(async () =>
            {
                await Task.Delay(_mouseStoppedMovingDelay, cancellationToken).ConfigureAwait(false);

                if (!cancellationToken.IsCancellationRequested && _userMouseIsInside)
                {
                    await HandleOnTooltipMouseOverAsync().ConfigureAwait(false);
                    await HandleMouseStoppedMovingEventAsync(mouseEventArgs).ConfigureAwait(false);
                }
            });
        }

        if (!_thinksLeftMouseButtonIsDown)
            return;

        var modelResourceUri = GetModel()?.ResourceUri;
        var viewModelKey = GetViewModel()?.ViewModelKey;

        if (modelResourceUri is null || viewModelKey is null)
            return;

        // Buttons is a bit flag '& 1' gets if left mouse button is held
        if (localThinksLeftMouseButtonIsDown && (mouseEventArgs.Buttons & 1) == 1)
        {
            _throttleControllerUiEvents.FireAndForget(new ThrottleEvent<byte>(
                nameof(HandleContentOnMouseMove),
                TimeSpan.FromMilliseconds(25),
                0,
                (throttleEvent, _) =>
                {
                    TextEditorService.Post(
                        nameof(HandleContentOnMouseMove),
                        async editContext =>
                        {
                            var modelModifier = editContext.GetModelModifier(modelResourceUri, true);
                            var viewModelModifier = editContext.GetViewModelModifier(viewModelKey.Value);

                            if (modelModifier is null || viewModelModifier is null)
                                return;

                            var cursorModifierBag = editContext.GetCursorModifierBag(viewModelModifier.ViewModel);
                            var primaryCursorModifier = editContext.GetPrimaryCursorModifier(cursorModifierBag);

                            if (cursorModifierBag is null || primaryCursorModifier is null)
                                return;

                            var rowAndColumnIndex = await CalculateRowAndColumnIndex(mouseEventArgs).ConfigureAwait(false);

                            primaryCursorModifier.RowIndex = rowAndColumnIndex.rowIndex;
                            primaryCursorModifier.ColumnIndex = rowAndColumnIndex.columnIndex;
                            primaryCursorModifier.PreferredColumnIndex = rowAndColumnIndex.columnIndex;

                            CursorDisplay?.PauseBlinkAnimation();

                            primaryCursorModifier.SelectionEndingPositionIndex = modelModifier.GetPositionIndex(primaryCursorModifier);
                        });

                    return Task.CompletedTask;
                },
                tuple => tuple.RecentEvent));
        }
        else
        {
            _thinksLeftMouseButtonIsDown = false;
        }
    }

    private void HandleContentOnMouseOut(MouseEventArgs mouseEventArgs)
    {
        _userMouseIsInside = false;
    }

    private async Task<(int rowIndex, int columnIndex)> CalculateRowAndColumnIndex(MouseEventArgs mouseEventArgs)
    {
        var model = GetModel();
        var viewModel = GetViewModel();
        var globalTextEditorOptions = TextEditorService.OptionsStateWrap.Value.Options;

        if (model is null || viewModel is null)
            return (0, 0);

        var charMeasurements = viewModel.VirtualizationResult.CharAndRowMeasurements;

        var relativeCoordinatesOnClick = await JsRuntime.InvokeAsync<RelativeCoordinates>(
                "luthetusTextEditor.getRelativePosition",
                viewModel.BodyElementId,
                mouseEventArgs.ClientX,
                mouseEventArgs.ClientY)
            .ConfigureAwait(false);

        var positionX = relativeCoordinatesOnClick.RelativeX;
        var positionY = relativeCoordinatesOnClick.RelativeY;

        // Scroll position offset
        {
            positionX += relativeCoordinatesOnClick.RelativeScrollLeft;
            positionY += relativeCoordinatesOnClick.RelativeScrollTop;
        }

        var rowIndex = (int)(positionY / charMeasurements.RowHeight);

        rowIndex = rowIndex > model.RowCount - 1
            ? model.RowCount - 1
            : rowIndex;

        int columnIndexInt;

        if (!globalTextEditorOptions.UseMonospaceOptimizations)
        {
            var guid = Guid.NewGuid();

            columnIndexInt = await JsRuntime.InvokeAsync<int>(
                    "luthetusTextEditor.calculateProportionalColumnIndex",
                    ProportionalFontMeasurementsContainerElementId,
                    $"luth_te_proportional-font-measurement-parent_{_textEditorHtmlElementId}_{guid}",
                    $"luth_te_proportional-font-measurement-cursor_{_textEditorHtmlElementId}_{guid}",
                    positionX,
                    charMeasurements.CharacterWidth,
                    model.GetLine(rowIndex))
                .ConfigureAwait(false);

            if (columnIndexInt == -1)
            {
                var columnIndexDouble = positionX / charMeasurements.CharacterWidth;
                columnIndexInt = (int)Math.Round(columnIndexDouble, MidpointRounding.AwayFromZero);
            }
        }
        else
        {
            var columnIndexDouble = positionX / charMeasurements.CharacterWidth;
            columnIndexInt = (int)Math.Round(columnIndexDouble, MidpointRounding.AwayFromZero);
        }

        var lengthOfRow = model.GetLengthOfRow(rowIndex);

        // Tab key column offset
        {
            var parameterForGetTabsCountOnSameRowBeforeCursor = columnIndexInt > lengthOfRow
                ? lengthOfRow
                : columnIndexInt;

            var tabsOnSameRowBeforeCursor = model.GetTabsCountOnSameRowBeforeCursor(
                rowIndex,
                parameterForGetTabsCountOnSameRowBeforeCursor);

            // 1 of the character width is already accounted for
            var extraWidthPerTabKey = TextEditorModel.TAB_WIDTH - 1;

            columnIndexInt -= extraWidthPerTabKey * tabsOnSameRowBeforeCursor;
        }

        columnIndexInt = columnIndexInt > lengthOfRow
            ? lengthOfRow
            : columnIndexInt;

        rowIndex = Math.Max(rowIndex, 0);
        columnIndexInt = Math.Max(columnIndexInt, 0);

        return (rowIndex, columnIndexInt);
    }

    /// <summary>The default <see cref="AfterOnKeyDownAsync"/> will provide syntax highlighting, and autocomplete.<br/><br/>The syntax highlighting occurs on ';', whitespace, paste, undo, redo<br/><br/>The autocomplete occurs on LetterOrDigit typed or { Ctrl + Space }. Furthermore, the autocomplete is done via <see cref="IAutocompleteService"/> and the one can provide their own implementation when registering the Luthetus.TextEditor services using <see cref="LuthetusTextEditorConfig.AutocompleteServiceFactory"/></summary>
    public TextEditorEdit HandleAfterOnKeyDownAsyncFactory(
        ResourceUri resourceUri,
        Key<TextEditorViewModel> viewModelKey,
        KeyboardEventArgs keyboardEventArgs,
        Func<TextEditorMenuKind, bool, Task> setTextEditorMenuKind)
    {
        return async editContext =>
        {
            var modelModifier = editContext.GetModelModifier(resourceUri);
            var viewModelModifier = editContext.GetViewModelModifier(viewModelKey);

            if (modelModifier is null || viewModelModifier is null)
                return;

            var cursorModifierBag = editContext.GetCursorModifierBag(viewModelModifier.ViewModel);
            var primaryCursorModifier = editContext.GetPrimaryCursorModifier(cursorModifierBag);

            if (primaryCursorModifier is null)
                return;

            // Indexing can be invoked and this method still check for syntax highlighting and such
            if (IsAutocompleteIndexerInvoker(keyboardEventArgs))
            {
                _ = Task.Run(async () => 
                {
                    if (primaryCursorModifier.ColumnIndex > 0)
                    {
                        // All keyboardEventArgs that return true from "IsAutocompleteIndexerInvoker"
                        // are to be 1 character long, as well either specific whitespace or punctuation.
                        // Therefore 1 character behind might be a word that can be indexed.
                        var word = modelModifier.ReadPreviousWordOrDefault(
                            primaryCursorModifier.RowIndex,
                            primaryCursorModifier.ColumnIndex);

                        if (word is not null)
                            await AutocompleteIndexer.IndexWordAsync(word).ConfigureAwait(false);
                    }
                }).ConfigureAwait(false);
            }

            if (IsAutocompleteMenuInvoker(keyboardEventArgs))
            {
                await setTextEditorMenuKind.Invoke(TextEditorMenuKind.AutoCompleteMenu, true).ConfigureAwait(false);
            }
            else if (IsSyntaxHighlightingInvoker(keyboardEventArgs))
            {
                _throttleApplySyntaxHighlighting.FireAndForget(async _ =>
                {
                    // The TextEditorModel may have been changed by the time this logic is ran and
                    // thus the local variables must be updated accordingly.
                    var model = GetModel();
                    var viewModel = GetViewModel();

                    if (model is not null)
                    {
                        await modelModifier.ApplySyntaxHighlightingAsync().ConfigureAwait(false);

                        if (viewModel is not null && model.CompilerService is not null)
                            model.CompilerService.ResourceWasModified(model.ResourceUri, ImmutableArray<TextEditorTextSpan>.Empty);
                    }
                });
            }
        };
    }
    
    public TextEditorEdit HandleAfterOnKeyDownRangeAsyncFactory(
        ResourceUri resourceUri,
        Key<TextEditorViewModel> viewModelKey,
        List<KeyboardEventArgs> keyboardEventArgsList,
        Func<TextEditorMenuKind, bool, Task> setTextEditorMenuKind)
    {
        return async editContext =>
        {
            var modelModifier = editContext.GetModelModifier(resourceUri);
            var viewModelModifier = editContext.GetViewModelModifier(viewModelKey);

            if (modelModifier is null || viewModelModifier is null)
                return;

            var cursorModifierBag = editContext.GetCursorModifierBag(viewModelModifier.ViewModel);
            var primaryCursorModifier = editContext.GetPrimaryCursorModifier(cursorModifierBag);

            if (primaryCursorModifier is null)
                return;

            var seenIsAutocompleteIndexerInvoker = false;
            var seenIsAutocompleteMenuInvoker = false;
            var seenIsSyntaxHighlightingInvoker = false;

            foreach (var keyboardEventArgs in keyboardEventArgsList)
            {
                if (!seenIsAutocompleteIndexerInvoker && IsAutocompleteIndexerInvoker(keyboardEventArgs))
                    seenIsAutocompleteIndexerInvoker = true;

                if (!seenIsAutocompleteMenuInvoker && IsAutocompleteMenuInvoker(keyboardEventArgs))
                    seenIsAutocompleteMenuInvoker = true;
                else if (!seenIsSyntaxHighlightingInvoker && IsSyntaxHighlightingInvoker(keyboardEventArgs))
                    seenIsSyntaxHighlightingInvoker = true;
            }

            if (seenIsAutocompleteIndexerInvoker)
            {
                _ = Task.Run(async () =>
                {
                    if (primaryCursorModifier.ColumnIndex > 0)
                    {
                        // All keyboardEventArgs that return true from "IsAutocompleteIndexerInvoker"
                        // are to be 1 character long, as well either specific whitespace or punctuation.
                        // Therefore 1 character behind might be a word that can be indexed.
                        var word = modelModifier.ReadPreviousWordOrDefault(
                            primaryCursorModifier.RowIndex,
                            primaryCursorModifier.ColumnIndex);

                        if (word is not null)
                            await AutocompleteIndexer.IndexWordAsync(word).ConfigureAwait(false);
                    }
                }).ConfigureAwait(false);
            }

            if (seenIsAutocompleteMenuInvoker)
            {
                await setTextEditorMenuKind.Invoke(TextEditorMenuKind.AutoCompleteMenu, true).ConfigureAwait(false);
            }

            if (seenIsSyntaxHighlightingInvoker)
            {
                _throttleApplySyntaxHighlighting.FireAndForget(async _ =>
                {
                    // The TextEditorModel may have been changed by the time this logic is ran and
                    // thus the local variables must be updated accordingly.
                    var model = GetModel();
                    var viewModel = GetViewModel();

                    if (model is not null)
                    {
                        await modelModifier.ApplySyntaxHighlightingAsync().ConfigureAwait(false);

                        if (viewModel is not null && model.CompilerService is not null)
                            model.CompilerService.ResourceWasModified(model.ResourceUri, ImmutableArray<TextEditorTextSpan>.Empty);
                    }
                });
            }
        };
    }

    private async Task HandleMouseStoppedMovingEventAsync(MouseEventArgs mouseEventArgs)
    {
        var model = GetModel();
        var viewModel = GetViewModel();

        if (model is null || viewModel is null)
            return;

        // Lazily calculate row and column index a second time. Otherwise one has to calculate it every mouse moved event.
        var rowAndColumnIndex = await CalculateRowAndColumnIndex(mouseEventArgs).ConfigureAwait(false);

        // TODO: (2023-05-28) This shouldn't be re-calcuated in the best case scenario. That is to say, the previous line invokes 'CalculateRowAndColumnIndex(...)' which also invokes this logic
        var relativeCoordinatesOnClick = await JsRuntime.InvokeAsync<RelativeCoordinates>(
                "luthetusTextEditor.getRelativePosition",
                viewModel.BodyElementId,
                mouseEventArgs.ClientX,
                mouseEventArgs.ClientY)
            .ConfigureAwait(false);

        var cursorPositionIndex = model.GetPositionIndex(new TextEditorCursor(
            rowAndColumnIndex.rowIndex,
            rowAndColumnIndex.columnIndex,
            true));

        var foundMatch = false;

        var symbols = model.CompilerService.GetSymbolsFor(model.ResourceUri);
        var diagnostics = model.CompilerService.GetDiagnosticsFor(model.ResourceUri);

        if (diagnostics.Length != 0)
        {
            foreach (var diagnostic in diagnostics)
            {
                if (cursorPositionIndex >= diagnostic.TextSpan.StartingIndexInclusive &&
                    cursorPositionIndex < diagnostic.TextSpan.EndingIndexExclusive)
                {
                    // Prefer showing a diagnostic over a symbol when both exist at the mouse location.
                    foundMatch = true;

                    var parameterMap = new Dictionary<string, object?>
                    {
                        {
                            nameof(ITextEditorDiagnosticRenderer.Diagnostic),
                            diagnostic
                        }
                    };

                    _tooltipViewModel = new(
                        LuthetusTextEditorComponentRenderers.DiagnosticRendererType,
                        parameterMap,
                        relativeCoordinatesOnClick,
                        null,
                        HandleOnTooltipMouseOverAsync);
                }
            }
        }

        if (!foundMatch && symbols.Length != 0)
        {
            foreach (var symbol in symbols)
            {
                if (cursorPositionIndex >= symbol.TextSpan.StartingIndexInclusive &&
                    cursorPositionIndex < symbol.TextSpan.EndingIndexExclusive)
                {
                    foundMatch = true;

                    var parameters = new Dictionary<string, object?>
                    {
                        {
                            nameof(ITextEditorSymbolRenderer.Symbol),
                            symbol
                        }
                    };

                    _tooltipViewModel = new(
                        LuthetusTextEditorComponentRenderers.SymbolRendererType,
                        parameters,
                        relativeCoordinatesOnClick,
                        null,
                        HandleOnTooltipMouseOverAsync);
                }
            }
        }

        if (!foundMatch)
        {
            if (_tooltipViewModel is null)
                return; // Avoid the re-render if nothing changed

            _tooltipViewModel = null;
        }

        // TODO: Measure the tooltip, and reposition if it would go offscreen.

        await InvokeAsync(StateHasChanged).ConfigureAwait(false);
    }

    private bool IsSyntaxHighlightingInvoker(KeyboardEventArgs keyboardEventArgs)
    {
        return keyboardEventArgs.Key == ";" ||
               KeyboardKeyFacts.IsWhitespaceCode(keyboardEventArgs.Code) ||
               keyboardEventArgs.CtrlKey && keyboardEventArgs.Key == "s" ||
               keyboardEventArgs.CtrlKey && keyboardEventArgs.Key == "v" ||
               keyboardEventArgs.CtrlKey && keyboardEventArgs.Key == "z" ||
               keyboardEventArgs.CtrlKey && keyboardEventArgs.Key == "y";
    }

    private bool IsAutocompleteMenuInvoker(KeyboardEventArgs keyboardEventArgs)
    {
        // Is {Ctrl + Space} or LetterOrDigit was hit without Ctrl being held
        return keyboardEventArgs.CtrlKey && keyboardEventArgs.Code == KeyboardKeyFacts.WhitespaceCodes.SPACE_CODE ||
               !keyboardEventArgs.CtrlKey &&
                !KeyboardKeyFacts.IsWhitespaceCode(keyboardEventArgs.Code) &&
                !KeyboardKeyFacts.IsMetaKey(keyboardEventArgs);
    }

    /// <summary>
    /// All keyboardEventArgs that return true from "IsAutocompleteIndexerInvoker"
    /// are to be 1 character long, as well either whitespace or punctuation.
    /// Therefore 1 character behind might be a word that can be indexed.
    /// </summary>
    private bool IsAutocompleteIndexerInvoker(KeyboardEventArgs keyboardEventArgs)
    {
        return (KeyboardKeyFacts.IsWhitespaceCode(keyboardEventArgs.Code) ||
                KeyboardKeyFacts.IsPunctuationCharacter(keyboardEventArgs.Key.First())) &&
               !keyboardEventArgs.CtrlKey;
    }

    private async Task HandleOnWheelAsync(WheelEventArgs wheelEventArgs)
    {
        var textEditorViewModel = GetViewModel();

        if (textEditorViewModel is null)
            return;

        if (wheelEventArgs.ShiftKey)
            textEditorViewModel.MutateScrollHorizontalPositionByPixels(wheelEventArgs.DeltaY);
        else
            textEditorViewModel.MutateScrollVerticalPositionByPixels(wheelEventArgs.DeltaY);
    }

    private Task HandleOnTooltipMouseOverAsync()
    {
        _onMouseOutTooltipCancellationTokenSource.Cancel();
        _onMouseOutTooltipCancellationTokenSource = new();

        return Task.CompletedTask;
    }

    private string GetGlobalHeightInPixelsStyling()
    {
        var heightInPixels = TextEditorService.OptionsStateWrap.Value.Options.TextEditorHeightInPixels;

        if (heightInPixels is null)
            return string.Empty;

        var heightInPixelsInvariantCulture = heightInPixels.Value.ToCssValue();

        return $"height: {heightInPixelsInvariantCulture}px;";
    }

    private Task HandleOnTouchStartAsync(TouchEventArgs touchEventArgs)
    {
        _touchStartDateTime = DateTime.UtcNow;

        _previousTouchEventArgs = touchEventArgs;
        _thinksTouchIsOccurring = true;

        return Task.CompletedTask;
    }

    private async Task HandleOnTouchMoveAsync(TouchEventArgs touchEventArgs)
    {
        var localThinksTouchIsOccurring = _thinksTouchIsOccurring;

        if (!_thinksTouchIsOccurring)
            return;

        var previousTouchPoint = _previousTouchEventArgs?.ChangedTouches.FirstOrDefault(x => x.Identifier == 0);
        var currentTouchPoint = touchEventArgs.ChangedTouches.FirstOrDefault(x => x.Identifier == 0);

        if (previousTouchPoint is null || currentTouchPoint is null)
            return;

        var viewModel = GetViewModel();

        if (viewModel is null)
            return;

        // Natural scrolling for touch devices
        var diffX = previousTouchPoint.ClientX - currentTouchPoint.ClientX;
        var diffY = previousTouchPoint.ClientY - currentTouchPoint.ClientY;

        viewModel.MutateScrollHorizontalPositionByPixels(diffX);
        viewModel.MutateScrollVerticalPositionByPixels(diffY);

        _previousTouchEventArgs = touchEventArgs;
    }

    private void ClearTouch(TouchEventArgs touchEventArgs)
    {
        var rememberStartTouchEventArgs = _previousTouchEventArgs;

        _thinksTouchIsOccurring = false;
        _previousTouchEventArgs = null;

        var clearTouchDateTime = DateTime.UtcNow;
        var touchTimespan = clearTouchDateTime - _touchStartDateTime;

        if (touchTimespan is null)
            return;

        if (touchTimespan.Value.TotalMilliseconds < 200)
        {
            var startTouchPoint = rememberStartTouchEventArgs?.ChangedTouches.FirstOrDefault(x => x.Identifier == 0);

            if (startTouchPoint is null)
                return;

            HandleContentOnMouseDown(new MouseEventArgs
            {
                Buttons = 1,
                ClientX = startTouchPoint.ClientX,
                ClientY = startTouchPoint.ClientY,
            });
        }
    }

    private void QueueRemeasureBackgroundTask(
        TextEditorRenderBatch localRefCurrentRenderBatch,
        string localMeasureCharacterWidthAndRowHeightElementId,
        int countOfTestCharacters,
        CancellationToken cancellationToken)
    {
        var modelResourceUri = GetModel()?.ResourceUri;
        var viewModelKey = GetViewModel()?.ViewModelKey;

        if (modelResourceUri is null || viewModelKey is null)
            return;

        TextEditorService.Post(
            nameof(QueueRemeasureBackgroundTask),
            TextEditorService.ViewModelApi.RemeasureFactory(
                modelResourceUri,
                viewModelKey.Value,
                localMeasureCharacterWidthAndRowHeightElementId,
                countOfTestCharacters,
                CancellationToken.None));
    }

    private void QueueCalculateVirtualizationResultBackgroundTask(
        TextEditorRenderBatch localCurrentRenderBatch)
    {
        var modelResourceUri = GetModel()?.ResourceUri;
        var viewModelKey = GetViewModel()?.ViewModelKey;

        if (modelResourceUri is null || viewModelKey is null)
            return;

        TextEditorService.Post(
            nameof(QueueCalculateVirtualizationResultBackgroundTask),
            TextEditorService.ViewModelApi.CalculateVirtualizationResultFactory(
                modelResourceUri,
                viewModelKey.Value,
                CancellationToken.None));
    }

    public void Dispose()
    {
        TextEditorViewModelsStateWrap.StateChanged -= GeneralOnStateChangedEventHandler;
        TextEditorOptionsStateWrap.StateChanged -= GeneralOnStateChangedEventHandler;

        lock (_linkedViewModelLock)
        {
            if (_linkedViewModel is not null)
            {
                _linkedViewModel.DisplayTracker.DecrementLinks(TextEditorModelStateWrap);
                _linkedViewModel = null;
            }
        }

        _mouseStoppedMovingCancellationTokenSource.Cancel();
    }
}