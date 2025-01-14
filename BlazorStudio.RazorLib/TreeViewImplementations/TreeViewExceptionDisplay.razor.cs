﻿using BlazorCommon.RazorLib.ComponentRenderers.Types;
using BlazorCommon.RazorLib.WatchWindow.TreeViewClasses;
using Microsoft.AspNetCore.Components;

namespace BlazorStudio.RazorLib.TreeViewImplementations;

public partial class TreeViewExceptionDisplay 
    : ComponentBase, ITreeViewExceptionRendererType
{
    [Parameter, EditorRequired]
    public TreeViewException TreeViewException { get; set; } = null!;
}