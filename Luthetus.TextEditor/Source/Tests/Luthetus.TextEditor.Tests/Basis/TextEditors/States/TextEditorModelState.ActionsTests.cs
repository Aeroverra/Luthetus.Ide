using Xunit;
using Luthetus.TextEditor.RazorLib.TextEditors.States;
using Luthetus.TextEditor.Tests.Basis.TextEditors.Models.TextEditorServices;
using Luthetus.TextEditor.RazorLib.TextEditors.Models;

namespace Luthetus.TextEditor.Tests.Basis.TextEditors.States;

/// <summary>
/// <see cref="TextEditorModelState"/>
/// </summary>
public class TextEditorModelStateActionsTests
{
	/// <summary>
	/// <see cref="TextEditorModelState.RegisterAction"/>
	/// </summary>
	[Fact]
	public void RegisterAction()
	{
        TextEditorServicesTestsHelper.InitializeTextEditorServicesTestsHelper(
            out var textEditorService,
            out var inModel,
            out var inViewModel,
            out var serviceProvider);

        var authenticatedActionKey = TextEditorService.AuthenticatedActionKey;

        var registerAction = new TextEditorModelState.RegisterAction(
            authenticatedActionKey,
            inModel);

		Assert.Equal(authenticatedActionKey, registerAction.AuthenticatedActionKey);
		Assert.Equal(inModel, registerAction.Model);
	}

	/// <summary>
	/// <see cref="TextEditorModelState.DisposeAction"/>
	/// </summary>
	[Fact]
	public void DisposeAction()
	{
        TextEditorServicesTestsHelper.InitializeTextEditorServicesTestsHelper(
            out var textEditorService,
            out var inModel,
            out var inViewModel,
            out var serviceProvider);
        
        var authenticatedActionKey = TextEditorService.AuthenticatedActionKey;

        var disposeAction = new TextEditorModelState.DisposeAction(
            authenticatedActionKey,
            inModel.ResourceUri);

        Assert.Equal(authenticatedActionKey, disposeAction.AuthenticatedActionKey);
        Assert.Equal(inModel.ResourceUri, disposeAction.ResourceUri);
	}
}