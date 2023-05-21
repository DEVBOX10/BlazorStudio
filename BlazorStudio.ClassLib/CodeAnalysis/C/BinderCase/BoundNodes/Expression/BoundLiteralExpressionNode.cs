﻿using BlazorStudio.ClassLib.CodeAnalysis.C.Syntax;
using BlazorStudio.ClassLib.CodeAnalysis.C.Syntax.SyntaxTokens;
using System.Collections.Immutable;

namespace BlazorStudio.ClassLib.CodeAnalysis.C.BinderCase.BoundNodes.Expression;

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
