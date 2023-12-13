﻿using Luthetus.Common.RazorLib.Keyboards.Models;
using Luthetus.Common.RazorLib.Keymaps.Models;
using Luthetus.Common.RazorLib.Keys.Models;
using Luthetus.Common.RazorLib.RenderStates.Models;
using Luthetus.TextEditor.RazorLib.Characters.Models;
using Luthetus.TextEditor.RazorLib.Commands.Models;
using Luthetus.TextEditor.RazorLib.CompilerServices;
using Luthetus.TextEditor.RazorLib.Cursors.Models;
using Luthetus.TextEditor.RazorLib.Decorations.Models;
using Luthetus.TextEditor.RazorLib.Edits.Models;
using Luthetus.TextEditor.RazorLib.Lexes.Models;
using Luthetus.TextEditor.RazorLib.Options.Models;
using Luthetus.TextEditor.RazorLib.Rows.Models;
using Microsoft.AspNetCore.Components.Web;
using System.Collections.Immutable;
using static Luthetus.TextEditor.RazorLib.TextEditors.States.TextEditorModelState;

namespace Luthetus.TextEditor.RazorLib.TextEditors.Models.TextEditorModels;

/// <summary>
/// Any modified state needs to be 'null coallesce assigned' to the existing TextEditorModel's value
///
/// When reading state, if the state had been 'null coallesce assigned' then the field will
/// be read. Otherwise, the existing TextEditorModel's value will be read.
/// <br/>
/// A large amount of this file is going to be deleted. I copied over everything from
/// <see cref="TextEditorModel"/>, and I need to think about how I want to simplify things.
/// </summary>
public partial class TextEditorModelModifier
{
    private readonly TextEditorModel _textEditorModel;

    public TextEditorModelModifier(TextEditorModel textEditorModel)
    {
        _textEditorModel = textEditorModel;
    }

    private List<RichCharacter>? _contentBag;
    private List<EditBlock>? _editBlocksBag;
    private List<(int positionIndex, RowEndingKind rowEndingKind)>? _rowEndingPositionsBag;
    private List<(RowEndingKind rowEndingKind, int count)>? _rowEndingKindCountsBag;
    private List<TextEditorPresentationModel>? _presentationModelsBag;
    private List<int>? _tabKeyPositionsBag;

    private RowEndingKind? _onlyRowEndingKind;
    /// <summary>
    /// Awkward special case here: <see cref="_onlyRowEndingKind"/> is allowed to be null.
    /// So, the design of this class where null means unmodified, doesn't work well here.
    /// </summary>
    private bool _onlyRowEndingKindWasModified;

    private RowEndingKind? _usingRowEndingKind;
    private ResourceUri? _resourceUri;
    private DateTime? _resourceLastWriteTime;
    private string? _fileExtension;
    private IDecorationMapper? _decorationMapper;
    private ICompilerService? _compilerService;
    private TextEditorSaveFileHelper? _textEditorSaveFileHelper;
    private int? _editBlockIndex;
    private (int rowIndex, int rowLength)? _mostCharactersOnASingleRowTuple;
    private Key<RenderState>? _renderStateKey = Key<RenderState>.NewKey();
    private Keymap? _textEditorKeymap;
    private TextEditorOptions? _textEditorOptions;

    public TextEditorModel ToTextEditorModel()
    {
        return new TextEditorModel(
            _contentBag is null ? _textEditorModel.ContentBag : _contentBag.ToImmutableList(),
            _editBlocksBag is null ? _textEditorModel.EditBlocksBag : _editBlocksBag.ToImmutableList(),
            _rowEndingPositionsBag is null ? _textEditorModel.RowEndingPositionsBag : _rowEndingPositionsBag.ToImmutableList(),
            _rowEndingKindCountsBag is null ? _textEditorModel.RowEndingKindCountsBag : _rowEndingKindCountsBag.ToImmutableList(),
            _presentationModelsBag is null ? _textEditorModel.PresentationModelsBag : _presentationModelsBag.ToImmutableList(),
            _tabKeyPositionsBag is null ? _textEditorModel.TabKeyPositionsBag : _tabKeyPositionsBag.ToImmutableList(),
            _onlyRowEndingKindWasModified ? _onlyRowEndingKind : _textEditorModel.OnlyRowEndingKind,
            _usingRowEndingKind ?? _textEditorModel.UsingRowEndingKind,
            _resourceUri ?? _textEditorModel.ResourceUri,
            _resourceLastWriteTime ?? _textEditorModel.ResourceLastWriteTime,
            _fileExtension ?? _textEditorModel.FileExtension,
            _decorationMapper ?? _textEditorModel.DecorationMapper,
            _compilerService ?? _textEditorModel.CompilerService,
            _textEditorSaveFileHelper ?? _textEditorModel.TextEditorSaveFileHelper,
            _editBlockIndex ?? _textEditorModel.EditBlockIndex,
            _mostCharactersOnASingleRowTuple ?? _textEditorModel.MostCharactersOnASingleRowTuple,
            _renderStateKey ?? _textEditorModel.RenderStateKey,
            _textEditorKeymap ?? _textEditorModel.TextEditorKeymap,
            _textEditorOptions ?? _textEditorModel.TextEditorOptions);
    }

    public void ModifyContentBag()
    {
        throw new NotImplementedException();
    }

    public void ClearContentBag()
    {
        _contentBag = new();
    }

    public void ModifyEditBlocksBag()
    {
        throw new NotImplementedException();
    }

    public void ModifyRowEndingPositionsBag()
    {
        throw new NotImplementedException();
    }

    public void ClearRowEndingPositionsBag()
    {
        _rowEndingPositionsBag = new();
    }

    public void ModifyRowEndingKindCountsBag()
    {
        throw new NotImplementedException();
    }

    public void ClearRowEndingKindCountsBag()
    {
        _rowEndingKindCountsBag = new();
    }

    public void ModifyPresentationModelsBag()
    {
        throw new NotImplementedException();
    }

    public void ModifyTabKeyPositionsBag()
    {
        throw new NotImplementedException();
    }

    public void ClearTabKeyPositionsBag()
    {
        _tabKeyPositionsBag = new();
    }

    public void ModifyOnlyRowEndingKind()
    {
        _onlyRowEndingKind = null;
        _onlyRowEndingKindWasModified = true;
    }

    public void ModifyUsingRowEndingKind(RowEndingKind rowEndingKind)
    {
        _usingRowEndingKind = rowEndingKind;
    }

    public void ModifyResourceUri()
    {
        throw new NotImplementedException();
    }

    public void ModifyResourceLastWriteTime()
    {
        throw new NotImplementedException();
    }

    public void ModifyResourceData(ResourceUri resourceUri, DateTime resourceLastWriteTime)
    {
        _resourceUri = resourceUri;
        _resourceLastWriteTime = resourceLastWriteTime;
    }

    public void ModifyFileExtension()
    {
        throw new NotImplementedException();
    }

    public void ModifyDecorationMapper(IDecorationMapper decorationMapper)
    {
        _decorationMapper = decorationMapper;
    }

    public void ModifyCompilerService(ICompilerService compilerService)
    {
        _compilerService = compilerService;
    }

    public void ModifyTextEditorSaveFileHelper(TextEditorSaveFileHelper textEditorSaveFileHelper)
    {
        _textEditorSaveFileHelper = textEditorSaveFileHelper;
    }

    public void ModifyEditBlockIndex()
    {
        throw new NotImplementedException();
    }

    public void ModifyMostCharactersOnASingleRowTuple()
    {
        throw new NotImplementedException();
    }

    public void ModifyRenderStateKey()
    {
        throw new NotImplementedException();
    }

    public void ModifyTextEditorKeymap()
    {
        throw new NotImplementedException();
    }

    public void ModifyTextEditorOptions()
    {
        throw new NotImplementedException();
    }

    //////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////

    private void EnsureUndoPoint(TextEditKind textEditKind, string? otherTextEditKindIdentifier = null)
    {
        // Any modified state needs to be 'null coallesce assigned' to the existing TextEditorModel's value
        //
        // When reading state, if the state had been 'null coallesce assigned' then the field will
        // be read. Otherwise, the existing TextEditorModel's value will be read.
        {
            _editBlocksBag ??= _textEditorModel.EditBlocksBag.ToList();
            _editBlockIndex ??= _textEditorModel.EditBlockIndex;
        }

        if (textEditKind == TextEditKind.Other && otherTextEditKindIdentifier is null)
            TextEditorCommand.ThrowOtherTextEditKindIdentifierWasExpectedException(textEditKind);

        var mostRecentEditBlock = EditBlocksBag.LastOrDefault();

        if (mostRecentEditBlock is null || mostRecentEditBlock.TextEditKind != textEditKind)
        {
            var newEditBlockIndex = EditBlockIndex;

            EditBlocksBag.Insert(newEditBlockIndex, new EditBlock(
                textEditKind,
                textEditKind.ToString(),
                this.GetAllText(),
                otherTextEditKindIdentifier));

            var removeBlocksStartingAt = newEditBlockIndex + 1;

            _editBlocksBag.RemoveRange(removeBlocksStartingAt, EditBlocksBag.Count - removeBlocksStartingAt);

            _editBlockIndex++;
        }

        while (EditBlocksBag.Count > TextEditorModel.MAXIMUM_EDIT_BLOCKS && EditBlocksBag.Count != 0)
        {
            _editBlockIndex--;
            EditBlocksBag.RemoveAt(0);
        }
    }

    private void PerformInsertions(KeyboardEventAction keyboardEventAction)
    {
        // Any modified state needs to be 'null coallesce assigned' to the existing TextEditorModel's value
        //
        // When reading state, if the state had been 'null coallesce assigned' then the field will
        // be read. Otherwise, the existing TextEditorModel's value will be read.
        {
            _contentBag ??= _textEditorModel.ContentBag.ToList();
            _rowEndingPositionsBag ??= _textEditorModel.RowEndingPositionsBag.ToList();
            _tabKeyPositionsBag ??= _textEditorModel.TabKeyPositionsBag.ToList();
            _mostCharactersOnASingleRowTuple ??= _textEditorModel.MostCharactersOnASingleRowTuple;
        }

        EnsureUndoPoint(TextEditKind.Insertion);

        foreach (var cursor in keyboardEventAction.CursorBag)
        {
            if (TextEditorSelectionHelper.HasSelectedText(cursor.Selection))
            {
                PerformDeletions(keyboardEventAction);

                var selectionBounds = TextEditorSelectionHelper.GetSelectionBounds(cursor.Selection);

                var lowerRowData = this.FindRowInformation(selectionBounds.lowerPositionIndexInclusive);

                var lowerColumnIndex = selectionBounds.lowerPositionIndexInclusive - lowerRowData.rowStartPositionIndex;

                // Move cursor to lower bound of text selection
                cursor.RowIndex = lowerRowData.rowIndex;
                cursor.ColumnIndex = lowerColumnIndex;

                var nextEdit = keyboardEventAction with
                {
                    CursorBag = new[]
                    {
                        cursor
                    }.ToImmutableArray()
                };

                // Because one cannot move reference of foreach variable,
                // one has to re-invoke the method with different paramters
                PerformInsertions(nextEdit);
                return;
            }

            var startOfRowPositionIndex = this.GetStartOfRowTuple(cursor.RowIndex).positionIndex;
            var cursorPositionIndex = startOfRowPositionIndex + cursor.ColumnIndex;

            // If cursor is out of bounds then continue
            if (cursorPositionIndex > ContentBag.Count)
                continue;

            var wasTabCode = false;
            var wasEnterCode = false;

            var characterValueToInsert = keyboardEventAction.KeyboardEventArgs.Key.First();

            if (KeyboardKeyFacts.IsWhitespaceCode(keyboardEventAction.KeyboardEventArgs.Code))
            {
                characterValueToInsert = KeyboardKeyFacts.ConvertWhitespaceCodeToCharacter(keyboardEventAction.KeyboardEventArgs.Code);

                wasTabCode = KeyboardKeyFacts.WhitespaceCodes.TAB_CODE == keyboardEventAction.KeyboardEventArgs.Code;
                wasEnterCode = KeyboardKeyFacts.WhitespaceCodes.ENTER_CODE == keyboardEventAction.KeyboardEventArgs.Code;
            }

            var characterCountInserted = 1;

            if (wasEnterCode)
            {
                var rowEndingKindToInsert = UsingRowEndingKind;

                var richCharacters = rowEndingKindToInsert.AsCharacters().Select(character => new RichCharacter
                {
                    Value = character,
                    DecorationByte = default,
                });

                characterCountInserted = rowEndingKindToInsert.AsCharacters().Length;

                _contentBag.InsertRange(cursorPositionIndex, richCharacters);

                RowEndingPositionsBag.Insert(cursor.RowIndex,
                    (cursorPositionIndex + characterCountInserted, rowEndingKindToInsert));

                MutateRowEndingKindCount(UsingRowEndingKind, 1);

                var rowIndex = cursor.RowIndex;
                var columnIndex = cursor.ColumnIndex;

                cursor.RowIndex = rowIndex + 1;
                cursor.ColumnIndex = 0;
                cursor.PreferredColumnIndex = cursor.ColumnIndex;
            }
            else
            {
                if (wasTabCode)
                {
                    var index = _tabKeyPositionsBag.FindIndex(x => x >= cursorPositionIndex);

                    if (index == -1)
                    {
                        TabKeyPositionsBag.Add(cursorPositionIndex);
                    }
                    else
                    {
                        for (var i = index; i < TabKeyPositionsBag.Count; i++)
                        {
                            _tabKeyPositionsBag[i]++;
                        }

                        TabKeyPositionsBag.Insert(index, cursorPositionIndex);
                    }
                }

                var richCharacterToInsert = new RichCharacter
                {
                    Value = characterValueToInsert,
                    DecorationByte = default,
                };

                ContentBag.Insert(cursorPositionIndex, richCharacterToInsert);

                var rowIndex = cursor.RowIndex;
                var columnIndex = cursor.ColumnIndex;

                cursor.RowIndex = rowIndex;
                cursor.ColumnIndex = columnIndex + 1;
                cursor.PreferredColumnIndex = cursor.ColumnIndex;
            }

            var firstRowIndexToModify = wasEnterCode
                ? cursor.RowIndex + 1
                : cursor.RowIndex;

            for (var i = firstRowIndexToModify; i < RowEndingPositionsBag.Count; i++)
            {
                var rowEndingTuple = RowEndingPositionsBag[i];
                RowEndingPositionsBag[i] = (rowEndingTuple.positionIndex + characterCountInserted, rowEndingTuple.rowEndingKind);
            }

            if (!wasTabCode)
            {
                var firstTabKeyPositionIndexToModify = _tabKeyPositionsBag.FindIndex(x => x >= cursorPositionIndex);

                if (firstTabKeyPositionIndexToModify != -1)
                {
                    for (var i = firstTabKeyPositionIndexToModify; i < TabKeyPositionsBag.Count; i++)
                    {
                        TabKeyPositionsBag[i] += characterCountInserted;
                    }
                }
            }

            // Reposition the Diagnostic Squigglies
            {
                var textSpanForInsertion = new TextEditorTextSpan(
                    cursorPositionIndex,
                    cursorPositionIndex + characterCountInserted,
                    0,
                    new(string.Empty),
                    string.Empty);

                var textModification = new TextEditorTextModification(true, textSpanForInsertion);

                foreach (var presentationModel in PresentationModelsBag)
                {
                    if (presentationModel.CompletedCalculation is not null)
                        presentationModel.CompletedCalculation.TextModificationsSinceRequestBag.Add(textModification);

                    if (presentationModel.PendingCalculation is not null)
                        presentationModel.PendingCalculation.TextModificationsSinceRequestBag.Add(textModification);
                }
            }
        }

        // TODO: Fix tracking the MostCharactersOnASingleRowTuple this way is possibly inefficient - should instead only check the rows that changed
        {
            (int rowIndex, int rowLength) localMostCharactersOnASingleRowTuple = (0, 0);

            for (var i = 0; i < RowEndingPositionsBag.Count; i++)
            {
                var lengthOfRow = this.GetLengthOfRow(i);

                if (lengthOfRow > localMostCharactersOnASingleRowTuple.rowLength)
                {
                    localMostCharactersOnASingleRowTuple = (i, lengthOfRow);
                }
            }

            localMostCharactersOnASingleRowTuple = (localMostCharactersOnASingleRowTuple.rowIndex,
                localMostCharactersOnASingleRowTuple.rowLength + TextEditorModel.MOST_CHARACTERS_ON_A_SINGLE_ROW_MARGIN);

            _mostCharactersOnASingleRowTuple = localMostCharactersOnASingleRowTuple;
        }
    }

    private void PerformDeletions(KeyboardEventAction keyboardEventAction)
    {
        // Any modified state needs to be 'null coallesce assigned' to the existing TextEditorModel's value
        //
        // When reading state, if the state had been 'null coallesce assigned' then the field will
        // be read. Otherwise, the existing TextEditorModel's value will be read.
        {
            _rowEndingPositionsBag ??= _textEditorModel.RowEndingPositionsBag.ToList();
            _tabKeyPositionsBag ??= _textEditorModel.TabKeyPositionsBag.ToList();
            _contentBag ??= _textEditorModel.ContentBag.ToList();
            _mostCharactersOnASingleRowTuple ??= _textEditorModel.MostCharactersOnASingleRowTuple;
        }

        EnsureUndoPoint(TextEditKind.Deletion);

        foreach (var cursor in keyboardEventAction.CursorBag)
        {
            var startOfRowPositionIndex = this.GetStartOfRowTuple(cursor.RowIndex).positionIndex;
            var cursorPositionIndex = startOfRowPositionIndex + cursor.ColumnIndex;

            // If cursor is out of bounds then continue
            if (cursorPositionIndex > ContentBag.Count)
                continue;

            int startingPositionIndexToRemoveInclusive;
            int countToRemove;
            bool moveBackwards;

            // Cannot calculate this after text was deleted - it would be wrong
            int? selectionUpperBoundRowIndex = null;
            // Needed for when text selection is deleted
            (int rowIndex, int columnIndex)? selectionLowerBoundIndexCoordinates = null;

            // TODO: The deletion logic should be the same whether it be 'Delete' 'Backspace' 'CtrlModified' or 'DeleteSelection'. What should change is one needs to calculate the starting and ending index appropriately foreach case.
            if (TextEditorSelectionHelper.HasSelectedText(cursor.Selection))
            {
                var lowerPositionIndexInclusiveBound = cursor.Selection.AnchorPositionIndex ?? 0;
                var upperPositionIndexExclusive = cursor.Selection.EndingPositionIndex;

                if (lowerPositionIndexInclusiveBound > upperPositionIndexExclusive)
                    (lowerPositionIndexInclusiveBound, upperPositionIndexExclusive) = (upperPositionIndexExclusive, lowerPositionIndexInclusiveBound);

                var lowerRowMetaData = this.FindRowInformation(lowerPositionIndexInclusiveBound);
                var upperRowMetaData = this.FindRowInformation(upperPositionIndexExclusive);

                // Value is needed when knowing what row ending positions to update after deletion is done
                selectionUpperBoundRowIndex = upperRowMetaData.rowIndex;

                // Value is needed when knowing where to position the cursor after deletion is done
                selectionLowerBoundIndexCoordinates = (lowerRowMetaData.rowIndex,
                    lowerPositionIndexInclusiveBound - lowerRowMetaData.rowStartPositionIndex);

                startingPositionIndexToRemoveInclusive = upperPositionIndexExclusive - 1;
                countToRemove = upperPositionIndexExclusive - lowerPositionIndexInclusiveBound;
                moveBackwards = true;

                cursor.Selection.AnchorPositionIndex = null;
            }
            else if (KeyboardKeyFacts.MetaKeys.BACKSPACE == keyboardEventAction.KeyboardEventArgs.Key)
            {
                moveBackwards = true;

                if (keyboardEventAction.KeyboardEventArgs.CtrlKey)
                {
                    var columnIndexOfCharacterWithDifferingKind = this.GetColumnIndexOfCharacterWithDifferingKind(
                        cursor.RowIndex,
                        cursor.ColumnIndex,
                        moveBackwards);

                    columnIndexOfCharacterWithDifferingKind = columnIndexOfCharacterWithDifferingKind == -1
                        ? 0
                        : columnIndexOfCharacterWithDifferingKind;

                    countToRemove = cursor.ColumnIndex -
                        columnIndexOfCharacterWithDifferingKind;

                    countToRemove = countToRemove == 0
                        ? 1
                        : countToRemove;
                }
                else
                {
                    countToRemove = 1;
                }

                startingPositionIndexToRemoveInclusive = cursorPositionIndex - 1;
            }
            else if (KeyboardKeyFacts.MetaKeys.DELETE == keyboardEventAction.KeyboardEventArgs.Key)
            {
                moveBackwards = false;

                if (keyboardEventAction.KeyboardEventArgs.CtrlKey)
                {
                    var columnIndexOfCharacterWithDifferingKind = this.GetColumnIndexOfCharacterWithDifferingKind(
                        cursor.RowIndex,
                        cursor.ColumnIndex,
                        moveBackwards);

                    columnIndexOfCharacterWithDifferingKind = columnIndexOfCharacterWithDifferingKind == -1
                        ? this.GetLengthOfRow(cursor.RowIndex)
                        : columnIndexOfCharacterWithDifferingKind;

                    countToRemove = columnIndexOfCharacterWithDifferingKind -
                        cursor.ColumnIndex;

                    countToRemove = countToRemove == 0
                        ? 1
                        : countToRemove;
                }
                else
                {
                    countToRemove = 1;
                }

                startingPositionIndexToRemoveInclusive = cursorPositionIndex;
            }
            else
            {
                throw new ApplicationException($"The keyboard key: {keyboardEventAction.KeyboardEventArgs.Key} was not recognized");
            }

            var charactersRemovedCount = 0;
            var rowsRemovedCount = 0;

            var indexToRemove = startingPositionIndexToRemoveInclusive;

            while (countToRemove-- > 0)
            {
                if (indexToRemove < 0 || indexToRemove > ContentBag.Count - 1)
                    break;

                var characterToDelete = ContentBag[indexToRemove];

                int startingIndexToRemoveRange;
                int countToRemoveRange;

                if (KeyboardKeyFacts.IsLineEndingCharacter(characterToDelete.Value))
                {
                    rowsRemovedCount++;

                    // rep.positionIndex == indexToRemove + 1
                    //     ^is for backspace
                    //
                    // rep.positionIndex == indexToRemove + 2
                    //     ^is for delete
                    var rowEndingTupleIndex = _rowEndingPositionsBag.FindIndex(rep =>
                        rep.positionIndex == indexToRemove + 1 ||
                        rep.positionIndex == indexToRemove + 2);

                    var rowEndingTuple = RowEndingPositionsBag[rowEndingTupleIndex];

                    RowEndingPositionsBag.RemoveAt(rowEndingTupleIndex);

                    var lengthOfRowEnding = rowEndingTuple.rowEndingKind.AsCharacters().Length;

                    if (moveBackwards)
                        startingIndexToRemoveRange = indexToRemove - (lengthOfRowEnding - 1);
                    else
                        startingIndexToRemoveRange = indexToRemove;

                    countToRemove -= lengthOfRowEnding - 1;
                    countToRemoveRange = lengthOfRowEnding;

                    MutateRowEndingKindCount(rowEndingTuple.rowEndingKind, -1);
                }
                else
                {
                    if (characterToDelete.Value == KeyboardKeyFacts.WhitespaceCharacters.TAB)
                        TabKeyPositionsBag.Remove(indexToRemove);

                    startingIndexToRemoveRange = indexToRemove;
                    countToRemoveRange = 1;
                }

                charactersRemovedCount += countToRemoveRange;

                _contentBag.RemoveRange(startingIndexToRemoveRange, countToRemoveRange);

                if (moveBackwards)
                    indexToRemove -= countToRemoveRange;
            }

            if (charactersRemovedCount == 0 && rowsRemovedCount == 0)
                return;

            if (moveBackwards && !selectionUpperBoundRowIndex.HasValue)
            {
                var modifyRowsBy = -1 * rowsRemovedCount;

                var startOfCurrentRowPositionIndex = this.GetStartOfRowTuple(cursor.RowIndex + modifyRowsBy)
                    .positionIndex;

                var modifyPositionIndexBy = -1 * charactersRemovedCount;

                var endingPositionIndex = cursorPositionIndex + modifyPositionIndexBy;

                var columnIndex = endingPositionIndex - startOfCurrentRowPositionIndex;

                var rowIndex = cursor.RowIndex;

                cursor.RowIndex = rowIndex + modifyRowsBy;
                cursor.ColumnIndex = columnIndex;
            }

            int firstRowIndexToModify;

            if (selectionUpperBoundRowIndex.HasValue)
            {
                firstRowIndexToModify = selectionLowerBoundIndexCoordinates!.Value.rowIndex;
                cursor.RowIndex = selectionLowerBoundIndexCoordinates!.Value.rowIndex;
                cursor.ColumnIndex = selectionLowerBoundIndexCoordinates!.Value.columnIndex;
            }
            else if (moveBackwards)
            {
                firstRowIndexToModify = cursor.RowIndex - rowsRemovedCount;
            }
            else
            {
                firstRowIndexToModify = cursor.RowIndex;
            }

            for (var i = firstRowIndexToModify; i < RowEndingPositionsBag.Count; i++)
            {
                var rowEndingTuple = RowEndingPositionsBag[i];
                _rowEndingPositionsBag[i] = (rowEndingTuple.positionIndex - charactersRemovedCount, rowEndingTuple.rowEndingKind);
            }

            var firstTabKeyPositionIndexToModify = _tabKeyPositionsBag.FindIndex(x => x >= startingPositionIndexToRemoveInclusive);

            if (firstTabKeyPositionIndexToModify != -1)
            {
                for (var i = firstTabKeyPositionIndexToModify; i < TabKeyPositionsBag.Count; i++)
                {
                    TabKeyPositionsBag[i] -= charactersRemovedCount;
                }
            }

            // Reposition the Diagnostic Squigglies
            {
                var textSpanForInsertion = new TextEditorTextSpan(
                    cursorPositionIndex,
                    cursorPositionIndex + charactersRemovedCount,
                    0,
                    new(string.Empty),
                    string.Empty);

                var textModification = new TextEditorTextModification(false, textSpanForInsertion);

                foreach (var presentationModel in PresentationModelsBag)
                {
                    if (presentationModel.CompletedCalculation is not null)
                    {
                        presentationModel.CompletedCalculation.TextModificationsSinceRequestBag.Add(textModification);
                    }

                    if (presentationModel.PendingCalculation is not null)
                    {
                        presentationModel.PendingCalculation.TextModificationsSinceRequestBag.Add(textModification);
                    }
                }
            }
        }

        // TODO: Fix tracking the MostCharactersOnASingleRowTuple this way is possibly inefficient - should instead only check the rows that changed
        {
            (int rowIndex, int rowLength) localMostCharactersOnASingleRowTuple = (0, 0);

            for (var i = 0; i < RowEndingPositionsBag.Count; i++)
            {
                var lengthOfRow = this.GetLengthOfRow(i);

                if (lengthOfRow > localMostCharactersOnASingleRowTuple.rowLength)
                {
                    localMostCharactersOnASingleRowTuple = (i, lengthOfRow);
                }
            }

            localMostCharactersOnASingleRowTuple = (localMostCharactersOnASingleRowTuple.rowIndex,
                localMostCharactersOnASingleRowTuple.rowLength + TextEditorModel.MOST_CHARACTERS_ON_A_SINGLE_ROW_MARGIN);

            _mostCharactersOnASingleRowTuple = localMostCharactersOnASingleRowTuple;
        }
    }

    private void MutateRowEndingKindCount(RowEndingKind rowEndingKind, int changeBy)
    {
        // Any modified state needs to be 'null coallesce assigned' to the existing TextEditorModel's value
        //
        // When reading state, if the state had been 'null coallesce assigned' then the field will
        // be read. Otherwise, the existing TextEditorModel's value will be read.
        {
            _rowEndingKindCountsBag ??= _textEditorModel.RowEndingKindCountsBag.ToList();
        }

        var indexOfRowEndingKindCount = _rowEndingKindCountsBag.FindIndex(x => x.rowEndingKind == rowEndingKind);
        var currentRowEndingKindCount = RowEndingKindCountsBag[indexOfRowEndingKindCount].count;

        RowEndingKindCountsBag[indexOfRowEndingKindCount] = (rowEndingKind, currentRowEndingKindCount + changeBy);

        CheckRowEndingPositions(false);
    }

    private void CheckRowEndingPositions(bool setUsingRowEndingKind)
    {
        // Any modified state needs to be 'null coallesce assigned' to the existing TextEditorModel's value
        //
        // When reading state, if the state had been 'null coallesce assigned' then the field will
        // be read. Otherwise, the existing TextEditorModel's value will be read.
        {
            _rowEndingKindCountsBag ??= _textEditorModel.RowEndingKindCountsBag.ToList();
            _onlyRowEndingKind ??= _textEditorModel.OnlyRowEndingKind;
            _usingRowEndingKind ??= _textEditorModel.UsingRowEndingKind;
        }

        var existingRowEndingsBag = RowEndingKindCountsBag
            .Where(x => x.count > 0)
            .ToArray();

        if (!existingRowEndingsBag.Any())
        {
            _onlyRowEndingKind = RowEndingKind.Unset;
            _usingRowEndingKind = RowEndingKind.Linefeed;
        }
        else
        {
            if (existingRowEndingsBag.Length == 1)
            {
                var rowEndingKind = existingRowEndingsBag.Single().rowEndingKind;

                if (setUsingRowEndingKind)
                    _usingRowEndingKind = rowEndingKind;

                _onlyRowEndingKind = rowEndingKind;
            }
            else
            {
                if (setUsingRowEndingKind)
                    _usingRowEndingKind = existingRowEndingsBag.MaxBy(x => x.count).rowEndingKind;

                _onlyRowEndingKind = null;
            }
        }
    }

    public void ModifyContent(string content)
    {
        // Any modified state needs to be 'null coallesce assigned' to the existing TextEditorModel's value
        //
        // When reading state, if the state had been 'null coallesce assigned' then the field will
        // be read. Otherwise, the existing TextEditorModel's value will be read.
        {
            _mostCharactersOnASingleRowTuple ??= _textEditorModel.MostCharactersOnASingleRowTuple;
            _rowEndingPositionsBag ??= _textEditorModel.RowEndingPositionsBag.ToList();
            _tabKeyPositionsBag ??= _textEditorModel.TabKeyPositionsBag.ToList();
            _contentBag ??= _textEditorModel.ContentBag.ToList();
            _rowEndingKindCountsBag ??= _textEditorModel.RowEndingKindCountsBag.ToList();
        }

        ModifyResetStateButNotEditHistory();

        var rowIndex = 0;
        var previousCharacter = '\0';

        var charactersOnRow = 0;

        var carriageReturnCount = 0;
        var linefeedCount = 0;
        var carriageReturnLinefeedCount = 0;

        for (var index = 0; index < content.Length; index++)
        {
            var character = content[index];

            charactersOnRow++;

            if (character == KeyboardKeyFacts.WhitespaceCharacters.CARRIAGE_RETURN)
            {
                if (charactersOnRow > MostCharactersOnASingleRowTuple.rowLength - TextEditorModel.MOST_CHARACTERS_ON_A_SINGLE_ROW_MARGIN)
                    _mostCharactersOnASingleRowTuple = (rowIndex, charactersOnRow + TextEditorModel.MOST_CHARACTERS_ON_A_SINGLE_ROW_MARGIN);

                RowEndingPositionsBag.Add((index + 1, RowEndingKind.CarriageReturn));
                rowIndex++;

                charactersOnRow = 0;

                carriageReturnCount++;
            }
            else if (character == KeyboardKeyFacts.WhitespaceCharacters.NEW_LINE)
            {
                if (charactersOnRow > MostCharactersOnASingleRowTuple.rowLength - TextEditorModel.MOST_CHARACTERS_ON_A_SINGLE_ROW_MARGIN)
                    _mostCharactersOnASingleRowTuple = (rowIndex, charactersOnRow + TextEditorModel.MOST_CHARACTERS_ON_A_SINGLE_ROW_MARGIN);

                if (previousCharacter == KeyboardKeyFacts.WhitespaceCharacters.CARRIAGE_RETURN)
                {
                    var lineEnding = RowEndingPositionsBag[rowIndex - 1];

                    RowEndingPositionsBag[rowIndex - 1] = (lineEnding.positionIndex + 1, RowEndingKind.CarriageReturnLinefeed);

                    carriageReturnCount--;
                    carriageReturnLinefeedCount++;
                }
                else
                {
                    RowEndingPositionsBag.Add((index + 1, RowEndingKind.Linefeed));
                    rowIndex++;

                    linefeedCount++;
                }

                charactersOnRow = 0;
            }

            if (character == KeyboardKeyFacts.WhitespaceCharacters.TAB)
                TabKeyPositionsBag.Add(index);

            previousCharacter = character;

            ContentBag.Add(new RichCharacter
            {
                Value = character,
                DecorationByte = default,
            });
        }

        _rowEndingKindCountsBag.AddRange(new List<(RowEndingKind rowEndingKind, int count)>
        {
            (RowEndingKind.CarriageReturn, carriageReturnCount),
            (RowEndingKind.Linefeed, linefeedCount),
            (RowEndingKind.CarriageReturnLinefeed, carriageReturnLinefeedCount),
        });

        CheckRowEndingPositions(true);

        RowEndingPositionsBag.Add((content.Length, RowEndingKind.EndOfFile));
    }

    public void ModifyResetStateButNotEditHistory()
    {
        ClearContentBag();
        ClearRowEndingKindCountsBag();
        ClearRowEndingPositionsBag();
        ClearTabKeyPositionsBag();
        ModifyOnlyRowEndingKind();
        ModifyUsingRowEndingKind(RowEndingKind.Unset);
    }

    public void PerformEditTextEditorAction(KeyboardEventAction keyboardEventAction)
    {
        if (KeyboardKeyFacts.IsMetaKey(keyboardEventAction.KeyboardEventArgs))
        {
            if (KeyboardKeyFacts.MetaKeys.BACKSPACE == keyboardEventAction.KeyboardEventArgs.Key ||
                KeyboardKeyFacts.MetaKeys.DELETE == keyboardEventAction.KeyboardEventArgs.Key)
            {
                PerformDeletions(keyboardEventAction);
            }
        }
        else
        {
            var cursorBag = keyboardEventAction.CursorBag;

            var primaryCursorSnapshot = cursorBag.FirstOrDefault(x => x.IsPrimaryCursor);

            if (primaryCursorSnapshot is null)
                return;

            /*
             * TODO: 2022-11-18 one must not use the UserCursor while
             * calculating but instead make a copy of the mutable cursor
             * by looking at the snapshot and mutate that local 'userCursor'
             * then once the transaction is done offer the 'userCursor' to the
             * user interface such that it can choose to move the actual user cursor
             * to that position
             */

            // TODO: start making a mutable copy of their immutable cursor snapshot
            // so if the user moves the cursor
            // while calculating nothing can go wrong causing exception
            //
            // See the var localCursor in this contiguous code block.
            //
            // var localCursor = new TextEditorCursor(
            //     (primaryCursorSnapshot.RowIndex, primaryCursorSnapshot.ColumnIndex), 
            //     true);

            if (TextEditorSelectionHelper.HasSelectedText(primaryCursorSnapshot.Selection))
            {
                PerformDeletions(new KeyboardEventAction(
                    keyboardEventAction.ResourceUri,
                    cursorBag,
                    new KeyboardEventArgs
                    {
                        Code = KeyboardKeyFacts.MetaKeys.DELETE,
                        Key = KeyboardKeyFacts.MetaKeys.DELETE,
                    },
                    CancellationToken.None));
            }

            var innerCursorBag = keyboardEventAction.CursorBag;

            PerformInsertions(keyboardEventAction with
            {
                CursorBag = innerCursorBag
            });
        }
    }

    public void PerformEditTextEditorAction(InsertTextAction insertTextAction)
    {
        var cursorBag = insertTextAction.CursorBag;
        var primaryCursor = cursorBag.FirstOrDefault(x => x.IsPrimaryCursor);

        if (primaryCursor is null)
            return;

        /*
         * TODO: 2022-11-18 one must not use the UserCursor while
         * calculating but instead make a copy of the mutable cursor
         * by looking at the snapshot and mutate that local 'userCursor'
         * then once the transaction is done offer the 'userCursor' to the
         * user interface such that it can choose to move the actual user cursor
         * to that position
         */

        // TODO: start making a mutable copy of their immutable cursor snapshot
        // so if the user moves the cursor
        // while calculating nothing can go wrong causing exception
        //
        // See the var localCursor in this contiguous code block.
        //
        // var localCursor = new TextEditorCursor(
        //     (primaryCursorSnapshot.RowIndex, primaryCursorSnapshot.ColumnIndex), 
        //     true);

        if (TextEditorSelectionHelper.HasSelectedText(primaryCursor.Selection))
        {
            PerformDeletions(new KeyboardEventAction(
                insertTextAction.ResourceUri,
                cursorBag,
                new KeyboardEventArgs
                {
                    Code = KeyboardKeyFacts.MetaKeys.DELETE,
                    Key = KeyboardKeyFacts.MetaKeys.DELETE,
                },
                CancellationToken.None));
        }

        var localContent = insertTextAction.Content.Replace("\r\n", "\n");

        foreach (var character in localContent)
        {
            // TODO: This needs to be rewritten everything should be inserted at the same time not a foreach loop insertion for each character
            //
            // Need innerCursorSnapshots because need
            // after every loop of the foreach that the
            // cursor snapshots are updated
            var innerCursorBag = insertTextAction.CursorBag;

            var code = character switch
            {
                '\r' => KeyboardKeyFacts.WhitespaceCodes.ENTER_CODE,
                '\n' => KeyboardKeyFacts.WhitespaceCodes.ENTER_CODE,
                '\t' => KeyboardKeyFacts.WhitespaceCodes.TAB_CODE,
                ' ' => KeyboardKeyFacts.WhitespaceCodes.SPACE_CODE,
                _ => character.ToString(),
            };

            var keyboardEventTextEditorModelAction = new KeyboardEventAction(
                insertTextAction.ResourceUri,
                innerCursorBag,
                new KeyboardEventArgs
                {
                    Code = code,
                    Key = character.ToString(),
                },
                CancellationToken.None);

            PerformEditTextEditorAction(keyboardEventTextEditorModelAction);
        }
    }

    public void PerformEditTextEditorAction(DeleteTextByMotionAction deleteTextByMotionAction)
    {
        var keyboardEventArgs = deleteTextByMotionAction.MotionKind switch
        {
            MotionKind.Backspace => new KeyboardEventArgs { Key = KeyboardKeyFacts.MetaKeys.BACKSPACE },
            MotionKind.Delete => new KeyboardEventArgs { Key = KeyboardKeyFacts.MetaKeys.DELETE },
            _ => throw new ApplicationException($"The {nameof(MotionKind)}: {deleteTextByMotionAction.MotionKind} was not recognized.")
        };

        var keyboardEventTextEditorModelAction = new KeyboardEventAction(
            deleteTextByMotionAction.ResourceUri,
            deleteTextByMotionAction.CursorBag,
            keyboardEventArgs,
            CancellationToken.None);

        PerformEditTextEditorAction(keyboardEventTextEditorModelAction);
    }

    public void PerformEditTextEditorAction(DeleteTextByRangeAction deleteTextByRangeAction)
    {
        // TODO: This needs to be rewritten everything should be deleted at the same time not a foreach loop for each character
        for (var i = 0; i < deleteTextByRangeAction.Count; i++)
        {
            // Need innerCursorSnapshots because need
            // after every loop of the foreach that the
            // cursor snapshots are updated
            var innerCursorSnapshotsBag = deleteTextByRangeAction.CursorBag;

            var keyboardEventTextEditorModelAction = new KeyboardEventAction(
                deleteTextByRangeAction.ResourceUri,
                innerCursorSnapshotsBag,
                new KeyboardEventArgs
                {
                    Code = KeyboardKeyFacts.MetaKeys.DELETE,
                    Key = KeyboardKeyFacts.MetaKeys.DELETE,
                },
                CancellationToken.None);

            PerformEditTextEditorAction(keyboardEventTextEditorModelAction);
        }
    }

    public void PerformRegisterPresentationModelAction(RegisterPresentationModelAction registerPresentationModelAction)
    {
        // Any modified state needs to be 'null coallesce assigned' to the existing TextEditorModel's value
        //
        // When reading state, if the state had been 'null coallesce assigned' then the field will
        // be read. Otherwise, the existing TextEditorModel's value will be read.
        {
            _presentationModelsBag ??= _textEditorModel.PresentationModelsBag.ToList();
        }

        if (!PresentationModelsBag.Any(x => x.TextEditorPresentationKey == registerPresentationModelAction.PresentationModel.TextEditorPresentationKey))
            PresentationModelsBag.Add(registerPresentationModelAction.PresentationModel);
    }

    public void PerformCalculatePresentationModelAction(CalculatePresentationModelAction calculatePresentationModelAction)
    {
        // Any modified state needs to be 'null coallesce assigned' to the existing TextEditorModel's value
        //
        // When reading state, if the state had been 'null coallesce assigned' then the field will
        // be read. Otherwise, the existing TextEditorModel's value will be read.
        {
            _presentationModelsBag ??= _textEditorModel.PresentationModelsBag.ToList();
        }

        var indexOfPresentationModel = _presentationModelsBag.FindIndex(
            x => x.TextEditorPresentationKey == calculatePresentationModelAction.PresentationKey);

        if (indexOfPresentationModel == -1)
            return;

        var presentationModel = PresentationModelsBag[indexOfPresentationModel];

        presentationModel.PendingCalculation = new(this.GetAllText());
    }

    public void ClearEditBlocks()
    {
        // Any modified state needs to be 'null coallesce assigned' to the existing TextEditorModel's value
        //
        // When reading state, if the state had been 'null coallesce assigned' then the field will
        // be read. Otherwise, the existing TextEditorModel's value will be read.
        {
            _editBlocksBag ??= _textEditorModel.EditBlocksBag.ToList();
            _editBlockIndex ??= _textEditorModel.EditBlockIndex;
        }

        _editBlockIndex = 0;
        EditBlocksBag.Clear();
    }

    /// <summary>The "if (EditBlockIndex == _editBlocksPersisted.Count)"<br/><br/>Is done because the active EditBlock is not yet persisted.<br/><br/>The active EditBlock is instead being 'created' as the user continues to make edits of the same <see cref="TextEditKind"/><br/><br/>For complete clarity, this comment refers to one possibly expecting to see "if (EditBlockIndex == _editBlocksPersisted.Count - 1)"</summary>
    public void UndoEdit()
    {
        // Any modified state needs to be 'null coallesce assigned' to the existing TextEditorModel's value
        //
        // When reading state, if the state had been 'null coallesce assigned' then the field will
        // be read. Otherwise, the existing TextEditorModel's value will be read.
        {
            _editBlocksBag ??= _textEditorModel.EditBlocksBag.ToList();
            _editBlockIndex ??= _textEditorModel.EditBlockIndex;
        }

        if (!this.CanUndoEdit())
            return;

        if (EditBlockIndex == EditBlocksBag.Count)
        {
            // If the edit block is pending then persist it
            // before reverting back to the previous persisted edit block.

            EnsureUndoPoint(TextEditKind.ForcePersistEditBlock);
            _editBlockIndex--;
        }

        _editBlockIndex--;

        var restoreEditBlock = EditBlocksBag[EditBlockIndex];

        ModifyContent(restoreEditBlock.ContentSnapshot);
    }

    public void RedoEdit()
    {
        // Any modified state needs to be 'null coallesce assigned' to the existing TextEditorModel's value
        //
        // When reading state, if the state had been 'null coallesce assigned' then the field will
        // be read. Otherwise, the existing TextEditorModel's value will be read.
        {
            _editBlocksBag ??= _textEditorModel.EditBlocksBag.ToList();
            _editBlockIndex ??= _textEditorModel.EditBlockIndex;
        }
        
        if (!this.CanRedoEdit())
            return;

        _editBlockIndex++;

        var restoreEditBlock = EditBlocksBag[EditBlockIndex];

        ModifyContent(restoreEditBlock.ContentSnapshot);
    }
}