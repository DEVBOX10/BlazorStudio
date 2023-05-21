﻿using BlazorStudio.ClassLib.CodeAnalysis.C.Syntax.SyntaxTokens;
using System.Collections.Immutable;

namespace BlazorStudio.ClassLib.CodeAnalysis.C.Syntax.SyntaxNodes.Expression;

public class LiteralExpressionNode : IExpressionNode
{
    public LiteralExpressionNode(ISyntaxToken literalSyntaxToken)
    {
        LiteralSyntaxToken = literalSyntaxToken;

        Children = new ISyntax[]
        {
            LiteralSyntaxToken
        }.ToImmutableArray();
    }

    public ISyntaxToken LiteralSyntaxToken { get; }

    public ImmutableArray<ISyntax> Children { get; }
    public bool IsFabricated { get; init; }
    public SyntaxKind SyntaxKind => SyntaxKind.LiteralExpressionNode;
}
