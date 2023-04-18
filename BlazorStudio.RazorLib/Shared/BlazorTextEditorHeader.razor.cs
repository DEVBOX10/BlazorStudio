using System.Collections.Immutable;
using BlazorCommon.RazorLib.BackgroundTaskCase;
using BlazorCommon.RazorLib.ComponentRenderers;
using BlazorCommon.RazorLib.Dialog;
using BlazorCommon.RazorLib.Dropdown;
using BlazorCommon.RazorLib.Menu;
using BlazorCommon.RazorLib.Store.AccountCase;
using BlazorCommon.RazorLib.Store.DialogCase;
using BlazorCommon.RazorLib.Store.DropdownCase;
using BlazorStudio.ClassLib.ComponentRenderers;
using BlazorStudio.ClassLib.FileSystem.Interfaces;
using BlazorStudio.ClassLib.Store.DotNetSolutionCase;
using BlazorStudio.ClassLib.Store.EditorCase;
using BlazorStudio.ClassLib.Store.FolderExplorerCase;
using BlazorStudio.RazorLib.Account;
using BlazorStudio.RazorLib.Button;
using BlazorStudio.RazorLib.DotNetSolutionForm;
using BlazorTextEditor.RazorLib;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using Microsoft.AspNetCore.Components;

namespace BlazorStudio.RazorLib.Shared;

public partial class BlazorTextEditorHeader : FluxorComponent
{
    [Inject]
    private IState<AccountState> AccountStateWrap { get; set; } = null!;
    [Inject]
    private IDispatcher Dispatcher { get; set; } = null!;
    [Inject]
    private ITextEditorService TextEditorService { get; set; } = null!;
    [Inject]
    private IBlazorStudioComponentRenderers BlazorStudioComponentRenderers { get; set; } = null!;
    [Inject]
    private IFileSystemProvider FileSystemProvider { get; set; } = null!;
    [Inject]
    private IEnvironmentProvider EnvironmentProvider { get; set; } = null!;
    [Inject]
    private IBackgroundTaskQueue BackgroundTaskQueue { get; set; } = null!;

    [Parameter, EditorRequired]
    public Type LoginDisplayComponentType { get; set; } = typeof(LoginFormDisplay);
    
    private DropdownKey _dropdownKeyFile = DropdownKey.NewDropdownKey();
    private MenuRecord _menuFile = new(ImmutableArray<MenuOptionRecord>.Empty);
    private ButtonDisplay? _buttonDisplayComponentFile;
    
    private DropdownKey _dropdownKeyAccount = DropdownKey.NewDropdownKey();
    private MenuRecord _menuAccount = new(ImmutableArray<MenuOptionRecord>.Empty);
    private ButtonDisplay? _buttonDisplayComponentAccount;

    protected override Task OnInitializedAsync()
    {
        InitializeMenuFile();
        InitializeMenuAccount();
        
        return base.OnInitializedAsync();
    }

    private void InitializeMenuFile()
    {
        var menuOptions = new List<MenuOptionRecord>();

        // Menu Option New
        {
            var menuOptionNewDotNetSolution = new MenuOptionRecord(
                ".NET Solution",
                MenuOptionKind.Other,
                OpenNewDotNetSolutionDialog);

            var menuOptionNew = new MenuOptionRecord(
                "New",
                MenuOptionKind.Other,
                SubMenu: new MenuRecord(
                    new[]
                    {
                        menuOptionNewDotNetSolution
                    }.ToImmutableArray()));
            
            menuOptions.Add(menuOptionNew);
        }
        
        // Menu Option Open
        {
            // TODO: Why do all the OnClicks have an async void lambda? Not quite sure what I was doing when I originally wrote this and should revisit this.
            
            var menuOptionOpenFile = new MenuOptionRecord(
                "File",
                MenuOptionKind.Other,
                async () => 
                    await EditorState.ShowInputFileAsync(
                        Dispatcher, 
                        TextEditorService,
                        BlazorStudioComponentRenderers,
                        FileSystemProvider,
                        BackgroundTaskQueue));
        
            var menuOptionOpenDirectory = new MenuOptionRecord(
                "Directory",
                MenuOptionKind.Other,
                async () => 
                    await FolderExplorerState.ShowInputFileAsync(Dispatcher));
        
            var menuOptionOpenCSharpProject = new MenuOptionRecord(
                "C# Project - TODO: Adhoc Sln",
                MenuOptionKind.Other,
                async () => 
                    await EditorState.ShowInputFileAsync(
                        Dispatcher, 
                        TextEditorService,
                        BlazorStudioComponentRenderers,
                        FileSystemProvider,
                        BackgroundTaskQueue));
            
            var menuOptionOpenDotNetSolution = new MenuOptionRecord(
                ".NET Solution",
                MenuOptionKind.Other,
                async () => 
                    await DotNetSolutionState.ShowInputFileAsync(
                        Dispatcher,
                        BlazorStudioComponentRenderers,
                        FileSystemProvider,
                        EnvironmentProvider));
        
            var menuOptionOpen = new MenuOptionRecord(
                "Open",
                MenuOptionKind.Other,
                SubMenu: new MenuRecord(
                    new []
                    {
                        menuOptionOpenFile,
                        menuOptionOpenDirectory,
                        menuOptionOpenCSharpProject,
                        menuOptionOpenDotNetSolution
                    }.ToImmutableArray()));
        
            menuOptions.Add(menuOptionOpen);
        }
        
        _menuFile = new MenuRecord(menuOptions.ToImmutableArray());
    }
    
    private void InitializeMenuAccount()
    {
        var menuOptions = new List<MenuOptionRecord>();

        // Menu Option Login
        {
            var menuOptionLogin = new MenuOptionRecord(
                "Login",
                MenuOptionKind.Other,
                WidgetRendererType: typeof(LoginFormDisplay));
            
            menuOptions.Add(menuOptionLogin);
        }
        
        _menuAccount = new MenuRecord(menuOptions.ToImmutableArray());
    }

    private void AddActiveDropdownKey(DropdownKey dropdownKey)
    {
        Dispatcher.Dispatch(
            new DropdownsState.AddActiveAction(
                dropdownKey));
    }
    
    /// <summary>
    /// TODO: Make this method abstracted into a component that takes care of the UI to show the dropdown and to restore focus when menu closed
    /// </summary>
    private async Task RestoreFocusToButtonDisplayComponentFileAsync()
    {
        try
        {           
            if (_buttonDisplayComponentFile?.ButtonElementReference is not null)
            {
                await _buttonDisplayComponentFile.ButtonElementReference.Value
                    .FocusAsync();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    /// <summary>
    /// TODO: Make this method abstracted into a component that takes care of the UI to show the dropdown and to restore focus when menu closed
    /// </summary>
    private async Task RestoreFocusToButtonDisplayComponentAccount()
    {
        try
        {           
            if (_buttonDisplayComponentAccount?.ButtonElementReference is not null)
            {
                await _buttonDisplayComponentAccount.ButtonElementReference.Value
                    .FocusAsync();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private void OpenNewDotNetSolutionDialog()
    {
        var dialogRecord = new DialogRecord(
            DialogKey.NewDialogKey(), 
            "New .NET Solution",
            typeof(DotNetSolutionFormDisplay),
            null,
            null)
        {
            IsResizable = true
        };
        
        Dispatcher.Dispatch(
            new DialogRecordsCollection.RegisterAction(
                dialogRecord));
    }
}