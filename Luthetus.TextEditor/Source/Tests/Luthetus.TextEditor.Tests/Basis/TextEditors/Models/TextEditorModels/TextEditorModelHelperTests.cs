﻿using Xunit;
using Luthetus.TextEditor.RazorLib.TextEditors.Models.TextEditorModels;
using Luthetus.TextEditor.Tests.Basis.TextEditors.Models.TextEditorServices;
using Luthetus.TextEditor.RazorLib.Rows.Models;
using Luthetus.TextEditor.RazorLib.Cursors.Models;

namespace Luthetus.TextEditor.Tests.Basis.TextEditors.Models.TextEditorModels;

/// <summary>
/// <see cref="TextEditorModelHelper"/>
/// </summary>
public class TextEditorModelHelperTests
{
	/// <summary>
	/// <see cref="TextEditorModelHelper.GetRowEndingThatCreatedRow(ITextEditorModel, int)"/>
	/// </summary>
	[Fact]
	public void GetStartOfRowTuple()
	{
		// This test is broken down into code blocks, one for each subcase.
		// TODO: Perhaps separate the subcases as individual tests.

		// InBounds
		{
			{
                TextEditorServicesTestsHelper.InBounds_StartOfRow(
					out var model,
					out var cursor);

				var expected = new RowEnding(12, 13, RowEndingKind.Linefeed);

				// TextEditorModel
				{
					var actual = model.GetRowEndingThatCreatedRow(cursor.RowIndex);
                    Assert.Equal(expected, actual);
                }
				
				// TextEditorModelModifier
				{
					var modelModifier = new TextEditorModelModifier(model);
                    var actual = modelModifier.GetRowEndingThatCreatedRow(cursor.RowIndex);
                    Assert.Equal(expected, actual);
                }
            }

            // !start_of_row && !end_of_row
            {
                TextEditorServicesTestsHelper.InBounds_NOT_StartOfRow_AND_NOT_EndOfRow(
                    out var model,
                    out var cursor);

                var expected = new RowEnding(12, 13, RowEndingKind.Linefeed);

                // TextEditorModel
                {
                    var actual = model.GetRowEndingThatCreatedRow(cursor.RowIndex);
                    Assert.Equal(expected, actual);
                }

                // TextEditorModelModifier
                {
                    var modelModifier = new TextEditorModelModifier(model);
                    var actual = modelModifier.GetRowEndingThatCreatedRow(cursor.RowIndex);
                    Assert.Equal(expected, actual);
                }
            }
			
			// End of a row && !end_of_document
			{
                TextEditorServicesTestsHelper.InBounds_EndOfRow(
                    out var model,
                    out var cursor);

                var expected = new RowEnding(12, 13, RowEndingKind.Linefeed);

                // TextEditorModel
                {
                    var actual = model.GetRowEndingThatCreatedRow(cursor.RowIndex);
                    Assert.Equal(expected, actual);
                }

                // TextEditorModelModifier
                {
                    var modelModifier = new TextEditorModelModifier(model);
                    var actual = modelModifier.GetRowEndingThatCreatedRow(cursor.RowIndex);
                    Assert.Equal(expected, actual);
                }
            }
			
			// Start of document
			{
                TextEditorServicesTestsHelper.InBounds_StartOfDocument(
                    out var model,
                    out var cursor);

                var expected = new RowEnding(0, 0, RowEndingKind.StartOfFile);

                // TextEditorModel
                {
                    var actual = model.GetRowEndingThatCreatedRow(cursor.RowIndex);
                    Assert.Equal(expected, actual);
                }

                // TextEditorModelModifier
                {
                    var modelModifier = new TextEditorModelModifier(model);
                    var actual = modelModifier.GetRowEndingThatCreatedRow(cursor.RowIndex);
                    Assert.Equal(expected, actual);
                }
            }

            // End of document
			//
            // NOTE: end of document is to mean: position_index == document.Length...
			// ...the reason for this is that an insertion cursor is allowed to go 1 index...
			// ...beyond the length of the document.
            {
                TextEditorServicesTestsHelper.InBounds_EndOfDocument(
                    out var model,
                    out var cursor);

                var expected = new RowEnding(24, 25, RowEndingKind.Linefeed);

                // TextEditorModel
                {
                    var actual = model.GetRowEndingThatCreatedRow(cursor.RowIndex);
                    Assert.Equal(expected, actual);
                }

                // TextEditorModelModifier
                {
                    var modelModifier = new TextEditorModelModifier(model);
                    var actual = modelModifier.GetRowEndingThatCreatedRow(cursor.RowIndex);
                    Assert.Equal(expected, actual);
                }
            }
		}

		// OutOfBounds
		{
			// position_index < 0
			{
                TextEditorServicesTestsHelper.OutOfBounds_PositionIndex_LESS_THAN_Zero(
                    out var model,
                    out var cursor);

                var expected = new RowEnding(0, 0, RowEndingKind.StartOfFile);

                // TextEditorModel
                {
                    var actual = model.GetRowEndingThatCreatedRow(cursor.RowIndex);
                    Assert.Equal(expected, actual);
                }

                // TextEditorModelModifier
                {
                    var modelModifier = new TextEditorModelModifier(model);
                    var actual = modelModifier.GetRowEndingThatCreatedRow(cursor.RowIndex);
                    Assert.Equal(expected, actual);
                }
            }

            // position_index > document.Length + 1
            {
                TextEditorServicesTestsHelper.OutOfBounds_PositionIndex_GREATER_THAN_DocumentLength_PLUS_One(
                    out var model,
                    out var cursor);

                var expected = new RowEnding(24, 25, RowEndingKind.Linefeed);

                // TextEditorModel
                {
                    var actual = model.GetRowEndingThatCreatedRow(cursor.RowIndex);
                    Assert.Equal(expected, actual);
                }

                // TextEditorModelModifier
                {
                    var modelModifier = new TextEditorModelModifier(model);
                    var actual = modelModifier.GetRowEndingThatCreatedRow(cursor.RowIndex);
                    Assert.Equal(expected, actual);
                }
            }
        }
	}

	/// <summary>
	/// <see cref="TextEditorModelHelper.GetLengthOfRow(ITextEditorModel, int, bool)"/>
	/// </summary>
	[Fact]
	public void GetLengthOfRow()
	{
		// Negative rowIndex
		{
            throw new NotImplementedException();
        }

		// First row
		{
            throw new NotImplementedException();
        }

		// Row which is between first and last row.
		{
            throw new NotImplementedException();
        }

        // Last row
        {
            throw new NotImplementedException();
        }

        // rowIndex > document.Rows.Count
        {
            throw new NotImplementedException();
        }
    }

	/// <summary>
	/// <see cref="TextEditorModelHelper.GetRows(ITextEditorModel, int, int)"/>
	/// </summary>
	[Fact]
	public void GetRows()
	{
        // Negative rowIndex
        {
            // Negative count
            {
                throw new NotImplementedException();
            }

            // Positive count
            {
				throw new NotImplementedException();
			}
        }

        // First row
        {
            // Negative count
            {
                throw new NotImplementedException();
            }

            // Positive count
            {
                throw new NotImplementedException();
            }
        }

        // Row which is between first and last row.
        {
            // Negative count
            {
                throw new NotImplementedException();
            }

            // Positive count
            {
                throw new NotImplementedException();
            }
        }

        // Last row
        {
            // Negative count
            {
                throw new NotImplementedException();
            }

            // Positive count
            {
                throw new NotImplementedException();
            }
        }

        // rowIndex > document.Rows.Count
        {
            // Negative count
            {
                throw new NotImplementedException();
            }

            // Positive count
            {
                throw new NotImplementedException();
            }
        }
	}

	/// <summary>
	/// <see cref="TextEditorModelHelper.GetTabsCountOnSameRowBeforeCursor(ITextEditorModel, int, int)"/>
	/// </summary>
	[Fact]
	public void GetTabsCountOnSameRowBeforeCursor()
	{
        // Negative rowIndex
        {
            // Negative columnIndex
            {
                throw new NotImplementedException();
            }

            // Positive columnIndex && WithinRange
            {
                throw new NotImplementedException();
            }

            // Positive columnIndex && OutOfRange
            {
                throw new NotImplementedException();
            }
        }

        // First row
        {
            // Negative columnIndex
            {
                throw new NotImplementedException();
            }

            // Positive columnIndex && WithinRange
            {
                throw new NotImplementedException();
            }
            
            // Positive columnIndex && OutOfRange
            {
                throw new NotImplementedException();
            }
        }

        // Row which is between first and last row.
        {
            // Negative columnIndex
            {
                throw new NotImplementedException();
            }

            // Positive columnIndex && WithinRange
            {
                throw new NotImplementedException();
            }

            // Positive columnIndex && OutOfRange
            {
                throw new NotImplementedException();
            }
        }

        // Last row
        {
            // Negative columnIndex
            {
                throw new NotImplementedException();
            }

            // Positive columnIndex && WithinRange
            {
                throw new NotImplementedException();
            }

            // Positive columnIndex && OutOfRange
            {
                throw new NotImplementedException();
            }
        }

        // rowIndex > document.Rows.Count
        {
            // Negative columnIndex
            {
                throw new NotImplementedException();
            }

            // Positive columnIndex && WithinRange
            {
                throw new NotImplementedException();
            }

            // Positive columnIndex && OutOfRange
            {
                throw new NotImplementedException();
            }
        }
    }

	/// <summary>
	/// <see cref="TextEditorModelHelper.ApplyDecorationRange(ITextEditorModel, IEnumerable{RazorLib.Lexes.Models.TextEditorTextSpan})"/>
	/// </summary>
	[Fact]
	public void ApplyDecorationRange()
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// <see cref="TextEditorModelHelper.ApplySyntaxHighlightingAsync(ITextEditorModel)"/>
	/// </summary>
	[Fact]
	public void ApplySyntaxHighlightingAsync()
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// <see cref="TextEditorModelHelper.GetAllText(ITextEditorModel)"/>
	/// </summary>
	[Fact]
	public void GetAllText()
	{
        // Empty
        {
            throw new NotImplementedException();
        }

        // NotEmpty
        {
		    throw new NotImplementedException();
        }
	}

	/// <summary>
	/// <see cref="TextEditorModelHelper.GetCursorPositionIndex(ITextEditorModel, TextEditorCursor)"/>
	/// </summary>
	[Fact]
	public void GetCursorPositionIndex_TextEditorCursor()
	{
        // Cursor.RowIndex < 0
        {
            throw new NotImplementedException();
        }

        // Cursor.RowIndex > 0 && Cursor.RowIndex is within bounds
        {
            // FirstRow
            {
                throw new NotImplementedException();
            }

            // !FirstRow && !LastRow
            {
                throw new NotImplementedException();
            }

            // LastRow
            {
                throw new NotImplementedException();
            }
        }

        // Cursor.RowIndex > 0 && Cursor.RowIndex is OUT of bounds
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// <see cref="TextEditorModelHelper.GetCursorPositionIndex(ITextEditorModel, TextEditorCursorModifier)"/>
    /// </summary>
    [Fact]
	public void GetCursorPositionIndex_TextEditorCursorModifier()
	{
        // CursorModifier.RowIndex < 0
        {
            throw new NotImplementedException();
        }

        // CursorModifier.RowIndex > 0 && CursorModifier.RowIndex is within bounds
        {
            // FirstRow
            {
                throw new NotImplementedException();
            }

            // !FirstRow && !LastRow
            {
                throw new NotImplementedException();
            }

            // LastRow
            {
                throw new NotImplementedException();
            }
        }

        // CursorModifier.RowIndex > 0 && CursorModifier.RowIndex is OUT of bounds
        {
            throw new NotImplementedException();
        }
    }

	/// <summary>
	/// <see cref="TextEditorModelHelper.GetPositionIndex(ITextEditorModel, int, int)"/>
	/// </summary>
	[Fact]
	public void GetPositionIndex()
	{
        // RowIndex < 0
        {
            throw new NotImplementedException();
        }

        // RowIndex > 0 && RowIndex is within bounds
        {
            // FirstRow
            {
                throw new NotImplementedException();
            }

            // !FirstRow && !LastRow
            {
                throw new NotImplementedException();
            }

            // LastRow
            {
                throw new NotImplementedException();
            }
        }

        // RowIndex > 0 && RowIndex is OUT of bounds
        {
            throw new NotImplementedException();
        }
    }

	/// <summary>
	/// <see cref="TextEditorModelHelper.GetTextAt(ITextEditorModel, int)"/>
	/// </summary>
	[Fact]
	public void GetTextAt()
	{
        // PositionIndex < 0
        {
            throw new NotImplementedException();
        }

        // PositionIndex > 0 && PositionIndex is within bounds
        {
            // PositionIndex resides on the FirstRow
            {
                throw new NotImplementedException();
            }

            //  PositionIndex does not reside on the FirstRow nor the LastRow
            {
                throw new NotImplementedException();
            }

            // PositionIndex resides on the LastRow
            {
                throw new NotImplementedException();
            }
        }

        // PositionIndex > 0 && PositionIndex is OUT of bounds
        {
            throw new NotImplementedException();
        }
    }

	/// <summary>
	/// <see cref="TextEditorModelHelper.GetTextRange(ITextEditorModel, int, int)"/>
	/// </summary>
	[Fact]
	public void GetTextRange()
	{
        // PositionIndex < 0
        {
            // Count < 0
            {
                throw new NotImplementedException();
            }

            // Count == 0
            {
                throw new NotImplementedException();
            }

            // Count > 0
            {
                throw new NotImplementedException();
            }

            // Count reads beyond the document length
            {
                throw new NotImplementedException();
            }
        }

        // PositionIndex > 0 && PositionIndex is within bounds
        {
            // PositionIndex resides on the FirstRow
            {
                // Count < 0
                {
                    throw new NotImplementedException();
                }

                // Count == 0
                {
                    throw new NotImplementedException();
                }

                // Count > 0
                {
                    throw new NotImplementedException();
                }

                // Count reads beyond the document length
                {
                    throw new NotImplementedException();
                }
            }

            //  PositionIndex does not reside on the FirstRow nor the LastRow
            {
                // Count < 0
                {
                    throw new NotImplementedException();
                }

                // Count == 0
                {
                    throw new NotImplementedException();
                }

                // Count > 0
                {
                    throw new NotImplementedException();
                }

                // Count reads beyond the document length
                {
                    throw new NotImplementedException();
                }
            }

            // PositionIndex resides on the LastRow
            {
                // Count < 0
                {
                    throw new NotImplementedException();
                }

                // Count == 0
                {
                    throw new NotImplementedException();
                }

                // Count > 0
                {
                    throw new NotImplementedException();
                }

                // Count reads beyond the document length
                {
                    throw new NotImplementedException();
                }
            }
        }

        // PositionIndex > 0 && PositionIndex is OUT of bounds
        {
            // Count < 0
            {
                throw new NotImplementedException();
            }

            // Count == 0
            {
                throw new NotImplementedException();
            }

            // Count > 0
            {
                throw new NotImplementedException();
            }

            // Count reads beyond the document length
            {
                throw new NotImplementedException();
            }
        }
    }

	/// <summary>
	/// <see cref="TextEditorModelHelper.GetLinesRange(ITextEditorModel, int, int)"/>
	/// </summary>
	[Fact]
	public void GetLinesRange()
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// <see cref="TextEditorModelHelper.GetWordAt(ITextEditorModel, int)"/>
	/// </summary>
	[Fact]
	public void GetWordAt()
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// <see cref="TextEditorModelHelper.FindRowInformation(ITextEditorModel, int)"/>
	/// </summary>
	[Fact]
	public void FindRowInformation()
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// <see cref="TextEditorModelHelper.GetColumnIndexOfCharacterWithDifferingKind(ITextEditorModel, int, int, bool)"/>
	/// </summary>
	[Fact]
	public void GetColumnIndexOfCharacterWithDifferingKind()
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// <see cref="TextEditorModelHelper.GetAllRichCharacters(ITextEditorModel)"/>
	/// </summary>
	[Fact]
	public void GetAllRichCharacters()
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// <see cref="TextEditorModelHelper.CanUndoEdit"/>
	/// </summary>
	[Fact]
	public void CanUndoEdit()
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// <see cref="TextEditorModelHelper.CanRedoEdit"/>
	/// </summary>
	[Fact]
	public void CanRedoEdit()
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// <see cref="TextEditorModelHelper.GetCharacterKindAt(ITextEditorModel, int)"/>
	/// </summary>
	[Fact]
	public void GetCharacterKindAt()
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// <see cref="TextEditorModelHelper.ReadPreviousWordOrDefault(ITextEditorModel, int, int, bool)"/>
	/// </summary>
	[Fact]
	public void ReadPreviousWordOrDefault()
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// <see cref="TextEditorModelHelper.ReadNextWordOrDefault(ITextEditorModel, int, int)"/>
	/// </summary>
	[Fact]
	public void ReadNextWordOrDefault()
	{
		throw new NotImplementedException();
	}

    /// <summary>
    /// <see cref="TextEditorModelHelper.GetTextOffsettingCursor(ITextEditorModel, TextEditorCursor)"/>
    /// </summary>
    [Fact]
	public void GetTextOffsettingCursor()
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// <see cref="TextEditorModelHelper.GetTextOnRow(ITextEditorModel, int)"/>
	/// </summary>
	[Fact]
	public void GetTextOnRow()
	{
		throw new NotImplementedException();
	}
}
