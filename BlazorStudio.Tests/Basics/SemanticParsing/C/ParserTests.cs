﻿using BlazorStudio.ClassLib.Parsing.C;
using BlazorStudio.ClassLib.Parsing.C.BoundNodes;
using BlazorStudio.ClassLib.Parsing.C.BoundNodes.Expression;
using BlazorStudio.ClassLib.Parsing.C.SyntaxTokens;
using System.Xml.Linq;

namespace BlazorStudio.Tests.Basics.SemanticParsing.C;

public class ParserTests
{
    [Fact]
    public void SHOULD_PARSE_NUMERIC_LITERAL_EXPRESSION()
    {
        string sourceText = "3".ReplaceLineEndings("\n");

        var lexer = new Lexer(sourceText);
        lexer.Lex();

        var parser = new Parser(
            lexer.SyntaxTokens,
            sourceText);

        var compilationUnit = parser.Parse();

        Assert.Single(compilationUnit.Children);

        var boundLiteralExpressionNode = (BoundLiteralExpressionNode)compilationUnit
            .Children[0];

        Assert.Equal(typeof(int), boundLiteralExpressionNode.ResultType);
    }
    
    [Fact]
    public void SHOULD_PARSE_STRING_LITERAL_EXPRESSION()
    {
        string sourceText = "\"123abc\"".ReplaceLineEndings("\n");

        var lexer = new Lexer(sourceText);
        lexer.Lex();

        var parser = new Parser(
            lexer.SyntaxTokens,
            sourceText);

        var compilationUnit = parser.Parse();

        Assert.Single(compilationUnit.Children);

        var boundLiteralExpressionNode = (BoundLiteralExpressionNode)compilationUnit
            .Children[0];

        Assert.Equal(typeof(string), boundLiteralExpressionNode.ResultType);
    }
    
    [Fact]
    public void SHOULD_PARSE_NUMERIC_BINARY_EXPRESSION()
    {
        string sourceText = "3 + 3".ReplaceLineEndings("\n");

        var lexer = new Lexer(sourceText);
        lexer.Lex();

        var parser = new Parser(
            lexer.SyntaxTokens,
            sourceText);

        var compilationUnit = parser.Parse();

        Assert.Single(compilationUnit.Children);

        var boundBinaryExpressionNode = (BoundBinaryExpressionNode)compilationUnit
            .Children[0];

        Assert.Equal(
            typeof(int),
            boundBinaryExpressionNode.LeftBoundExpressionNode.ResultType);

        Assert.Equal(
            typeof(int),
            boundBinaryExpressionNode.BoundBinaryOperatorNode.ResultType);

        Assert.Equal(
            typeof(int),
            boundBinaryExpressionNode.RightBoundExpressionNode.ResultType);
    }

    [Fact]
    public void SHOULD_PARSE_LIBRARY_REFERENCE()
    {
        string sourceText = "#include <stdlib.h>"
            .ReplaceLineEndings("\n");

        var lexer = new Lexer(sourceText);

        lexer.Lex();

        var parser = new Parser(
            lexer.SyntaxTokens,
            sourceText);

        var compilationUnit = parser.Parse();

        Assert.Single(compilationUnit.Children);
        
        var libraryReferenceNode = compilationUnit.Children.Single();

        Assert.Equal(
            SyntaxKind.PreprocessorLibraryReferenceStatement,
            libraryReferenceNode.SyntaxKind);
    }

    [Fact]
    public void SHOULD_PARSE_TWO_LIBRARY_REFERENCES()
    {
        string sourceText = @"#include <stdlib.h>
#include <stdio.h>"
            .ReplaceLineEndings("\n");

        var lexer = new Lexer(sourceText);

        lexer.Lex();

        var parser = new Parser(
            lexer.SyntaxTokens,
            sourceText);

        var compilationUnit = parser.Parse();

        Assert.Equal(2, compilationUnit.Children.Length);

        var firstLibraryReferenceNode = compilationUnit.Children.First();

        Assert.Equal(
            SyntaxKind.PreprocessorLibraryReferenceStatement,
            firstLibraryReferenceNode.SyntaxKind);

        var secondLibraryReferenceNode = compilationUnit.Children.Last();

        Assert.Equal(
            SyntaxKind.PreprocessorLibraryReferenceStatement,
            secondLibraryReferenceNode.SyntaxKind);
    }
    
    [Fact]
    public void SHOULD_NOT_PARSE_COMMENT_SINGLE_LINE_STATEMENT()
    {
        string sourceText = @"// C:\Users\hunte\Repos\Aaa\"
            .ReplaceLineEndings("\n");

        var lexer = new Lexer(sourceText);

        lexer.Lex();

        var parser = new Parser(
            lexer.SyntaxTokens,
            sourceText);

        var compilationUnit = parser.Parse();

        Assert.Empty(compilationUnit.Children);
    }

    [Fact]
    public void SHOULD_PARSE_VARIABLE_DECLARATION_STATEMENT()
    {
        string sourceText = @"int x;"
            .ReplaceLineEndings("\n");

        var lexer = new Lexer(sourceText);

        lexer.Lex();

        var parser = new Parser(
            lexer.SyntaxTokens,
            sourceText);

        var compilationUnit = parser.Parse();

        Assert.Single(compilationUnit.Children);

        var boundVariableDeclarationStatementNode = 
            (BoundVariableDeclarationStatementNode)compilationUnit.Children
                .Single();

        Assert.Equal(
            SyntaxKind.BoundVariableDeclarationStatementNode,
            boundVariableDeclarationStatementNode.SyntaxKind);

        Assert.Equal(
            2,
            boundVariableDeclarationStatementNode.Children.Length);

        var boundTypeNode = (BoundTypeNode)boundVariableDeclarationStatementNode
            .Children[0];

        Assert.Equal(
            SyntaxKind.BoundTypeNode,
            boundTypeNode.SyntaxKind);
        
        Assert.Equal(
            typeof(int),
            boundTypeNode.Type);

        var identifierToken = boundVariableDeclarationStatementNode.Children[1];

        Assert.Equal(
            SyntaxKind.IdentifierToken,
            identifierToken.SyntaxKind);
    }

    [Fact]
    public void SHOULD_PARSE_VARIABLE_DECLARATION_STATEMENT_THEN_VARIABLE_ASSIGNMENT_STATEMENT()
    {
        string sourceText = @"int x;
x = 42;"
            .ReplaceLineEndings("\n");

        var lexer = new Lexer(sourceText);

        lexer.Lex();

        var parser = new Parser(
            lexer.SyntaxTokens,
            sourceText);

        var compilationUnit = parser.Parse();

        Assert.Equal(2, compilationUnit.Children.Length);

        var boundVariableDeclarationStatementNode = 
            (BoundVariableDeclarationStatementNode)compilationUnit.Children[0];

        Assert.Equal(
            SyntaxKind.BoundVariableDeclarationStatementNode,
            boundVariableDeclarationStatementNode.SyntaxKind);

        var boundVariableAssignmentStatementNode =
            (BoundVariableAssignmentStatementNode)compilationUnit.Children[1];

        Assert.Equal(
            SyntaxKind.BoundVariableAssignmentStatementNode,
            boundVariableAssignmentStatementNode.SyntaxKind);
    }

    [Fact]
    public void SHOULD_PARSE_COMPOUND_VARIABLE_DECLARATION_AND_ASSIGNMENT_STATEMENT()
    {
        string sourceText = @"int x = 42;"
            .ReplaceLineEndings("\n");

        var lexer = new Lexer(sourceText);

        lexer.Lex();

        var parser = new Parser(
            lexer.SyntaxTokens,
            sourceText);

        var compilationUnit = parser.Parse();

        Assert.Equal(2, compilationUnit.Children.Length);

        var boundVariableDeclarationStatementNode =
            (BoundVariableDeclarationStatementNode)compilationUnit.Children[0];

        Assert.Equal(
            SyntaxKind.BoundVariableDeclarationStatementNode,
            boundVariableDeclarationStatementNode.SyntaxKind);

        var boundVariableAssignmentStatementNode =
            (BoundVariableAssignmentStatementNode)compilationUnit.Children[1];

        Assert.Equal(
            SyntaxKind.BoundVariableAssignmentStatementNode,
            boundVariableAssignmentStatementNode.SyntaxKind);
    }

    [Fact]
    public void SHOULD_PARSE_FUNCTION_DEFINITION_STATEMENT()
    {
        string sourceText = @"void WriteHelloWorldToConsole()
{
}"
            .ReplaceLineEndings("\n");

        var lexer = new Lexer(sourceText);

        lexer.Lex();

        var parser = new Parser(
            lexer.SyntaxTokens,
            sourceText);

        var compilationUnit = parser.Parse();

        Assert.Single(compilationUnit.Children);

        var boundFunctionDeclarationNode = 
            (BoundFunctionDeclarationNode)compilationUnit.Children[0];

        Assert.Equal(
            SyntaxKind.BoundFunctionDeclarationNode,
            boundFunctionDeclarationNode.SyntaxKind);
    }

    [Fact]
    public void SHOULD_PARSE_FUNCTION_INVOCATION_STATEMENT()
    {
        string sourceText = @"void WriteHelloWorldToConsole()
{
}

WriteHelloWorldToConsole();"
            .ReplaceLineEndings("\n");

        var lexer = new Lexer(sourceText);

        lexer.Lex();

        var parser = new Parser(
            lexer.SyntaxTokens,
            sourceText);

        var compilationUnit = parser.Parse();

        Assert.Equal(2, compilationUnit.Children.Length);

        var boundFunctionDeclarationNode =
            (BoundFunctionDeclarationNode)compilationUnit.Children[0];

        Assert.Equal(
            SyntaxKind.BoundFunctionDeclarationNode,
            boundFunctionDeclarationNode.SyntaxKind);
        
        var boundFunctionInvocationNode =
            (BoundFunctionInvocationNode)compilationUnit.Children[1];

        Assert.Equal(
            SyntaxKind.BoundFunctionInvocationNode,
            boundFunctionInvocationNode.SyntaxKind);
    }
}