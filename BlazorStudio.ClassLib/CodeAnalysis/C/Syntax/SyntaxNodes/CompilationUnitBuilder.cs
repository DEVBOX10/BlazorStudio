﻿using BlazorTextEditor.RazorLib.Analysis;
using System.Collections.Immutable;

namespace BlazorStudio.ClassLib.CodeAnalysis.C.Syntax.SyntaxNodes;

public class CompilationUnitBuilder
{
    public CompilationUnitBuilder(CompilationUnitBuilder? parent)
    {
        Parent = parent;
    }

    public bool IsExpression { get; set; }
    public List<ISyntax> Children { get; set; } = new();
    public CompilationUnitBuilder? Parent { get; }

    public CompilationUnit Build()
    {
        return new CompilationUnit(
            IsExpression,
            Children.ToImmutableArray());
    }

    public CompilationUnit Build(
        ImmutableArray<TextEditorDiagnostic> diagnostics)
    {
        return new CompilationUnit(
            IsExpression,
            Children.ToImmutableArray(),
            diagnostics);
    }
}