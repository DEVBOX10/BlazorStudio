using System.Collections.Immutable;
using BlazorCommon.RazorLib.BackgroundTaskCase;
using BlazorCommon.RazorLib.ComponentRenderers;
using BlazorCommon.RazorLib.Store.ApplicationOptions;
using BlazorCommon.RazorLib.Store.DropdownCase;
using BlazorCommon.RazorLib.TreeView;
using BlazorCommon.RazorLib.TreeView.Commands;
using BlazorCommon.RazorLib.TreeView.TreeViewClasses;
using BlazorStudio.ClassLib.ComponentRenderers;
using BlazorStudio.ClassLib.Dimensions;
using BlazorStudio.ClassLib.DotNet;
using BlazorStudio.ClassLib.FileSystem.Classes.FilePath;
using BlazorStudio.ClassLib.FileSystem.Interfaces;
using BlazorStudio.ClassLib.Menu;
using BlazorStudio.ClassLib.Namespaces;
using BlazorStudio.ClassLib.Store.DotNetSolutionCase;
using BlazorStudio.ClassLib.Store.EditorCase;
using BlazorStudio.ClassLib.Store.FolderExplorerCase;
using BlazorStudio.ClassLib.Store.InputFileCase;
using BlazorStudio.ClassLib.Store.TerminalCase;
using BlazorStudio.ClassLib.TreeViewImplementations;
using BlazorStudio.RazorLib.TreeViewImplementations;
using BlazorTextEditor.RazorLib;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorStudio.RazorLib.SolutionExplorer;

public partial class SolutionExplorerDisplay : FluxorComponent
{
    [Inject]
    private IState<TerminalSessionsState> TerminalSessionsStateWrap { get; set; } = null!;
    [Inject]
    private IState<AppOptionsState> AppOptionsStateWrap { get; set; } = null!;
    [Inject]
    private IState<DotNetSolutionState> DotNetSolutionStateWrap { get; set; } = null!;
    [Inject]
    private IDispatcher Dispatcher { get; set; } = null!;
    [Inject]
    private ITextEditorService TextEditorService { get; set; } = null!;
    [Inject]
    private ITreeViewService TreeViewService { get; set; } = null!;
    [Inject]
    private IBlazorStudioComponentRenderers BlazorStudioComponentRenderers { get; set; } = null!;
    [Inject]
    private ClassLib.Menu.ICommonMenuOptionsFactory CommonMenuOptionsFactory { get; set; } = null!;
    [Inject]
    private IFileSystemProvider FileSystemProvider { get; set; } = null!;
    [Inject]
    private IEnvironmentProvider EnvironmentProvider { get; set; } = null!;
    [Inject]
    private IBackgroundTaskQueue BackgroundTaskQueue { get; set; } = null!;
    
    private const string SOLUTION_EXPLORER_ABSOLUTE_PATH_STRING = @"C:\Users\hunte\Repos\TestSolutionParser\TestSolutionParser.sln";
    
    public static readonly TreeViewStateKey TreeViewSolutionExplorerStateKey = 
        TreeViewStateKey.NewTreeViewStateKey();
    
    private string _filePath = string.Empty;
    private ITreeViewCommandParameter? _mostRecentTreeViewCommandParameter;
    private SolutionExplorerTreeViewKeymap _solutionExplorerTreeViewKeymap = null!;
    private SolutionExplorerTreeViewMouseEventHandler _solutionExplorerTreeViewMouseEventHandler = null!;
    private bool _disposed;

    private int OffsetPerDepthInPixels => (int)Math.Ceiling(
        AppOptionsStateWrap.Value.Options.IconSizeInPixels.GetValueOrDefault() *
        (2.0/3.0));

    protected override void OnInitialized()
    {
        DotNetSolutionStateWrap.StateChanged += DotNetSolutionStateWrapOnStateChanged;
        
        _solutionExplorerTreeViewKeymap = new SolutionExplorerTreeViewKeymap(
            TerminalSessionsStateWrap,
            CommonMenuOptionsFactory,
            BlazorStudioComponentRenderers,
            FileSystemProvider,
            Dispatcher,
            TreeViewService,
            TextEditorService,
            BackgroundTaskQueue);
        
        _solutionExplorerTreeViewMouseEventHandler = 
            new SolutionExplorerTreeViewMouseEventHandler(
                Dispatcher,
                TextEditorService,
                BlazorStudioComponentRenderers,
                FileSystemProvider,
                TreeViewService,
                BackgroundTaskQueue);
    
        base.OnInitialized();
    }

    private async void DotNetSolutionStateWrapOnStateChanged(object? sender, EventArgs e)
    {
        var dotNetSolutionState = DotNetSolutionStateWrap.Value;
        
        if (dotNetSolutionState.DotNetSolution is not null)
        {
            await SetSolutionExplorerTreeViewRootAsync(
                dotNetSolutionState.DotNetSolution);
        }

        await InvokeAsync(StateHasChanged);
    }

    private async Task OnTreeViewContextMenuFunc(ITreeViewCommandParameter treeViewCommandParameter)
    {
        _mostRecentTreeViewCommandParameter = treeViewCommandParameter;
        
        Dispatcher.Dispatch(
            new DropdownsState.AddActiveAction(
                SolutionExplorerContextMenu.ContextMenuEventDropdownKey));
        
        await InvokeAsync(StateHasChanged);
    }
    
    private async Task SetSolutionExplorerTreeViewRootAsync(DotNetSolution dotNetSolution)
    {
        var rootNode = new TreeViewSolution(
            dotNetSolution,
            BlazorStudioComponentRenderers,
            FileSystemProvider,
            EnvironmentProvider,
            true,
            true);

        await rootNode.LoadChildrenAsync();

        if (!TreeViewService.TryGetTreeViewState(
                TreeViewSolutionExplorerStateKey,
                out _))
        {
            TreeViewService.RegisterTreeViewState(new TreeViewState(
                TreeViewSolutionExplorerStateKey,
                rootNode,
                rootNode,
                ImmutableList<TreeViewNoType>.Empty));
        }
        else
        {
            TreeViewService.SetRoot(
                TreeViewSolutionExplorerStateKey,
                rootNode);

            TreeViewService.SetActiveNode(
                TreeViewSolutionExplorerStateKey,
                rootNode);
        }
    }

    private async Task SetSolutionExplorerOnClick(
        string localSolutionExplorerAbsolutePathString)
    {
        await DotNetSolutionState.SetActiveSolutionAsync(
            localSolutionExplorerAbsolutePathString,
            FileSystemProvider,
            EnvironmentProvider,
            Dispatcher);
        
        var content = await FileSystemProvider.File.ReadAllTextAsync(
            localSolutionExplorerAbsolutePathString,
            CancellationToken.None);

        var solutionAbsoluteFilePath = new AbsoluteFilePath(
            localSolutionExplorerAbsolutePathString,
            false,
            EnvironmentProvider);
        
        var solutionNamespacePath = new NamespacePath(
            string.Empty,
            solutionAbsoluteFilePath);

        var dotNetSolution = DotNetSolutionParser.Parse(
            content,
            solutionNamespacePath,
            EnvironmentProvider);
        
        Dispatcher.Dispatch(
            new DotNetSolutionState.WithAction(
                inDotNetSolutionState => inDotNetSolutionState with
                {
                    DotNetSolution = dotNetSolution
                }));
    }
    
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
                DotNetSolutionStateWrap.StateChanged -= DotNetSolutionStateWrapOnStateChanged;
            }
        }
        
        base.Dispose(true);
    }
}