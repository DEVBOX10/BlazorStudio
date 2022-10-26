﻿using System.Collections.Immutable;
using Blazor.Text.Editor.Analysis.Html.ClassLib.SyntaxActors;
using BlazorTextEditor.RazorLib.Lexing;

namespace Blazor.Text.Editor.Analysis.Razor.ClassLib;

public class TextEditorRazorLexer : ILexer
{
    public Task<ImmutableArray<TextEditorTextSpan>> Lex(string content)
    {
        var htmlSyntaxUnit = HtmlSyntaxTree.ParseText(
            content,
            RazorInjectedLanguageFacts.RazorInjectedLanguageDefinition);

        var syntaxNodeRoot = htmlSyntaxUnit.RootTagSyntax;

        var htmlSyntaxWalker = new HtmlSyntaxWalker();

        htmlSyntaxWalker.Visit(syntaxNodeRoot);

        List<TextEditorTextSpan> textEditorTextSpans = new();

        // Tag Names
        {
            textEditorTextSpans.AddRange(htmlSyntaxWalker.TagNameSyntaxes
                .Select(tns => tns.TextEditorTextSpan));
        }

        // InjectedLanguageFragmentSyntaxes
        {
            textEditorTextSpans.AddRange(htmlSyntaxWalker.InjectedLanguageFragmentSyntaxes
                .Select(ilfs => ilfs.TextEditorTextSpan));
        }

        return Task.FromResult(textEditorTextSpans.ToImmutableArray());
    }
}