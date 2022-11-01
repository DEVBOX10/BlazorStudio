using System.Collections.Immutable;
using BlazorStudio.ClassLib.Menu;
using BlazorStudio.ClassLib.Store.DialogCase;
using BlazorStudio.ClassLib.Store.DropdownCase;
using BlazorStudio.ClassLib.Store.EditorCase;
using BlazorStudio.ClassLib.Store.FolderExplorerCase;
using BlazorStudio.ClassLib.Store.InputFileCase;
using BlazorStudio.ClassLib.Store.NotificationCase;
using BlazorStudio.ClassLib.Store.SolutionExplorer;
using BlazorStudio.ClassLib.Store.TextEditorResourceMapCase;
using BlazorStudio.RazorLib.Button;
using BlazorStudio.RazorLib.DotNetSolutionForm;
using BlazorStudio.RazorLib.InputFile;
using BlazorStudio.RazorLib.Notifications;
using BlazorTextEditor.RazorLib;
using Fluxor;
using Microsoft.AspNetCore.Components;

namespace BlazorStudio.RazorLib.Shared;

public partial class BlazorTextEditorHeader : ComponentBase
{
    [Inject]
    private IState<TextEditorResourceMapState> TextEditorResourceMapStateWrap { get; set; } = null!;
    [Inject]
    private IDispatcher Dispatcher { get; set; } = null!;
    [Inject]
    private ITextEditorService TextEditorService { get; set; } = null!;
    
    private DropdownKey _dropdownKeyFileDropdown = DropdownKey.NewDropdownKey();
    private MenuRecord _fileMenu = new(ImmutableArray<MenuOptionRecord>.Empty);
    private ButtonDisplay? _fileButtonDisplay;

    protected override Task OnInitializedAsync()
    {
        _fileMenu = new MenuRecord(
            new []
            {
                GetMenuOptionNew(),
                GetMenuOptionOpen()
            }.ToImmutableArray());
        
        return base.OnInitializedAsync();
    }

    private MenuOptionRecord GetMenuOptionNew()
    {
        var newDotNetSolution = new MenuOptionRecord(
            ".NET Solution",
            OpenNewDotNetSolutionDialog);
        
        return new MenuOptionRecord(
            "New",
            SubMenu: new MenuRecord(
                new []
                {
                    newDotNetSolution
                }.ToImmutableArray()));
    }

    private MenuOptionRecord GetMenuOptionOpen()
    {
        var openFile = new MenuOptionRecord(
            "File",
            async () => 
                await EditorState.ShowInputFileAsync(
                    Dispatcher, 
                    TextEditorService, 
                    TextEditorResourceMapStateWrap.Value));
        
        var openDirectory = new MenuOptionRecord(
            "Directory",
            async () => 
                await FolderExplorerState.ShowInputFileAsync(Dispatcher));
        
        var openCSharpProject = new MenuOptionRecord(
            "C# Project - TODO: Adhoc Sln",
            async () => 
                await EditorState.ShowInputFileAsync(
                    Dispatcher, 
                    TextEditorService, 
                    TextEditorResourceMapStateWrap.Value));
        
        var openDotNetSolution = new MenuOptionRecord(
            ".NET Solution",
            async () => 
                await SolutionExplorerState.ShowInputFileAsync(Dispatcher));

        return new MenuOptionRecord(
            "Open",
            SubMenu: new MenuRecord(
                new []
                {
                    openFile,
                    openDirectory,
                    openCSharpProject,
                    openDotNetSolution
                }.ToImmutableArray()));
    }

    private void AddActiveFileDropdown()
    {
        Dispatcher.Dispatch(new AddActiveDropdownKeyAction(_dropdownKeyFileDropdown));
    }
    
    /// <summary>
    /// TODO: Make this method abstracted into a component that takes care of the UI to show the dropdown and to restore focus when menu closed
    /// </summary>
    private void RestoreFocusToFileButton()
    {
        _fileButtonDisplay?.ButtonElementReference?
            .FocusAsync();
    }

    private void OpenNewDotNetSolutionDialog()
    {
        var dialogRecord = new DialogRecord(
            DialogKey.NewDialogKey(), 
            "New .NET Solution",
            typeof(DotNetSolutionFormDisplay),
            null);
        
        Dispatcher.Dispatch(
            new RegisterDialogRecordAction(
                dialogRecord));
    }
    
    private void TestNotificationsOnClick()
    {
        var notificationInformative = new NotificationRecord(
            NotificationKey.NewNotificationKey(), 
            "AaaTest",
            typeof(CommonInformativeNotificationDisplay),
            new Dictionary<string, object?>
            {
                {
                    nameof(CommonInformativeNotificationDisplay.Message), 
                    "messageTest"
                },
            });
        
        var notificationError = new NotificationRecord(
            NotificationKey.NewNotificationKey(), 
            "AaaTest",
            typeof(CommonErrorNotificationDisplay),
            new Dictionary<string, object?>
            {
                {
                    nameof(CommonErrorNotificationDisplay.Message), 
                    "messageTest"
                },
            });
        
        Dispatcher.Dispatch(
            new NotificationState.RegisterNotificationAction(
                notificationInformative));
        
        Dispatcher.Dispatch(
            new NotificationState.RegisterNotificationAction(
                notificationError));
    }
}