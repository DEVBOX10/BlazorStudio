﻿using BlazorCommon.RazorLib.TreeView.TreeViewClasses;
using BlazorStudio.ClassLib.ComponentRenderers;
using BlazorStudio.ClassLib.DotNet.CSharp;
using BlazorStudio.ClassLib.FileSystem.Interfaces;

namespace BlazorStudio.ClassLib.TreeViewImplementations;

public class TreeViewCSharpProjectDependencies : TreeViewWithType<CSharpProjectDependencies>
{
    public TreeViewCSharpProjectDependencies(
        CSharpProjectDependencies cSharpProjectDependencies,
        IBlazorStudioComponentRenderers blazorStudioComponentRenderers,
        IFileSystemProvider fileSystemProvider,
        IEnvironmentProvider environmentProvider,
        bool isExpandable,
        bool isExpanded)
            : base(
                cSharpProjectDependencies,
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
            obj is not TreeViewCSharpProjectDependencies treeViewCSharpProjectDependencies ||
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
            BlazorStudioComponentRenderers.TreeViewCSharpProjectDependenciesRendererType!,
            null);
    }
    
    public override async Task LoadChildrenAsync()
    {
        if (Item is null)
            return;
        
        var treeViewCSharpProjectNugetPackageReferences = new TreeViewCSharpProjectNugetPackageReferences(
            new CSharpProjectNugetPackageReferences(Item.CSharpProjectNamespacePath),
            BlazorStudioComponentRenderers,
            FileSystemProvider,
            EnvironmentProvider,
            true,
            false)
        {
            TreeViewChangedKey = TreeViewChangedKey.NewTreeViewChangedKey()
        };
        
        var treeViewCSharpProjectToProjectReferences = new TreeViewCSharpProjectToProjectReferences(
            new CSharpProjectToProjectReferences(),
            BlazorStudioComponentRenderers,
            FileSystemProvider,
            EnvironmentProvider,
            true,
            false)
        {
            TreeViewChangedKey = TreeViewChangedKey.NewTreeViewChangedKey()
        };

        var newChildren = new List<TreeViewNoType>
        {
            treeViewCSharpProjectNugetPackageReferences,
            treeViewCSharpProjectToProjectReferences
        };
        
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