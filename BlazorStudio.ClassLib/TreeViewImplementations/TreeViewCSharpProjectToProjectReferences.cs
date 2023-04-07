﻿using BlazorCommon.RazorLib.TreeView.TreeViewClasses;
using BlazorStudio.ClassLib.ComponentRenderers;
using BlazorStudio.ClassLib.DotNet.CSharp;
using BlazorStudio.ClassLib.FileSystem.Classes.FilePath;
using BlazorStudio.ClassLib.FileSystem.Interfaces;
using BlazorStudio.ClassLib.Nuget;
using BlazorTextEditor.RazorLib.Analysis.Html.SyntaxActors;

namespace BlazorStudio.ClassLib.TreeViewImplementations;

public class TreeViewCSharpProjectToProjectReferences : TreeViewWithType<CSharpProjectToProjectReferences>
{
    public TreeViewCSharpProjectToProjectReferences(
        CSharpProjectToProjectReferences cSharpProjectToProjectReferences,
        IBlazorStudioComponentRenderers blazorStudioComponentRenderers,
        IFileSystemProvider fileSystemProvider,
        IEnvironmentProvider environmentProvider,
        bool isExpandable,
        bool isExpanded)
            : base(
                cSharpProjectToProjectReferences,
                isExpandable,
                isExpanded)
    {
        BlazorStudioComponentRenderers = blazorStudioComponentRenderers;
        FileSystemProvider = fileSystemProvider;
        EnvironmentProvider = environmentProvider;
    }
 
    public IBlazorStudioComponentRenderers BlazorStudioComponentRenderers { get; }
    public IFileSystemProvider FileSystemProvider { get; }
    public IEnvironmentProvider EnvironmentProvider { get; }

    public override bool Equals(object? obj)
    {
        if (obj is null ||
            obj is not TreeViewCSharpProjectToProjectReferences treeViewCSharpProjectToProjectReferences ||
            Item is null)
        {
            return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        return Item?
            .GetHashCode()
               ?? default;
    }

    public override TreeViewRenderer GetTreeViewRenderer()
    {
        return new TreeViewRenderer(
            BlazorStudioComponentRenderers.TreeViewCSharpProjectToProjectReferencesRendererType!,
            null);
    }
    
    public override async Task LoadChildrenAsync()
    {
        if (Item is null)
            return;

        var content = await FileSystemProvider.File.ReadAllTextAsync(
            Item.CSharpProjectNamespacePath.AbsoluteFilePath.GetAbsoluteFilePathString());
	    
        var htmlSyntaxUnit = HtmlSyntaxTree.ParseText(content);

        var syntaxNodeRoot = htmlSyntaxUnit.RootTagSyntax;

        var cSharpProjectSyntaxWalker = new CSharpProjectSyntaxWalker();

        cSharpProjectSyntaxWalker.Visit(syntaxNodeRoot);
	    
        var projectReferences = cSharpProjectSyntaxWalker.TagSyntaxes
            .Where(ts => (ts.OpenTagNameSyntax?.Value ?? string.Empty) == "ProjectReference")
            .ToList();

        List<CSharpProjectToProjectReference> cSharpProjectToProjectReferences = new();

        foreach (var projectReference in projectReferences)
        {
            var attributeNameValueTuples = projectReference
                .AttributeSyntaxes
                .Select(x => (
                    x.AttributeNameSyntax.TextEditorTextSpan
                        .GetText(content)
                        .Trim(),
                    x.AttributeValueSyntax.TextEditorTextSpan
                        .GetText(content)
                        .Replace("\"", string.Empty)
                        .Replace("=", string.Empty)
                        .Trim()))
                .ToArray();

            var includeAttribute = attributeNameValueTuples
                .FirstOrDefault(x => x.Item1 == "Include");

            var referencedProjectAbsoluteFilePathString = AbsoluteFilePath
                .JoinAnAbsoluteFilePathAndRelativeFilePath(
                    Item.CSharpProjectNamespacePath.AbsoluteFilePath,
                    includeAttribute.Item2,
                    EnvironmentProvider);

            var referencedProjectAbsoluteFilePath = new AbsoluteFilePath(
                referencedProjectAbsoluteFilePathString,
                false,
                EnvironmentProvider);
            
            var cSharpProjectToProjectReference = new CSharpProjectToProjectReference(
                referencedProjectAbsoluteFilePath);

            cSharpProjectToProjectReferences.Add(cSharpProjectToProjectReference);
        }

        var newChildren = cSharpProjectToProjectReferences
            .Select(x => (TreeViewNoType)new TreeViewCSharpProjectToProjectReference(
                x,
                BlazorStudioComponentRenderers,
                FileSystemProvider,
                EnvironmentProvider,
                false,
                false)
            {
                TreeViewChangedKey = TreeViewChangedKey.NewTreeViewChangedKey()
            })
            .ToList();
        
        for (int i = 0; i < newChildren.Count; i++)
        {
            var newChild = newChildren[i];
                
            newChild.IndexAmongSiblings = i;
            newChild.Parent = this;
            newChild.TreeViewChangedKey = TreeViewChangedKey.NewTreeViewChangedKey();
        }

        Children = newChildren;
        TreeViewChangedKey = TreeViewChangedKey.NewTreeViewChangedKey();
    }

    public override void RemoveRelatedFilesFromParent(List<TreeViewNoType> siblingsAndSelfTreeViews)
    {
        return;
    }
}