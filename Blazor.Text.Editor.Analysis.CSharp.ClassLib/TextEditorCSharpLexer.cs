﻿using System.Collections.Immutable;
using BlazorTextEditor.RazorLib.Lexing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Blazor.Text.Editor.Analysis.CSharp.ClassLib;

public class TextEditorCSharpLexer : ILexer
{
    public async Task<ImmutableArray<TextEditorTextSpan>> Lex(string content)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(content);

        var syntaxNodeRoot = await syntaxTree.GetRootAsync();

        var generalSyntaxCollector = new GeneralSyntaxCollector();

        generalSyntaxCollector.Visit(syntaxNodeRoot);

        List<TextEditorTextSpan> textEditorTextSpans = new();

        // Type decorations
        {
            var decorationByte = (byte)CSharpDecorationKind.Type;

            // Property Type
            textEditorTextSpans.AddRange(generalSyntaxCollector.PropertyDeclarationSyntaxes
                .Select(pds => pds.Type.Span)
                .Select(roslynSpan =>
                    new TextEditorTextSpan(
                        roslynSpan.Start,
                        roslynSpan.End,
                        decorationByte)));

            // Class Declaration
            textEditorTextSpans.AddRange(generalSyntaxCollector.ClassDeclarationSyntaxes
                .Select(cds => cds.Identifier.Span)
                .Select(roslynSpan =>
                    new TextEditorTextSpan(
                        roslynSpan.Start,
                        roslynSpan.End,
                        decorationByte)));

            // Method return Type
            textEditorTextSpans.AddRange(generalSyntaxCollector.MethodDeclarationSyntaxes
                .Select(mds =>
                {
                    var retType = mds
                        .ChildNodes()
                        .FirstOrDefault(x => x.IsKind(SyntaxKind.IdentifierName));

                    return retType?.Span ?? default;
                })
                .Select(roslynSpan =>
                    new TextEditorTextSpan(
                        roslynSpan.Start,
                        roslynSpan.End,
                        decorationByte)));

            // Parameter declaration Type
            textEditorTextSpans.AddRange(generalSyntaxCollector.ParameterSyntaxes
                .Select(ps =>
                {
                    var identifierNameNode = ps.ChildNodes()
                        .FirstOrDefault(x => x.IsKind(SyntaxKind.IdentifierName));

                    if (identifierNameNode is null) return TextSpan.FromBounds(0, 0);

                    return identifierNameNode.Span;
                })
                .Select(roslynSpan =>
                    new TextEditorTextSpan(
                        roslynSpan.Start,
                        roslynSpan.End,
                        decorationByte)));
        }

        // Method decorations
        {
            var decorationByte = (byte)CSharpDecorationKind.Method;

            // Method declaration identifier
            textEditorTextSpans.AddRange(generalSyntaxCollector.MethodDeclarationSyntaxes
                .Select(mds => mds.Identifier.Span)
                .Select(roslynSpan =>
                    new TextEditorTextSpan(
                        roslynSpan.Start,
                        roslynSpan.End,
                        decorationByte)));

            // InvocationExpression
            textEditorTextSpans.AddRange(generalSyntaxCollector.InvocationExpressionSyntaxes
                .Select(ies =>
                {
                    var childNodes = ies.Expression.ChildNodes();

                    var lastNode = childNodes.LastOrDefault();

                    return lastNode?.Span ?? TextSpan.FromBounds(0, 0);
                })
                .Select(roslynSpan =>
                    new TextEditorTextSpan(
                        roslynSpan.Start,
                        roslynSpan.End,
                        decorationByte)));
        }

        // Local variable decorations
        {
            var decorationByte = (byte)CSharpDecorationKind.Parameter;

            // Parameter declaration identifier
            textEditorTextSpans.AddRange(generalSyntaxCollector.ParameterSyntaxes
                .Select(ps =>
                {
                    var identifierToken =
                        ps.ChildTokens()
                            .FirstOrDefault(x =>
                                x.IsKind(SyntaxKind.IdentifierToken));

                    return identifierToken.Span;
                })
                .Select(roslynSpan =>
                    new TextEditorTextSpan(
                        roslynSpan.Start,
                        roslynSpan.End,
                        decorationByte)));

            // Argument declaration identifier
            textEditorTextSpans.AddRange(generalSyntaxCollector.ArgumentSyntaxes
                .Select(argumentSyntax => argumentSyntax.Span)
                .Select(roslynSpan =>
                    new TextEditorTextSpan(
                        roslynSpan.Start,
                        roslynSpan.End,
                        decorationByte)));
        }

        // String literal
        {
            var decorationByte = (byte)CSharpDecorationKind.StringLiteral;

            // String literal
            textEditorTextSpans.AddRange(generalSyntaxCollector.StringLiteralExpressionSyntaxes
                .Select(sles => sles.Span)
                .Select(roslynSpan =>
                    new TextEditorTextSpan(
                        roslynSpan.Start,
                        roslynSpan.End,
                        decorationByte)));
        }

        // Keywords
        {
            var decorationByte = (byte)CSharpDecorationKind.Keyword;

            // Keywords
            textEditorTextSpans.AddRange(generalSyntaxCollector.KeywordSyntaxTokens
                .Select(kst => kst.Span)
                .Select(roslynSpan =>
                    new TextEditorTextSpan(
                        roslynSpan.Start,
                        roslynSpan.End,
                        decorationByte)));

            // Contextual var keyword
            textEditorTextSpans.AddRange(generalSyntaxCollector
                .VarTextSpans
                .Select(roslynSpan =>
                    new TextEditorTextSpan(
                        roslynSpan.Start,
                        roslynSpan.End,
                        decorationByte)));
        }

        // Comments
        {
            var decorationByte = (byte)CSharpDecorationKind.Comment;

            // Default comments
            textEditorTextSpans.AddRange(generalSyntaxCollector.SyntaxTrivias
                .Select(st => st.Span)
                .Select(roslynSpan =>
                    new TextEditorTextSpan(
                        roslynSpan.Start,
                        roslynSpan.End,
                        decorationByte)));

            // Xml comments
            textEditorTextSpans.AddRange(generalSyntaxCollector.XmlCommentSyntaxes
                .Select(xml => xml.Span)
                .Select(roslynSpan =>
                    new TextEditorTextSpan(
                        roslynSpan.Start,
                        roslynSpan.End,
                        decorationByte)));
        }

        return textEditorTextSpans.ToImmutableArray();
    }
}