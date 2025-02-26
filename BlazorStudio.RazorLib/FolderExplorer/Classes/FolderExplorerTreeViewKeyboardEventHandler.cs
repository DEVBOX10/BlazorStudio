﻿using BlazorCommon.RazorLib.BackgroundTaskCase;
using BlazorCommon.RazorLib.ComponentRenderers.Types;
using BlazorCommon.RazorLib.Keyboard;
using BlazorCommon.RazorLib.Menu;
using BlazorCommon.RazorLib.Notification;
using BlazorCommon.RazorLib.Store.NotificationCase;
using BlazorCommon.RazorLib.TreeView;
using BlazorCommon.RazorLib.TreeView.Commands;
using BlazorCommon.RazorLib.TreeView.Events;
using BlazorCommon.RazorLib.TreeView.TreeViewClasses;
using BlazorStudio.ClassLib.ComponentRenderers;
using BlazorStudio.ClassLib.FileSystem.Interfaces;
using BlazorStudio.ClassLib.Store.EditorCase;
using BlazorStudio.ClassLib.Store.TerminalCase;
using BlazorStudio.ClassLib.TreeViewImplementations;
using BlazorStudio.RazorLib.SolutionExplorer;
using BlazorTextEditor.RazorLib;
using Fluxor;

namespace BlazorStudio.RazorLib.FolderExplorer.Classes;

public class FolderExplorerTreeViewKeyboardEventHandler : TreeViewKeyboardEventHandler
{
    private readonly IState<TerminalSessionsState> _terminalSessionsStateWrap;
    private readonly BlazorStudio.ClassLib.Menu.ICommonMenuOptionsFactory _commonMenuOptionsFactory;
    private readonly IBlazorStudioComponentRenderers _blazorStudioComponentRenderers;
    private readonly IFileSystemProvider _fileSystemProvider;
    private readonly IDispatcher _dispatcher;
    private readonly ITreeViewService _treeViewService;
    private readonly ITextEditorService _textEditorService;
    private readonly IBackgroundTaskQueue _backgroundTaskQueue;

    public FolderExplorerTreeViewKeyboardEventHandler(
        IState<TerminalSessionsState> terminalSessionsStateWrap,
        BlazorStudio.ClassLib.Menu.ICommonMenuOptionsFactory commonMenuOptionsFactory,
        IBlazorStudioComponentRenderers blazorStudioComponentRenderers,
        IFileSystemProvider fileSystemProvider,
        IDispatcher dispatcher,
        ITreeViewService treeViewService,
        ITextEditorService textEditorService,
        IBackgroundTaskQueue backgroundTaskQueue)
        : base(treeViewService)
    {
        _terminalSessionsStateWrap = terminalSessionsStateWrap;
        _commonMenuOptionsFactory = commonMenuOptionsFactory;
        _blazorStudioComponentRenderers = blazorStudioComponentRenderers;
        _fileSystemProvider = fileSystemProvider;
        _dispatcher = dispatcher;
        _treeViewService = treeViewService;
        _textEditorService = textEditorService;
        _backgroundTaskQueue = backgroundTaskQueue;
    }

    public override async Task<bool> OnKeyDownAsync(
        ITreeViewCommandParameter treeViewCommandParameter)
    {
        if (treeViewCommandParameter.KeyboardEventArgs is null)
            return false;
        
        _ = await base.OnKeyDownAsync(treeViewCommandParameter);

        switch (treeViewCommandParameter.KeyboardEventArgs.Code)
        {
            case KeyboardKeyFacts.WhitespaceCodes.ENTER_CODE:
                await InvokeOpenInEditorAsync(
                    treeViewCommandParameter,
                    true);
                return true;
            case KeyboardKeyFacts.WhitespaceCodes.SPACE_CODE:
                await InvokeOpenInEditorAsync(
                    treeViewCommandParameter,
                    false);
                return true;
        }

        if (treeViewCommandParameter.KeyboardEventArgs.CtrlKey)
        {
            var wasMappedToAnAction = await CtrlModifiedKeymapAsync(treeViewCommandParameter);

            if (wasMappedToAnAction)
                return wasMappedToAnAction;
        }

        if (treeViewCommandParameter.KeyboardEventArgs.AltKey)
        {
            var wasMappedToAnAction = await AltModifiedKeymapAsync(treeViewCommandParameter);

            if (wasMappedToAnAction)
                return wasMappedToAnAction;
        }

        return false;
    }

    private async Task<bool> CtrlModifiedKeymapAsync(
        ITreeViewCommandParameter treeViewCommandParameter)
    {
        if (treeViewCommandParameter.KeyboardEventArgs is null)
            return false;

        if (treeViewCommandParameter.KeyboardEventArgs.AltKey)
        {
            var wasMappedToAnAction = await CtrlAltModifiedKeymapAsync(treeViewCommandParameter);

            if (wasMappedToAnAction)
                return wasMappedToAnAction;
        }

        switch (treeViewCommandParameter.KeyboardEventArgs.Key)
        {
            case "c":
                await InvokeCopyFileAsync(treeViewCommandParameter);
                return true;
            case "x":
                await InvokeCutFile(treeViewCommandParameter);
                return true;
            case "v":
                await InvokePasteClipboard(treeViewCommandParameter);
                return true;
        }
        
        return false;
    }

    /// <summary>
    ///     Do not go from <see cref="AltModifiedKeymapAsync" /> to
    ///     <see cref="CtrlAltModifiedKeymapAsync" />
    ///     <br /><br />
    ///     Code in this method should only be here if it
    ///     does not include a Ctrl key being pressed.
    ///     <br /><br />
    ///     As otherwise, we'd have to permute over
    ///     all the possible keyboard modifier
    ///     keys and have a method for each permutation.
    /// </summary>
    private Task<bool> AltModifiedKeymapAsync(
        ITreeViewCommandParameter treeViewCommandParameter)
    {
        return Task.FromResult(false);
    }

    private Task<bool> CtrlAltModifiedKeymapAsync(
        ITreeViewCommandParameter treeViewCommandParameter)
    {
        return Task.FromResult(false);
    }

    private Task NotifyCopyCompleted(IAbsoluteFilePath absoluteFilePath)
    {
        if (_blazorStudioComponentRenderers.BlazorCommonComponentRenderers.InformativeNotificationRendererType is not null)
        {
            var notificationInformative = new NotificationRecord(
                NotificationKey.NewNotificationKey(),
                "Copy Action",
                _blazorStudioComponentRenderers.BlazorCommonComponentRenderers.InformativeNotificationRendererType,
                new Dictionary<string, object?>
                {
                    {
                        nameof(IInformativeNotificationRendererType.Message),
                        $"Copied: {absoluteFilePath.FilenameWithExtension}"
                    },
                },
                TimeSpan.FromSeconds(3),
                null);

            _dispatcher.Dispatch(
                new NotificationRecordsCollection.RegisterAction(
                    notificationInformative));
        }

        return Task.CompletedTask;
    }

    private Task NotifyCutCompleted(
        IAbsoluteFilePath absoluteFilePath,
        TreeViewAbsoluteFilePath? parentTreeViewModel)
    {
        SolutionExplorerContextMenu.ParentOfCutFile = parentTreeViewModel;

        if (_blazorStudioComponentRenderers.BlazorCommonComponentRenderers.InformativeNotificationRendererType is not null)
        {
            var notificationInformative = new NotificationRecord(
                NotificationKey.NewNotificationKey(),
                "Cut Action",
                _blazorStudioComponentRenderers.BlazorCommonComponentRenderers.InformativeNotificationRendererType,
                new Dictionary<string, object?>
                {
                    {
                        nameof(IInformativeNotificationRendererType.Message),
                        $"Cut: {absoluteFilePath.FilenameWithExtension}"
                    },
                },
                TimeSpan.FromSeconds(3),
                null);

            _dispatcher.Dispatch(
                new NotificationRecordsCollection.RegisterAction(
                    notificationInformative));
        }

        return Task.CompletedTask;
    }

    private Task InvokeCopyFileAsync(ITreeViewCommandParameter treeViewCommandParameter)
    {
        var activeNode = treeViewCommandParameter.TreeViewState.ActiveNode;

        if (activeNode is null ||
            activeNode is not TreeViewAbsoluteFilePath treeViewAbsoluteFilePathPath ||
            treeViewAbsoluteFilePathPath.Item is null)
        {
            return Task.CompletedTask;
        }

        var copyFileMenuOption = _commonMenuOptionsFactory.CopyFile(
            treeViewAbsoluteFilePathPath.Item,
            () => NotifyCopyCompleted(treeViewAbsoluteFilePathPath.Item));

        copyFileMenuOption.OnClick?.Invoke();

        return Task.CompletedTask;
    }

    private Task InvokePasteClipboard(ITreeViewCommandParameter treeViewCommandParameter)
    {
        var activeNode = treeViewCommandParameter.TreeViewState.ActiveNode;

        if (activeNode is null ||
            activeNode is not TreeViewAbsoluteFilePath treeViewAbsoluteFilePathPath ||
            treeViewAbsoluteFilePathPath.Item is null)
        {
            return Task.CompletedTask;
        }

        MenuOptionRecord pasteMenuOptionRecord;

        if (treeViewAbsoluteFilePathPath.Item.IsDirectory)
        {
            pasteMenuOptionRecord = _commonMenuOptionsFactory.PasteClipboard(
                treeViewAbsoluteFilePathPath.Item,
                async () =>
                {
                    var localParentOfCutFile =
                        SolutionExplorerContextMenu.ParentOfCutFile;

                    SolutionExplorerContextMenu.ParentOfCutFile = null;

                    if (localParentOfCutFile is not null)
                        await ReloadTreeViewModel(localParentOfCutFile);

                    await ReloadTreeViewModel(treeViewAbsoluteFilePathPath);
                });
        }
        else
        {
            var parentDirectory = (IAbsoluteFilePath)treeViewAbsoluteFilePathPath
                .Item.Directories.Last();

            pasteMenuOptionRecord = _commonMenuOptionsFactory.PasteClipboard(
                parentDirectory,
                async () =>
                {
                    var localParentOfCutFile =
                        SolutionExplorerContextMenu.ParentOfCutFile;

                    SolutionExplorerContextMenu.ParentOfCutFile = null;

                    if (localParentOfCutFile is not null)
                        await ReloadTreeViewModel(localParentOfCutFile);

                    await ReloadTreeViewModel(treeViewAbsoluteFilePathPath);
                });
        }

        pasteMenuOptionRecord.OnClick?.Invoke();
        return Task.CompletedTask;
    }

    private Task InvokeCutFile(ITreeViewCommandParameter treeViewCommandParameter)
    {
        var activeNode = treeViewCommandParameter.TreeViewState.ActiveNode;

        if (activeNode is null ||
            activeNode is not TreeViewAbsoluteFilePath treeViewAbsoluteFilePathPath ||
            treeViewAbsoluteFilePathPath.Item is null)
        {
            return Task.CompletedTask;
        }

        var parent = treeViewAbsoluteFilePathPath.Parent as TreeViewAbsoluteFilePath;

        MenuOptionRecord cutFileOptionRecord = _commonMenuOptionsFactory.CutFile(
            treeViewAbsoluteFilePathPath.Item,
            () => NotifyCutCompleted(
                treeViewAbsoluteFilePathPath.Item,
                parent));

        cutFileOptionRecord.OnClick?.Invoke();
        return Task.CompletedTask;
    }

    private async Task InvokeOpenInEditorAsync(
        ITreeViewCommandParameter treeViewCommandParameter,
        bool shouldSetFocusToEditor)
    {
        var activeNode = treeViewCommandParameter.TreeViewState.ActiveNode;

        if (activeNode is null ||
            activeNode is not TreeViewAbsoluteFilePath treeViewAbsoluteFilePathPath ||
            treeViewAbsoluteFilePathPath.Item is null)
        {
            return;
        }

        await EditorState.OpenInEditorAsync(
            treeViewAbsoluteFilePathPath.Item,
            shouldSetFocusToEditor,
            _dispatcher,
            _textEditorService,
            _blazorStudioComponentRenderers,
            _fileSystemProvider,
            _backgroundTaskQueue);
    }

    private async Task ReloadTreeViewModel(
        TreeViewNoType? treeViewModel)
    {
        if (treeViewModel is null)
            return;

        await treeViewModel.LoadChildrenAsync();

        _treeViewService.ReRenderNode(
            SolutionExplorerDisplay.TreeViewSolutionExplorerStateKey,
            treeViewModel);

        _treeViewService.MoveUp(
            SolutionExplorerDisplay.TreeViewSolutionExplorerStateKey,
            false);
    }
}