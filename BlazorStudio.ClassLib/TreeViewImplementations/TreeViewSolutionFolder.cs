﻿using BlazorCommon.RazorLib.TreeView.TreeViewClasses;
using BlazorCommon.RazorLib.WatchWindow.TreeViewImplementations;
using BlazorStudio.ClassLib.ComponentRenderers;
using BlazorStudio.ClassLib.ComponentRenderers.Types;
using BlazorStudio.ClassLib.DotNet;
using BlazorStudio.ClassLib.DotNet.CSharp;
using BlazorStudio.ClassLib.FileSystem.Interfaces;
using BlazorStudio.ClassLib.Namespaces;
using BlazorStudio.ClassLib.TreeViewImplementations.Helper;

namespace BlazorStudio.ClassLib.TreeViewImplementations;

public class TreeViewSolutionFolder : TreeViewWithType<DotNetSolutionFolder>
{
    public TreeViewSolutionFolder(
        DotNetSolutionFolder dotNetSolutionFolder,
        IBlazorStudioComponentRenderers blazorStudioComponentRenderers,
        IFileSystemProvider fileSystemProvider,
        IEnvironmentProvider environmentProvider,
        bool isExpandable,
        bool isExpanded)
            : base(
                dotNetSolutionFolder,
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
            obj is not TreeViewSolutionFolder treeViewSolutionFolder ||
            treeViewSolutionFolder.Item is null ||
            Item is null)
        {
            return false;
        }

        return treeViewSolutionFolder.Item.AbsoluteFilePath.GetAbsoluteFilePathString() ==
               Item.AbsoluteFilePath.GetAbsoluteFilePathString();
    }

    public override int GetHashCode()
    {
        return Item?.AbsoluteFilePath
            .GetAbsoluteFilePathString()
            .GetHashCode()
               ?? default;
    }

    public override TreeViewRenderer GetTreeViewRenderer()
    {
        return new TreeViewRenderer(
            BlazorStudioComponentRenderers.TreeViewSolutionFolderRendererType!,
            new Dictionary<string, object?>
            {
                {
                    nameof(ITreeViewSolutionFolderRendererType.DotNetSolutionFolder),
                    Item
                },
            });
    }
    
    public override async Task LoadChildrenAsync()
    {
        if (Item is null)
            return;

        try
        {
        }
        catch (Exception exception)
        {
            Children = new List<TreeViewNoType>
            {
                new TreeViewException(
                    exception,
                    false,
                    false,
                    BlazorStudioComponentRenderers.BlazorCommonComponentRenderers.WatchWindowTreeViewRenderers)
                {
                    Parent = this,
                    IndexAmongSiblings = 0,
                }
            };
        }
        
        TreeViewChangedKey = TreeViewChangedKey.NewTreeViewChangedKey();
    }

    public override void RemoveRelatedFilesFromParent(
        List<TreeViewNoType> siblingsAndSelfTreeViews)
    {
        var ancestorNode = Parent;

        if (ancestorNode is not TreeViewSolution)
        {
            if (ancestorNode.Parent is null)
                return;
            
            while (ancestorNode is not TreeViewSolution &&
                   ancestorNode.Parent is not null)
            {
                ancestorNode = ancestorNode.Parent;
            }

            if (ancestorNode is not TreeViewSolution)
                return;
        }
        
        if (ancestorNode is TreeViewSolution treeViewSolution)
        {
            var nestedProjectEntries = treeViewSolution
                .Item.DotNetSolutionGlobalSection.GlobalSectionNestedProjects.NestedProjectEntries
                .Where(x => x.SolutionFolderIdGuid == Item.ProjectIdGuid)
                .ToArray();

            var childProjectIds = nestedProjectEntries
                .Select(x => x.ChildProjectIdGuid)
                .ToArray();

            var childProjects =
                treeViewSolution.Item.DotNetProjects
                    .Where(x => childProjectIds.Contains(x.ProjectIdGuid))
                    .ToArray();

            var childTreeViews = childProjects
                .Select(x =>
                {
                    if (x.DotNetProjectKind == DotNetProjectKind.SolutionFolder)
                    {
                        return ConstructTreeViewSolutionFolder((DotNetSolutionFolder)x);
                    }
                    else
                    {
                        return ConstructTreeViewCSharpProject((CSharpProject)x);
                    }
                }).ToList();
            
            for (int siblingsIndex = siblingsAndSelfTreeViews.Count - 1; siblingsIndex >= 0; siblingsIndex--)
            {
                var siblingOrSelf = siblingsAndSelfTreeViews[siblingsIndex];

                for (var childrensIndex = 0; childrensIndex < childTreeViews.Count; childrensIndex++)
                {
                    var childTreeView = childTreeViews[childrensIndex];
                    
                    if (siblingOrSelf.Equals(childTreeView))
                    {
                        // What i'm doing here is super confusing and needs changed.
                        // In lines above I re-created a TreeView node for a second time.
                        //
                        // Now I have to figure out where that re-created TreeView node
                        // existed originally because it will have its
                        // "RemoveRelatedFilesFromParent" invoked.
                        //
                        // Without this logic a:
                        //     solution folder -> solution folder -> project
                        //
                        // Will not render the project.
                        //
                        // TODO: Revisit this logic.
                        var originalTreeView = siblingsAndSelfTreeViews[siblingsIndex];
                        
                        originalTreeView.Parent = this;
                        originalTreeView.IndexAmongSiblings = childrensIndex;
                        originalTreeView.TreeViewChangedKey = TreeViewChangedKey.NewTreeViewChangedKey();
                        
                        siblingsAndSelfTreeViews.RemoveAt(siblingsIndex);

                        childTreeViews[childrensIndex] = originalTreeView;
                    }
                    else
                    {
                        childTreeView.Parent = this;
                        childTreeView.IndexAmongSiblings = childrensIndex;
                        childTreeView.TreeViewChangedKey = TreeViewChangedKey.NewTreeViewChangedKey();
                    }
                }
            }

            Children = childTreeViews;
        }
        
        return;
    }

    private TreeViewNoType ConstructTreeViewSolutionFolder(
        DotNetSolutionFolder dotNetSolutionFolder)
    {
        return (TreeViewNoType)new TreeViewSolutionFolder(
            dotNetSolutionFolder,
            BlazorStudioComponentRenderers,
            FileSystemProvider,
            EnvironmentProvider,
            true,
            false)
        {
            TreeViewChangedKey = TreeViewChangedKey.NewTreeViewChangedKey()
        };
    }
    
    private TreeViewNoType ConstructTreeViewCSharpProject(
        CSharpProject cSharpProject)
    {
        var namespacePath = new NamespacePath(
            cSharpProject.AbsoluteFilePath.FileNameNoExtension,
            cSharpProject.AbsoluteFilePath);
        
        return (TreeViewNoType)new TreeViewNamespacePath(
            namespacePath,
            BlazorStudioComponentRenderers,
            FileSystemProvider,
            EnvironmentProvider,
            true,
            false)
        {
            TreeViewChangedKey = TreeViewChangedKey.NewTreeViewChangedKey()
        };
    }
}