﻿using BlazorStudio.ClassLib.Menu;
using Microsoft.AspNetCore.Components;
using System.Collections.Immutable;
using BlazorCommon.RazorLib.Dialog;
using BlazorCommon.RazorLib.Dimensions;
using BlazorCommon.RazorLib.Dropdown;
using BlazorCommon.RazorLib.Menu;
using BlazorCommon.RazorLib.Notification;
using BlazorCommon.RazorLib.Store.DialogCase;
using BlazorCommon.RazorLib.Store.NotificationCase;
using BlazorCommon.RazorLib.TreeView;
using BlazorCommon.RazorLib.TreeView.Commands;
using BlazorCommon.RazorLib.TreeView.TreeViewClasses;
using BlazorStudio.ClassLib.CommandLine;
using BlazorStudio.ClassLib.CommonComponents;
using BlazorStudio.ClassLib.FileConstants;
using BlazorStudio.ClassLib.FileSystem.Interfaces;
using BlazorStudio.ClassLib.InputFile;
using BlazorStudio.ClassLib.Namespaces;
using BlazorStudio.ClassLib.Store.InputFileCase;
using BlazorStudio.ClassLib.Store.ProgramExecutionCase;
using BlazorStudio.ClassLib.Store.TerminalCase;
using BlazorStudio.ClassLib.TreeViewImplementations;
using BlazorStudio.RazorLib.CSharpProjectForm;
using BlazorStudio.RazorLib.DotNetSolutionForm;
using Fluxor;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorStudio.RazorLib.SolutionExplorer;

public partial class SolutionExplorerContextMenu : ComponentBase
{
    [Inject]
    private IState<TerminalSessionsState> TerminalSessionsStateWrap { get; set; } = null!;
    [Inject]
    private IDispatcher Dispatcher { get; set; } = null!;
    [Inject]
    private ClassLib.Menu.ICommonMenuOptionsFactory CommonMenuOptionsFactory { get; set; } = null!;
    [Inject]
    private ICommonComponentRenderers CommonComponentRenderers { get; set; } = null!;
    [Inject]
    private ITreeViewService TreeViewService { get; set; } = null!;
    
    [Parameter, EditorRequired]
    public ITreeViewCommandParameter TreeViewCommandParameter { get; set; } = null!;

    public static readonly DropdownKey ContextMenuEventDropdownKey = DropdownKey.NewDropdownKey();
    
    /// <summary>
    /// The program is currently running using Photino locally on the user's computer
    /// therefore this static solution works without leaking any information.
    /// </summary>
    public static TreeViewNoType? ParentOfCutFile;
    
    private MenuRecord GetMenuRecord(
        ITreeViewCommandParameter treeViewCommandParameter)
    {
        if (treeViewCommandParameter.TargetNode is null)
            return MenuRecord.Empty;
        
        var menuRecords = new List<MenuOptionRecord>();
        
        var treeViewModel = treeViewCommandParameter.TargetNode;
        var parentTreeViewModel = treeViewModel.Parent;

        var parentTreeViewNamespacePath = parentTreeViewModel as TreeViewNamespacePath;
        
        if (treeViewModel is not TreeViewNamespacePath treeViewNamespacePath ||
            treeViewNamespacePath.Item is null)
        {
            if (treeViewModel is TreeViewSolution treeViewSolution &&
                treeViewSolution.Item is not null)
            {
                if (treeViewSolution.Item.NamespacePath.AbsoluteFilePath.ExtensionNoPeriod ==
                    ExtensionNoPeriodFacts.DOT_NET_SOLUTION)
                {
                    if (treeViewSolution.Parent is null ||
                        treeViewSolution.Parent is TreeViewAdhoc)
                    {
                        menuRecords.AddRange(
                            GetDotNetSolutionMenuOptions(treeViewSolution));
                    }
                }
            }
            else
            {
                return MenuRecord.Empty;
            }
            
        }
        else if (treeViewNamespacePath.Item.AbsoluteFilePath.IsDirectory)
        {
            menuRecords.AddRange(
                GetFileMenuOptions(treeViewNamespacePath, parentTreeViewNamespacePath)
                    .Union(GetDirectoryMenuOptions(treeViewNamespacePath))
                    .Union(GetDebugMenuOptions(treeViewNamespacePath)));
        }
        else
        {
            switch (treeViewNamespacePath.Item.AbsoluteFilePath.ExtensionNoPeriod)
            {
                case ExtensionNoPeriodFacts.C_SHARP_PROJECT:
                    menuRecords.AddRange(
                        GetCSharpProjectMenuOptions(treeViewNamespacePath)
                            .Union(GetDebugMenuOptions(treeViewNamespacePath)));
                    break;
                default:
                    menuRecords.AddRange(
                        GetFileMenuOptions(treeViewNamespacePath, parentTreeViewNamespacePath)
                            .Union(GetDebugMenuOptions(treeViewNamespacePath)));
                    break;
            }
        }
        
        return new MenuRecord(
            menuRecords.ToImmutableArray());
    }

    private MenuOptionRecord[] GetDotNetSolutionMenuOptions(TreeViewSolution treeViewSolution)
    {
        if (treeViewSolution.Item is null)
            return Array.Empty<MenuOptionRecord>();
        
        // TODO: Add menu options for non C# projects perhaps a more generic option is good

        var addNewCSharpProject = new MenuOptionRecord(
            "New C# Project",
            MenuOptionKind.Other,
            () => OpenNewCSharpProjectDialog(treeViewSolution.Item.NamespacePath));
        
        var addExistingCSharpProject = new MenuOptionRecord(
            "Existing C# Project",
            MenuOptionKind.Other,
            () => AddExistingProjectToSolution(treeViewSolution.Item.NamespacePath));
        
        return new[]
        {
            new MenuOptionRecord(
                "Add",
                MenuOptionKind.Other,
                SubMenu: new MenuRecord(
                    new MenuOptionRecord[]
                    {
                        addNewCSharpProject,
                        addExistingCSharpProject
                    }.ToImmutableArray())),
        };
    }
    
    private MenuOptionRecord[] GetCSharpProjectMenuOptions(TreeViewNamespacePath treeViewModel)
    {
        var parentDirectory = (IAbsoluteFilePath)treeViewModel.Item.AbsoluteFilePath.Directories.Last();

        // likely a .NET Solution
        var treeViewParent = treeViewModel.Parent as TreeViewNamespacePath;

        return new[]
        {
            CommonMenuOptionsFactory.NewEmptyFile(
                parentDirectory,
                async () => await ReloadTreeViewModel(treeViewModel)),
            CommonMenuOptionsFactory.NewTemplatedFile(
                new NamespacePath(treeViewModel.Item.Namespace, parentDirectory),
                async () => await ReloadTreeViewModel(treeViewModel)),
            CommonMenuOptionsFactory.NewDirectory(
                parentDirectory,
                async () => await ReloadTreeViewModel(treeViewModel)),
            CommonMenuOptionsFactory.PasteClipboard(
                parentDirectory,
                async () =>
                {
                    var localParentOfCutFile = 
                        ParentOfCutFile;
                    
                    ParentOfCutFile = null;
                    
                    if (localParentOfCutFile is not null)
                        await ReloadTreeViewModel(localParentOfCutFile);
                    
                    await ReloadTreeViewModel(treeViewModel);
                }),
            CommonMenuOptionsFactory.AddProjectToProjectReference(
                treeViewModel,
                TerminalSessionsStateWrap.Value
                    .TerminalSessionMap[
                        TerminalSessionFacts.GENERAL_TERMINAL_SESSION_KEY],
                Dispatcher, () => Task.CompletedTask),
            new MenuOptionRecord(
                "Set as Startup Project",
                MenuOptionKind.Other,
                () => Dispatcher.Dispatch(
                    new ProgramExecutionState.SetStartupProjectAbsoluteFilePathAction(
                        treeViewModel.Item.AbsoluteFilePath))),
            CommonMenuOptionsFactory.RemoveCSharpProjectReferenceFromSolution(
                treeViewParent,
                treeViewModel,
                TerminalSessionsStateWrap.Value
                    .TerminalSessionMap[
                        TerminalSessionFacts.GENERAL_TERMINAL_SESSION_KEY],
                Dispatcher,
                () => Task.CompletedTask),
        };
    }
    
    private MenuOptionRecord[] GetDirectoryMenuOptions(TreeViewNamespacePath treeViewModel)
    {
        return new[]
        {
            CommonMenuOptionsFactory.NewEmptyFile(
                treeViewModel.Item.AbsoluteFilePath,
                async () => await ReloadTreeViewModel(treeViewModel)),
            CommonMenuOptionsFactory.NewTemplatedFile(
                treeViewModel.Item,
                async () => await ReloadTreeViewModel(treeViewModel)),
            CommonMenuOptionsFactory.NewDirectory(
                treeViewModel.Item.AbsoluteFilePath,
                async () => await ReloadTreeViewModel(treeViewModel)),
            CommonMenuOptionsFactory.PasteClipboard(
                treeViewModel.Item.AbsoluteFilePath,
                async () =>
                {
                    var localParentOfCutFile = 
                        ParentOfCutFile;
                    
                    ParentOfCutFile = null;
                    
                    if (localParentOfCutFile is not null)
                        await ReloadTreeViewModel(localParentOfCutFile);
                    
                    await ReloadTreeViewModel(treeViewModel);
                }),
        };
    }
    
    private MenuOptionRecord[] GetFileMenuOptions(
        TreeViewNamespacePath treeViewModel,
        TreeViewNamespacePath? parentTreeViewModel)
    {
        return new[]
        {
            CommonMenuOptionsFactory.CopyFile(
                treeViewModel.Item.AbsoluteFilePath,
                () => NotifyCopyCompleted(treeViewModel.Item)),
            CommonMenuOptionsFactory.CutFile(
                treeViewModel.Item.AbsoluteFilePath,
                () => NotifyCutCompleted(treeViewModel.Item, parentTreeViewModel)),
            CommonMenuOptionsFactory.DeleteFile(
                treeViewModel.Item.AbsoluteFilePath,
                async () =>
                {
                    await ReloadTreeViewModel(parentTreeViewModel);
                }),
            CommonMenuOptionsFactory.RenameFile(
                treeViewModel.Item.AbsoluteFilePath,
                Dispatcher,
                async ()  =>
                {
                    await ReloadTreeViewModel(parentTreeViewModel);
                }),
        };
    }
    
    private MenuOptionRecord[] GetDebugMenuOptions(
        TreeViewNamespacePath treeViewModel)
    {
        return new MenuOptionRecord[]
        {
            // new MenuOptionRecord(
            //     $"namespace: {treeViewModel.Item.Namespace}",
            //     MenuOptionKind.Read)
        };
    }

    private void OpenNewCSharpProjectDialog(NamespacePath solutionNamespacePath)
    {
        var dialogRecord = new DialogRecord(
            DialogKey.NewDialogKey(), 
            "New C# Project",
            typeof(CSharpProjectFormDisplay),
            new Dictionary<string, object?>
            {
                {
                    nameof(CSharpProjectFormDisplay.SolutionNamespacePath),
                    solutionNamespacePath
                }
            },
            null)
        {
            IsResizable = true
        };
        
        Dispatcher.Dispatch(
            new DialogRecordsCollection.RegisterAction(
                dialogRecord));
    }
    
    private void AddExistingProjectToSolution(NamespacePath solutionNamespacePath)
    {
        Dispatcher.Dispatch(
            new InputFileState.RequestInputFileStateFormAction(
                "Existing C# Project to add to solution",
                async afp =>
                {
                    if (afp is null)
                        return;
                    
                    var localInterpolatedAddExistingProjectToSolutionCommand = DotNetCliFacts
                        .FormatAddExistingProjectToSolution(
                            solutionNamespacePath.AbsoluteFilePath.GetAbsoluteFilePathString(),
                            afp.GetAbsoluteFilePathString());
                    
                    var addExistingProjectToSolutionTerminalCommand = new TerminalCommand(
                        TerminalCommandKey.NewTerminalCommandKey(), 
                        localInterpolatedAddExistingProjectToSolutionCommand,
                        null,
                        CancellationToken.None,
                        () => Task.CompletedTask);
                    
                    var generalTerminalSession = TerminalSessionsStateWrap.Value.TerminalSessionMap[
                        TerminalSessionFacts.GENERAL_TERMINAL_SESSION_KEY];

                    await generalTerminalSession
                        .EnqueueCommandAsync(addExistingProjectToSolutionTerminalCommand);
                },
                afp =>
                {
                    if (afp is null ||
                        afp.IsDirectory)
                    {
                        return Task.FromResult(false);
                    }

                    return Task.FromResult(
                        afp.ExtensionNoPeriod
                            .EndsWith(ExtensionNoPeriodFacts.C_SHARP_PROJECT));
                },
                new[]
                {
                    new InputFilePattern(
                        "C# Project",
                        afp => 
                            afp.ExtensionNoPeriod
                                .EndsWith(ExtensionNoPeriodFacts.C_SHARP_PROJECT))
                }.ToImmutableArray()));
    }

    /// <summary>
    /// This method I believe is causing bugs
    /// <br/><br/>
    /// For example, when removing a C# Project the
    /// solution is reloaded and a new root is made.
    /// <br/><br/>
    /// Then there is a timing issue where the new root is made and set
    /// as the root. But this method erroneously reloads the old root.
    /// </summary>
    /// <param name="treeViewModel"></param>
    private async Task ReloadTreeViewModel(
        TreeViewNoType? treeViewModel)
    {
        if (treeViewModel is null)
            return;

        await treeViewModel.LoadChildrenAsync();
        
        TreeViewService.ReRenderNode(
            SolutionExplorerDisplay.TreeViewSolutionExplorerStateKey, 
            treeViewModel);
        
        TreeViewService.MoveUp(
            SolutionExplorerDisplay.TreeViewSolutionExplorerStateKey,
            false);
    }
    
    private Task NotifyCopyCompleted(NamespacePath namespacePath)
    {
        if (CommonComponentRenderers.InformativeNotificationRendererType != null)
        {
            var notificationInformative  = new NotificationRecord(
                NotificationKey.NewNotificationKey(), 
                "Copy Action",
                CommonComponentRenderers.InformativeNotificationRendererType,
                new Dictionary<string, object?>
                {
                    {
                        nameof(IInformativeNotificationRendererType.Message), 
                        $"Copied: {namespacePath.AbsoluteFilePath.FilenameWithExtension}"
                    },
                },
                TimeSpan.FromSeconds(3),
                null);

            Dispatcher.Dispatch(
                new NotificationRecordsCollection.RegisterAction(
                    notificationInformative));
        }

        return Task.CompletedTask;
    }
    
    private Task NotifyCutCompleted(
        NamespacePath namespacePath,
        TreeViewNamespacePath? parentTreeViewModel)
    {
        ParentOfCutFile = parentTreeViewModel;

        if (CommonComponentRenderers.InformativeNotificationRendererType != null)
        {
            var notificationInformative  = new NotificationRecord(
                NotificationKey.NewNotificationKey(), 
                "Cut Action",
                CommonComponentRenderers.InformativeNotificationRendererType,
                new Dictionary<string, object?>
                {
                    {
                        nameof(IInformativeNotificationRendererType.Message), 
                        $"Cut: {namespacePath.AbsoluteFilePath.FilenameWithExtension}"
                    },
                },
                TimeSpan.FromSeconds(3),
                null);
        
            Dispatcher.Dispatch(
                new NotificationRecordsCollection.RegisterAction(
                    notificationInformative));
        }

        return Task.CompletedTask;
    }
    
    public static string GetContextMenuCssStyleString(ITreeViewCommandParameter? treeViewCommandParameter)
    {
        if (treeViewCommandParameter?.ContextMenuFixedPosition is null)
            return "display: none;";
        
        var left = 
            $"left: {treeViewCommandParameter.ContextMenuFixedPosition.LeftPositionInPixels.ToCssValue()}px;";
        
        var top = 
            $"top: {treeViewCommandParameter.ContextMenuFixedPosition.TopPositionInPixels.ToCssValue()}px;";

        return $"{left} {top}";
    }
}
