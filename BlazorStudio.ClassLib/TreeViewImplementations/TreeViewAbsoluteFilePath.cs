﻿using BlazorCommon.RazorLib.TreeView.TreeViewClasses;
using BlazorCommon.RazorLib.WatchWindow.TreeViewClasses;
using BlazorStudio.ClassLib.ComponentRenderers;
using BlazorStudio.ClassLib.ComponentRenderers.Types;
using BlazorStudio.ClassLib.FileSystem.Interfaces;
using BlazorStudio.ClassLib.TreeViewImplementations.Helper;

namespace BlazorStudio.ClassLib.TreeViewImplementations;

public class TreeViewAbsoluteFilePath : TreeViewWithType<IAbsoluteFilePath>
{
    public TreeViewAbsoluteFilePath(
        IAbsoluteFilePath absoluteFilePath,
        IBlazorStudioComponentRenderers blazorStudioComponentRenderers,
        IFileSystemProvider fileSystemProvider,
        IEnvironmentProvider environmentProvider,
        bool isExpandable,
        bool isExpanded)
            : base(
                absoluteFilePath,
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
            obj is not TreeViewNamespacePath treeViewSolutionExplorer ||
            treeViewSolutionExplorer.Item is null ||
            Item is null)
        {
            return false;
        }

        return treeViewSolutionExplorer.Item.AbsoluteFilePath
                   .GetAbsoluteFilePathString() == 
               Item.GetAbsoluteFilePathString();
    }

    public override int GetHashCode()
    {
        return Item?.GetAbsoluteFilePathString().GetHashCode() ?? default;
    }

    public override TreeViewRenderer GetTreeViewRenderer()
    {
        return new TreeViewRenderer(
            BlazorStudioComponentRenderers.TreeViewAbsoluteFilePathRendererType!,
            new Dictionary<string, object?>
            {
                {
                    nameof(ITreeViewAbsoluteFilePathRendererType.TreeViewAbsoluteFilePath),
                    this
                },
            });
    }
    
    public override async Task LoadChildrenAsync()
    {
        if (Item is null)
            return;

        try
        {
            var newChildren = new List<TreeViewNoType>();
            
            if (Item.IsDirectory)
            {
                newChildren = await TreeViewHelper
                    .LoadChildrenForDirectoryAsync(this);
            }
        
            var oldChildrenMap = Children
                .ToDictionary(child => child);
        
            foreach (var newChild in newChildren)
            {
                if (oldChildrenMap.TryGetValue(newChild, out var oldChild))
                {
                    newChild.IsExpanded = oldChild.IsExpanded;
                    newChild.IsExpandable = oldChild.IsExpandable;
                    newChild.IsHidden = oldChild.IsHidden;
                    newChild.TreeViewNodeKey = oldChild.TreeViewNodeKey;
                    newChild.Children = oldChild.Children;
                }
            }
            
            for (int i = 0; i < newChildren.Count; i++)
            {
                var newChild = newChildren[i];
                
                newChild.IndexAmongSiblings = i;
                newChild.Parent = this;
                newChild.TreeViewChangedKey = TreeViewChangedKey.NewTreeViewChangedKey();
            }
            
            Children = newChildren;
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

    public override void RemoveRelatedFilesFromParent(List<TreeViewNoType> siblingsAndSelfTreeViews)
    {
        // This method is meant to do nothing in this case.
    }
}