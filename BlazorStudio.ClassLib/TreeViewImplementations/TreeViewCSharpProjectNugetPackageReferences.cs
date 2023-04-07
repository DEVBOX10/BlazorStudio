﻿using BlazorCommon.RazorLib.TreeView.TreeViewClasses;
using BlazorStudio.ClassLib.ComponentRenderers;
using BlazorStudio.ClassLib.DotNet.CSharp;
using BlazorStudio.ClassLib.FileSystem.Interfaces;
using BlazorStudio.ClassLib.Nuget;
using BlazorTextEditor.RazorLib.Analysis.Html.SyntaxActors;

namespace BlazorStudio.ClassLib.TreeViewImplementations;

public class TreeViewCSharpProjectNugetPackageReferences : TreeViewWithType<CSharpProjectNugetPackageReferences>
{
    public TreeViewCSharpProjectNugetPackageReferences(
        CSharpProjectNugetPackageReferences cSharpProjectNugetPackageReferences,
        IBlazorStudioComponentRenderers blazorStudioComponentRenderers,
        IFileSystemProvider fileSystemProvider,
        IEnvironmentProvider environmentProvider,
        bool isExpandable,
        bool isExpanded)
            : base(
                cSharpProjectNugetPackageReferences,
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
            obj is not TreeViewCSharpProjectNugetPackageReferences treeViewCSharpProjectNugetPackageReferences ||
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
            BlazorStudioComponentRenderers.TreeViewCSharpProjectNugetPackageReferencesRendererType!,
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
	    
        var packageReferences = cSharpProjectSyntaxWalker.TagSyntaxes
            .Where(ts => (ts.OpenTagNameSyntax?.Value ?? string.Empty) == "PackageReference")
            .ToList();

        List<LightWeightNugetPackageRecord> lightWeightNugetPackageRecords = new();

        foreach (var packageReference in packageReferences)
        {
            var attributeNameValueTuples = packageReference
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

            var versionAttribute = attributeNameValueTuples
                .FirstOrDefault(x => x.Item1 == "Version");

            var lightWeightNugetPackageRecord = new LightWeightNugetPackageRecord(
                includeAttribute.Item2,
                versionAttribute.Item2);

            lightWeightNugetPackageRecords.Add(lightWeightNugetPackageRecord);
        }

        var newChildren = lightWeightNugetPackageRecords
            .Select(x => (TreeViewNoType)new TreeViewLightWeightNugetPackageRecord(
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