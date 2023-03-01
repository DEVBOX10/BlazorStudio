﻿using BlazorCommon.RazorLib.TreeView.TreeViewClasses;
using BlazorStudio.ClassLib.CommonComponents;
using BlazorStudio.ClassLib.TreeViewImplementations;
using Microsoft.AspNetCore.Components;

namespace BlazorStudio.RazorLib.TreeViewImplementations;

public partial class TreeViewAbsoluteFilePathDisplay 
    : ComponentBase, ITreeViewAbsoluteFilePathRendererType
{
    [CascadingParameter]
    public TreeViewState TreeViewState { get; set; } = null!;
    [CascadingParameter(Name="SearchQuery")]
    public string SearchQuery { get; set; } = string.Empty;
    [CascadingParameter(Name="SearchMatchTuples")]
    public List<(TreeViewStateKey treeViewStateKey, TreeViewAbsoluteFilePath treeViewAbsoluteFilePath)>? SearchMatchTuples { get; set; }
    
    [Parameter, EditorRequired]
    public TreeViewAbsoluteFilePath TreeViewAbsoluteFilePath { get; set; } = null!;
}