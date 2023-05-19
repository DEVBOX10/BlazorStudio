﻿using BlazorStudio.ClassLib.Parsing.C.SyntaxTokens;
using System.Collections.Immutable;

namespace BlazorStudio.ClassLib.Parsing.C.BoundNodes.Expression;

public class BoundLiteralExpressionNode : IBoundExpressionNode
{
    public BoundLiteralExpressionNode(
        ISyntaxToken literalSyntaxToken,
        Type resultType)
    {
        LiteralSyntaxToken = literalSyntaxToken;
        ResultType = resultType;

        Children = new ISyntax[]
        {
            literalSyntaxToken
        }.ToImmutableArray();
    }

    public ISyntaxToken LiteralSyntaxToken { get; }
    public Type ResultType { get; }

    public ImmutableArray<ISyntax> Children { get; }
    public bool IsFabricated { get; init; }
    public SyntaxKind SyntaxKind => SyntaxKind.BoundLiteralExpressionNode;
}
