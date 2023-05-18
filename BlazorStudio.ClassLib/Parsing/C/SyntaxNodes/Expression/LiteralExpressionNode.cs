﻿using BlazorStudio.ClassLib.Parsing.C.SyntaxTokens;
using System.Collections.Immutable;

namespace BlazorStudio.ClassLib.Parsing.C.SyntaxNodes.Expression;

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
    public SyntaxKind SyntaxKind => SyntaxKind.LiteralExpressionNode;
}