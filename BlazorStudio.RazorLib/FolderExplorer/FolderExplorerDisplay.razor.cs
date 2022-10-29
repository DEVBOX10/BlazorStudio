using System.Collections.Immutable;
using BlazorStudio.ClassLib.Dimensions;
using BlazorStudio.ClassLib.FileSystem.Classes;
using BlazorStudio.ClassLib.FileSystem.Interfaces;
using BlazorStudio.ClassLib.Menu;
using BlazorStudio.ClassLib.Store.FolderExplorerCase;
using BlazorStudio.ClassLib.TreeView;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorStudio.RazorLib.FolderExplorer;

public partial class FolderExplorerDisplay : FluxorComponent
{
    [Inject]
    private IState<FolderExplorerState> FolderExplorerStateWrap { get; set; } = null!;
    [Inject]
    private IDispatcher Dispatcher { get; set; } = null!;
    
    [Parameter, EditorRequired]
    public ElementDimensions FolderExplorerElementDimensions { get; set; } = null!;

    private string _filePath = string.Empty;
    private TreeViewModel<IAbsoluteFilePath>? _rootTreeViewModel;

    protected override void OnInitialized()
    {
        FolderExplorerStateWrap.StateChanged += FolderExplorerStateWrapOnStateChanged;
        
        base.OnInitialized();
    }

    private void FolderExplorerStateWrapOnStateChanged(object? sender, EventArgs e)
    {
        if (FolderExplorerStateWrap.Value.AbsoluteFilePath is null)
            return;

        _rootTreeViewModel = new TreeViewModel<IAbsoluteFilePath>(
            FolderExplorerStateWrap.Value.AbsoluteFilePath,
            true,
            LoadChildrenAsync);

        _rootTreeViewModel.LoadChildrenFuncAsync.Invoke(_rootTreeViewModel);
    }
    
    private Task LoadChildrenAsync(TreeViewModel<IAbsoluteFilePath> treeViewModel)
    {
        var absoluteFilePathString = treeViewModel.Item.GetAbsoluteFilePathString();

        var childFiles = Directory
            .GetFiles(absoluteFilePathString)
            .OrderBy(filename => filename)
            .Select(cf => new AbsoluteFilePath(cf, false));
        
        var childDirectories = Directory
            .GetDirectories(absoluteFilePathString)
            .OrderBy(filename => filename)
            .Select(cd => new AbsoluteFilePath(cd, true));

        var childTreeViewModels = childDirectories
            .Union(childFiles)
            .Select(afp => 
                new TreeViewModel<IAbsoluteFilePath>(
                    afp, 
                    true, 
                    LoadChildrenAsync));

        treeViewModel.Children.Clear();
        treeViewModel.Children.AddRange(childTreeViewModels);
        
        return Task.CompletedTask;
    }
    
    private void DispatchSetFolderExplorerStateOnClick()
    {
        if (!Directory.Exists(_filePath))
            throw new DirectoryNotFoundException();
        
        Dispatcher.Dispatch(
            new SetFolderExplorerStateAction(
                new AbsoluteFilePath(_filePath, true)));
    }
    
    private MenuRecord GetContextMenu(TreeViewModel<IAbsoluteFilePath> treeViewModel)
    {
        var openInTextEditorMenuOption = new MenuOptionRecord(
            "Nothing here TODO: Aaa",
            () => { });

        return new MenuRecord(new []
        {
            openInTextEditorMenuOption
        }.ToImmutableArray());
    }

    private string GetStyleForContextMenu(MouseEventArgs? mouseEventArgs)
    {
        if (mouseEventArgs is null)
            return string.Empty;

        return 
            $"position: fixed; left: {mouseEventArgs.ClientX}px; top: {mouseEventArgs.ClientY}px;";
    }

    private Task OpenFolderOnClick()
    {
        throw new NotImplementedException();
    }
    
    public void Dispose()
    {
        FolderExplorerStateWrap.StateChanged -= FolderExplorerStateWrapOnStateChanged;
    }
}