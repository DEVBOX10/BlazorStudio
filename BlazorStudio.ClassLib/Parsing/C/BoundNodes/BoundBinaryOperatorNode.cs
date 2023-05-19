﻿using BlazorStudio.ClassLib.Parsing.C.SyntaxNodes;
using BlazorStudio.ClassLib.Parsing.C.SyntaxNodes.Expression;
using BlazorStudio.ClassLib.Parsing.C.SyntaxTokens;
using System.Collections.Immutable;

namespace BlazorStudio.ClassLib.Parsing.C.BoundNodes;

public class BoundBinaryOperatorNode : ISyntaxNode
{
    public BoundBinaryOperatorNode(
        Type leftOperandType,
        ISyntaxToken operatorToken,
        Type rightOperandType,
        Type resultType)
    {
        LeftOperandType = leftOperandType;
        OperatorToken = operatorToken;
        RightOperandType = rightOperandType;
        ResultType = resultType;

        Children = new ISyntax[] 
        {
            OperatorToken
        }
        .ToImmutableArray();
    }

    public Type LeftOperandType { get; }
    public ISyntaxToken OperatorToken { get; }
    public Type RightOperandType { get; }
    public Type ResultType { get; }

    public ImmutableArray<ISyntax> Children { get; }
    public bool IsFabricated { get; init; }
    public SyntaxKind SyntaxKind => SyntaxKind.BoundBinaryOperatorNode;
}
