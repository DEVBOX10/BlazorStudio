﻿using Microsoft.AspNetCore.Components;
using System.Collections.Immutable;
using BlazorCommon.RazorLib.ComponentRenderers;
using BlazorCommon.RazorLib.ComponentRenderers.Types;
using BlazorCommon.RazorLib.Dimensions;
using BlazorCommon.RazorLib.Dropdown;
using BlazorCommon.RazorLib.Menu;
using BlazorCommon.RazorLib.Notification;
using BlazorCommon.RazorLib.Store.NotificationCase;
using BlazorCommon.RazorLib.TreeView;
using BlazorCommon.RazorLib.TreeView.Commands;
using BlazorCommon.RazorLib.TreeView.TreeViewClasses;
using BlazorStudio.ClassLib.FileSystem.Interfaces;
using BlazorStudio.ClassLib.Store.TerminalCase;
using BlazorStudio.ClassLib.TreeViewImplementations;
using Fluxor;

namespace BlazorStudio.RazorLib.FolderExplorer.InternalComponents;

public partial class FolderExplorerContextMenu : ComponentBase
{
    [Inject]
    private IState<TerminalSessionsState> TerminalSessionsStateWrap { get; set; } = null!;
    [Inject]
    private IDispatcher Dispatcher { get; set; } = null!;
    [Inject]
    private ClassLib.Menu.ICommonMenuOptionsFactory CommonMenuOptionsFactory { get; set; } = null!;
    [Inject]
    private IBlazorCommonComponentRenderers BlazorCommonComponentRenderers { get; set; } = null!;
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

        var parentTreeViewAbsoluteFilePath = parentTreeViewModel as TreeViewAbsoluteFilePath;
        
        if (treeViewModel is not TreeViewAbsoluteFilePath treeViewAbsoluteFilePath ||
            treeViewAbsoluteFilePath.Item is null)
        {
            return MenuRecord.Empty;
        }
        
        if (treeViewAbsoluteFilePath.Item.IsDirectory)
        {
            menuRecords.AddRange(
                GetFileMenuOptions(treeViewAbsoluteFilePath, parentTreeViewAbsoluteFilePath)
                    .Union(GetDirectoryMenuOptions(treeViewAbsoluteFilePath))
                    .Union(GetDebugMenuOptions(treeViewAbsoluteFilePath)));
        }
        else
        {
            menuRecords.AddRange(
                GetFileMenuOptions(treeViewAbsoluteFilePath, parentTreeViewAbsoluteFilePath)
                    .Union(GetDebugMenuOptions(treeViewAbsoluteFilePath)));
        }
        
        return new MenuRecord(
            menuRecords.ToImmutableArray());
    }

    private MenuOptionRecord[] GetDirectoryMenuOptions(TreeViewAbsoluteFilePath treeViewModel)
    {
        return new[]
        {
            CommonMenuOptionsFactory.NewEmptyFile(
                treeViewModel.Item,
                async () => await ReloadTreeViewModel(treeViewModel)),
            CommonMenuOptionsFactory.NewDirectory(
                treeViewModel.Item,
                async () => await ReloadTreeViewModel(treeViewModel)),
            CommonMenuOptionsFactory.PasteClipboard(
                treeViewModel.Item,
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
        TreeViewAbsoluteFilePath treeViewModel,
        TreeViewAbsoluteFilePath? parentTreeViewModel)
    {
        return new[]
        {
            CommonMenuOptionsFactory.CopyFile(
                treeViewModel.Item,
                () => NotifyCopyCompleted(treeViewModel.Item)),
            CommonMenuOptionsFactory.CutFile(
                treeViewModel.Item,
                () => NotifyCutCompleted(treeViewModel.Item, parentTreeViewModel)),
            CommonMenuOptionsFactory.DeleteFile(
                treeViewModel.Item,
                async () =>
                {
                    await ReloadTreeViewModel(parentTreeViewModel);
                }),
            CommonMenuOptionsFactory.RenameFile(
                treeViewModel.Item,
                Dispatcher,
                async ()  =>
                {
                    await ReloadTreeViewModel(parentTreeViewModel);
                }),
        };
    }
    
    private MenuOptionRecord[] GetDebugMenuOptions(
        TreeViewAbsoluteFilePath treeViewModel)
    {
        return new MenuOptionRecord[]
        {
            // new MenuOptionRecord(
            //     $"namespace: {treeViewModel.Item.Namespace}",
            //     MenuOptionKind.Read)
        };
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
            FolderExplorerDisplay.TreeViewFolderExplorerContentStateKey, 
            treeViewModel);
        
        TreeViewService.MoveUp(
            FolderExplorerDisplay.TreeViewFolderExplorerContentStateKey,
            false);
    }
    
    private Task NotifyCopyCompleted(IAbsoluteFilePath absoluteFilePath)
    {
        if (BlazorCommonComponentRenderers.InformativeNotificationRendererType != null)
        {
            var notificationInformative  = new NotificationRecord(
                NotificationKey.NewNotificationKey(), 
                "Copy Action",
                BlazorCommonComponentRenderers.InformativeNotificationRendererType,
                new Dictionary<string, object?>
                {
                    {
                        nameof(IInformativeNotificationRendererType.Message), 
                        $"Copied: {absoluteFilePath.FilenameWithExtension}"
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
        IAbsoluteFilePath absoluteFilePath,
        TreeViewAbsoluteFilePath? parentTreeViewModel)
    {
        ParentOfCutFile = parentTreeViewModel;

        if (BlazorCommonComponentRenderers.InformativeNotificationRendererType != null)
        {
            var notificationInformative  = new NotificationRecord(
                NotificationKey.NewNotificationKey(), 
                "Cut Action",
                BlazorCommonComponentRenderers.InformativeNotificationRendererType,
                new Dictionary<string, object?>
                {
                    {
                        nameof(IInformativeNotificationRendererType.Message), 
                        $"Cut: {absoluteFilePath.FilenameWithExtension}"
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
