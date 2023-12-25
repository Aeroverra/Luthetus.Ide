using System.Collections.Immutable;

namespace Luthetus.TextEditor.RazorLib.CompilerServices.Syntax.SyntaxNodes;

/// <summary>
/// Used when invoking a syntax which contains a generic type.
/// </summary>
public sealed record GenericParameterEntryNode : ISyntaxNode
{
    public GenericParameterEntryNode(TypeClauseNode typeClauseNode)
    {
        TypeClauseNode = typeClauseNode;

        var children = new List<ISyntax>
        {
            TypeClauseNode
        };

        ChildBag = children.ToImmutableArray();
    }

    public TypeClauseNode TypeClauseNode { get; }

    public ImmutableArray<ISyntax> ChildBag { get; }

    public bool IsFabricated { get; init; }
    public SyntaxKind SyntaxKind => SyntaxKind.GenericParameterEntryNode;
}