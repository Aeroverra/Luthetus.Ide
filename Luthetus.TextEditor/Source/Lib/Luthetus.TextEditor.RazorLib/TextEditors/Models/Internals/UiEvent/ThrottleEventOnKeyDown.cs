﻿using Luthetus.Common.RazorLib.Commands.Models;
using Luthetus.Common.RazorLib.Keyboards.Models;
using Luthetus.Common.RazorLib.Keys.Models;
using Luthetus.Common.RazorLib.Reactives.Models;
using Luthetus.TextEditor.RazorLib.Commands.Models;
using Luthetus.TextEditor.RazorLib.Cursors.Models;
using Luthetus.TextEditor.RazorLib.Lexes.Models;
using Luthetus.TextEditor.RazorLib.TextEditors.Displays;
using Microsoft.AspNetCore.Components.Web;
using static Luthetus.TextEditor.RazorLib.TextEditors.Displays.TextEditorViewModelDisplay;

namespace Luthetus.TextEditor.RazorLib.TextEditors.Models.Internals.UiEvent;

public class ThrottleEventOnKeyDown : IThrottleEvent
{
    private readonly TextEditorEvents _events;

    public ThrottleEventOnKeyDown(
        TextEditorEvents events,
        KeyboardEventArgs keyboardEventArgs,
        ResourceUri resourceUri,
        Key<TextEditorViewModel> viewModelKey)
    {
        _events = events;

        KeyboardEventArgs = keyboardEventArgs;
        ResourceUri = resourceUri;
        ViewModelKey = viewModelKey;

        var badViewModel = _events.TextEditorService.ViewModelApi.GetOrDefault(ViewModelKey);

        if (badViewModel is not null)
        {
            TentativeHasSelection = TextEditorSelectionHelper.HasSelectedText(badViewModel.PrimaryCursor.Selection);

            TentativeKeyboardEventArgsKind = TextEditorEventsUtils.GetKeyboardEventArgsKind(
                KeyboardEventArgs,
                TentativeHasSelection,
                _events.TextEditorService,
                out var localCommand);

            Command = localCommand;
        }
    }

    public TimeSpan ThrottleTimeSpan => TimeSpan.Zero;
    public KeyboardEventArgs KeyboardEventArgs { get; }
    public CommandNoType? Command { get; }

    /// <summary>
    /// The initial enqueueing of this throttle event might result in an incorrect <see cref="TentativeKeyboardEventArgsKind"/>
    /// This is due to the selection being checked, prior to the previous UI events that came before this have been handled.<br/><br/>
    ///
    /// A 'tab' key for example, without a selection: it inserts a tab character, but with a selection: it indents the lines
    /// that are selected. So, from not having a selection, to having one, a 'tab' key could be text or a command.<br/><br/>
    /// </summary>
    public KeyboardEventArgsKind TentativeKeyboardEventArgsKind { get; }

    /// <summary>
    /// The initial enqueueing of this throttle event might result in an incorrect <see cref="TentativeHasSelection"/>
    /// This is due to the selection being checked, prior to the previous UI events that came before this have been handled.<br/><br/>
    ///
    /// It is possible a user might at the moment of pressing a key, not have a selection.
    /// But have a previous event pending, that will result in a future selection when this event is handled.<br/><br/>
    /// </summary>
    public bool TentativeHasSelection { get; }

    public ResourceUri ResourceUri { get; }
    public Key<TextEditorViewModel> ViewModelKey { get; }

    public IThrottleEvent? BatchOrDefault(IThrottleEvent moreRecentEvent)
    {
        if (moreRecentEvent is ThrottleEventOnKeyDown moreRecentEventOnKeyDown)
        {
            switch (TentativeKeyboardEventArgsKind)
            {
                case KeyboardEventArgsKind.Text:
                    if (moreRecentEventOnKeyDown.TentativeKeyboardEventArgsKind == TentativeKeyboardEventArgsKind)
                    {
                        return new ThrottleEventOnKeyDownBatch(
                            _events,
                            new List<ThrottleEventOnKeyDown>()
                            {
                                moreRecentEventOnKeyDown,
                                this
                            },
                            TentativeKeyboardEventArgsKind,
                            ResourceUri,
                            ViewModelKey);
                    }
                    break;
                case KeyboardEventArgsKind.Movement:
                    if (moreRecentEventOnKeyDown.TentativeKeyboardEventArgsKind == TentativeKeyboardEventArgsKind &&
                        KeyAndModifiersAreEqual(KeyboardEventArgs, moreRecentEventOnKeyDown.KeyboardEventArgs))
                    {
                        return new ThrottleEventOnKeyDownBatch(
                            _events,
                            new List<ThrottleEventOnKeyDown>()
                            {
                                moreRecentEventOnKeyDown,
                                this
                            },
                            TentativeKeyboardEventArgsKind,
                            ResourceUri,
                            ViewModelKey);
                    }
                    break;
                case KeyboardEventArgsKind.Other:
                    if (KeyboardKeyFacts.IsMetaKey(KeyboardEventArgs))
                    {
                        if (KeyboardKeyFacts.MetaKeys.BACKSPACE == KeyboardEventArgs.Key ||
                            KeyboardKeyFacts.MetaKeys.DELETE == KeyboardEventArgs.Key)
                        {
                            if (TentativeKeyboardEventArgsKind == moreRecentEventOnKeyDown.TentativeKeyboardEventArgsKind &&
                                KeyAndModifiersAreEqual(KeyboardEventArgs, moreRecentEventOnKeyDown.KeyboardEventArgs))
                            {
                                return new ThrottleEventOnKeyDownBatch(
                                    _events,
                                    new List<ThrottleEventOnKeyDown>()
                                    {
                                        moreRecentEventOnKeyDown,
                                        this,
                                    },
                                    TentativeKeyboardEventArgsKind,
                                    ResourceUri,
                                    ViewModelKey);
                            }
                        }
                    }
                    break;
                case KeyboardEventArgsKind.ContextMenu:
                    break;
                case KeyboardEventArgsKind.Command:
                    if (TentativeKeyboardEventArgsKind == moreRecentEventOnKeyDown.TentativeKeyboardEventArgsKind &&
                        KeyAndModifiersAreEqual(KeyboardEventArgs, moreRecentEventOnKeyDown.KeyboardEventArgs) &&
                        Command is not null &&
                        moreRecentEventOnKeyDown.Command is not null &&
                        Command.InternalIdentifier == moreRecentEventOnKeyDown.Command.InternalIdentifier)
                    {
                        return new ThrottleEventOnKeyDownBatch(
                            _events,
                            new List<ThrottleEventOnKeyDown>()
                            {
                                moreRecentEventOnKeyDown,
                                this
                            },
                            TentativeKeyboardEventArgsKind,
                            ResourceUri,
                            ViewModelKey);
                    }
                    break;
            }
        }
        
        if (moreRecentEvent is ThrottleEventOnKeyDownBatch moreRecentEventOnKeyDownBatch)
        {
            var inspectThrottleEventOnKeyDown = moreRecentEventOnKeyDownBatch.ThrottleEventOnKeyDownList.First();
            switch (TentativeKeyboardEventArgsKind)
            {
                case KeyboardEventArgsKind.Text:
                    if (TentativeKeyboardEventArgsKind == moreRecentEventOnKeyDownBatch.KeyboardEventArgsKind)
                    {
                        moreRecentEventOnKeyDownBatch.ThrottleEventOnKeyDownList.Add(this);
                        return moreRecentEventOnKeyDownBatch;
                    }
                    break;
                case KeyboardEventArgsKind.Movement:
                    if (TentativeKeyboardEventArgsKind == moreRecentEventOnKeyDownBatch.KeyboardEventArgsKind &&
                        KeyAndModifiersAreEqual(KeyboardEventArgs, inspectThrottleEventOnKeyDown.KeyboardEventArgs))
                    {
                        moreRecentEventOnKeyDownBatch.ThrottleEventOnKeyDownList.Add(this);
                        return moreRecentEventOnKeyDownBatch;
                    }
                    break;
                case KeyboardEventArgsKind.Other:
                    if (KeyboardKeyFacts.IsMetaKey(KeyboardEventArgs))
                    {
                        if (KeyboardEventArgs.Key == KeyboardKeyFacts.MetaKeys.BACKSPACE ||
                            KeyboardEventArgs.Key == KeyboardKeyFacts.MetaKeys.DELETE)
                        {
                            if (TentativeKeyboardEventArgsKind == moreRecentEventOnKeyDownBatch.KeyboardEventArgsKind &&
                                KeyAndModifiersAreEqual(KeyboardEventArgs, inspectThrottleEventOnKeyDown.KeyboardEventArgs))
                            {
                                moreRecentEventOnKeyDownBatch.ThrottleEventOnKeyDownList.Add(this);
                                return moreRecentEventOnKeyDownBatch;
                            }
                        }
                    }
                    break;
                case KeyboardEventArgsKind.ContextMenu:
                    break;
                case KeyboardEventArgsKind.Command:
                    if (TentativeKeyboardEventArgsKind == moreRecentEventOnKeyDownBatch.KeyboardEventArgsKind &&
                        KeyAndModifiersAreEqual(KeyboardEventArgs, inspectThrottleEventOnKeyDown.KeyboardEventArgs) &&
                        Command is not null &&
                        inspectThrottleEventOnKeyDown.Command is not null &&
                        Command.InternalIdentifier == inspectThrottleEventOnKeyDown.Command.InternalIdentifier)
                    {
                        moreRecentEventOnKeyDownBatch.ThrottleEventOnKeyDownList.Add(this);
                        return moreRecentEventOnKeyDownBatch;
                    }
                    break;
            }
        }

        return null;
    }

    public Task HandleEvent(CancellationToken cancellationToken)
    {
        var eventName = string.Empty;
        {
            if (TentativeKeyboardEventArgsKind == KeyboardEventArgsKind.Command &&
                Command is not null)
            {
                eventName = Command.InternalIdentifier;
            }
            else
            {
                eventName = KeyboardEventArgs.Key;
            }
        }

        _events.TextEditorService.Post(
            $"okd_{eventName}",
            async editContext =>
            {
                var modelModifier = editContext.GetModelModifier(ResourceUri);
                var viewModelModifier = editContext.GetViewModelModifier(ViewModelKey);
                var cursorModifierBag = editContext.GetCursorModifierBag(viewModelModifier?.ViewModel);
                var primaryCursorModifier = editContext.GetPrimaryCursorModifier(cursorModifierBag);

                if (modelModifier is null || viewModelModifier is null || cursorModifierBag is null || primaryCursorModifier is null)
                    return;

                // The initial enqueueing of this throttle event might result in an incorrect KeyboardEventArgsKind
                // This is due to the selection being checked, prior to the previous UI events that came before this have been handled.
                //
                // It is possible a user might at the moment of pressing a key, not have a selection.
                // But have a previous event pending, that will result in a future selection when this event is handled.
                var definiteHasSelection = TextEditorSelectionHelper.HasSelectedText(primaryCursorModifier);

                var definiteKeyboardEventArgsKind = TextEditorEventsUtils.GetKeyboardEventArgsKind(
                    KeyboardEventArgs, definiteHasSelection, _events.TextEditorService, out var command);

                var shouldInvokeAfterOnKeyDownAsync = false;

                switch (definiteKeyboardEventArgsKind)
                {
                    case KeyboardEventArgsKind.Command:
                        shouldInvokeAfterOnKeyDownAsync = true;

                        await command.CommandFunc.Invoke(new TextEditorCommandArgs(
                                modelModifier.ResourceUri,
                                viewModelModifier.ViewModel.ViewModelKey,
                                definiteHasSelection,
                                _events.ClipboardService,
                                _events.TextEditorService,
                                _events.HandleMouseStoppedMovingEventAsync,
                                _events.JsRuntime,
                                _events.Dispatcher,
                                _events.ServiceProvider,
                                _events.TextEditorConfig))
                            .ConfigureAwait(false);
                        break;
                    case KeyboardEventArgsKind.Movement:
                        if ((KeyboardKeyFacts.MovementKeys.ARROW_DOWN == KeyboardEventArgs.Key || KeyboardKeyFacts.MovementKeys.ARROW_UP == KeyboardEventArgs.Key) &&
                             _events.CursorDisplay is not null && _events.CursorDisplay.MenuKind == TextEditorMenuKind.AutoCompleteMenu)
                        {
                            await _events.CursorDisplay.SetFocusToActiveMenuAsync().ConfigureAwait(false);
                        }
                        else
                        {
                            await _events.TextEditorService.ViewModelApi.MoveCursorFactory(
                                    KeyboardEventArgs,
                                    modelModifier.ResourceUri,
                                    viewModelModifier.ViewModel.ViewModelKey)
                                .Invoke(editContext)
                                .ConfigureAwait(false);

                            await (_events.CursorDisplay?.SetShouldDisplayMenuAsync(TextEditorMenuKind.None) ?? Task.CompletedTask)
                                .ConfigureAwait(false);
                        }
                        break;
                    case KeyboardEventArgsKind.ContextMenu:
                        await (_events.CursorDisplay?.SetShouldDisplayMenuAsync(TextEditorMenuKind.ContextMenu) ?? Task.CompletedTask)
                            .ConfigureAwait(false);
                        break;
                    case KeyboardEventArgsKind.Text:
                    case KeyboardEventArgsKind.Other:
                        shouldInvokeAfterOnKeyDownAsync = true;

                        if (!_events.IsAutocompleteMenuInvoker(KeyboardEventArgs))
                        {
                            if (KeyboardKeyFacts.MetaKeys.ESCAPE == KeyboardEventArgs.Key ||
                                KeyboardKeyFacts.MetaKeys.BACKSPACE == KeyboardEventArgs.Key ||
                                KeyboardKeyFacts.MetaKeys.DELETE == KeyboardEventArgs.Key ||
                                !KeyboardKeyFacts.IsMetaKey(KeyboardEventArgs))
                            {
                                await (_events.CursorDisplay?.SetShouldDisplayMenuAsync(TextEditorMenuKind.None) ?? Task.CompletedTask)
                                    .ConfigureAwait(false);
                            }
                        }

                        _events.TooltipViewModel = null;

                        await _events.TextEditorService.ModelApi.HandleKeyboardEventFactory(
                                ResourceUri,
                                ViewModelKey,
                                KeyboardEventArgs,
                                CancellationToken.None)
                            .Invoke(editContext)
                            .ConfigureAwait(false);
                        break;
                }

                if (shouldInvokeAfterOnKeyDownAsync)
                {
                    if (command is null ||
                        command is TextEditorCommand commandTextEditor && commandTextEditor.ShouldScrollCursorIntoView)
                    {
                        primaryCursorModifier.ShouldRevealCursor = true;
                    }

                    var cursorDisplay = _events.CursorDisplay;

                    if (cursorDisplay is not null)
                    {
                        await _events.HandleAfterOnKeyDownAsyncFactory(
                                modelModifier.ResourceUri,
                                viewModelModifier.ViewModel.ViewModelKey,
                                KeyboardEventArgs,
                                cursorDisplay.SetShouldDisplayMenuAsync)
                            .Invoke(editContext)
                            .ConfigureAwait(false);
                    }
                }
            });

        return Task.CompletedTask;
    }

    private bool KeyAndModifiersAreEqual(KeyboardEventArgs x, KeyboardEventArgs y)
    {
        return
            x.Key == y.Key &&
            x.ShiftKey == y.ShiftKey &&
            x.CtrlKey == y.CtrlKey &&
            x.AltKey == y.AltKey;
    }
}
