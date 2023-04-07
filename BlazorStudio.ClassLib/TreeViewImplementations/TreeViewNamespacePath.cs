﻿using BlazorCommon.RazorLib.TreeView.TreeViewClasses;
using BlazorCommon.RazorLib.WatchWindow.TreeViewImplementations;
using BlazorStudio.ClassLib.ComponentRenderers;
using BlazorStudio.ClassLib.ComponentRenderers.Types;
using BlazorStudio.ClassLib.FileConstants;
using BlazorStudio.ClassLib.FileSystem.Interfaces;
using BlazorStudio.ClassLib.Namespaces;
using BlazorStudio.ClassLib.TreeViewImplementations.Helper;

namespace BlazorStudio.ClassLib.TreeViewImplementations;

public class TreeViewNamespacePath : TreeViewWithType<NamespacePath>
{
    public TreeViewNamespacePath(
        NamespacePath namespacePath,
        IBlazorStudioComponentRenderers blazorStudioComponentRenderers,
        IFileSystemProvider fileSystemProvider,
        IEnvironmentProvider environmentProvider,
        bool isExpandable,
        bool isExpanded)
            : base(
                namespacePath,
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
               Item.AbsoluteFilePath.GetAbsoluteFilePathString();
    }

    public override int GetHashCode()
    {
        return Item?.AbsoluteFilePath.GetAbsoluteFilePathString().GetHashCode() ?? default;
    }

    public override TreeViewRenderer GetTreeViewRenderer()
    {
        return new TreeViewRenderer(
            BlazorStudioComponentRenderers.TreeViewNamespacePathRendererType!,
            new Dictionary<string, object?>
            {
                {
                    nameof(ITreeViewNamespacePathRendererType.NamespacePath),
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
            var newChildren = new List<TreeViewNoType>();
            
            if (Item.AbsoluteFilePath.IsDirectory)
            {
                newChildren = await TreeViewHelper
                    .LoadChildrenForDirectoryAsync(this);
            }
            else
            {
                switch (Item.AbsoluteFilePath.ExtensionNoPeriod)
                {
                    case ExtensionNoPeriodFacts.DOT_NET_SOLUTION:
                        return;
                    case ExtensionNoPeriodFacts.C_SHARP_PROJECT:
                        newChildren = await TreeViewHelper
                            .LoadChildrenForCSharpProjectAsync(this);
                        break;
                    case ExtensionNoPeriodFacts.RAZOR_MARKUP:
                        newChildren = await TreeViewHelper
                            .LoadChildrenForRazorMarkupAsync(this);
                        break;
                }
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

    /// <summary>
    /// This method is called on each child when loading children for a parent node.
    /// This method allows for code-behinds
    /// </summary>
    public override void RemoveRelatedFilesFromParent(List<TreeViewNoType> siblingsAndSelfTreeViews)
    {
        if (Item is null)
            return;
        
        if (Item.AbsoluteFilePath.ExtensionNoPeriod
            .EndsWith(ExtensionNoPeriodFacts.RAZOR_MARKUP))
        {
            TreeViewHelper.RazorMarkupFindRelatedFiles(
                this, 
                siblingsAndSelfTreeViews);
        }
    }
}