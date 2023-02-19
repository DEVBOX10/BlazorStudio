﻿using BlazorALaCarte.Shared.Keyboard;
using BlazorALaCarte.TreeView;
using BlazorALaCarte.TreeView.Commands;
using BlazorALaCarte.TreeView.Events;
using BlazorALaCarte.TreeView.Services;
using BlazorStudio.ClassLib.CommonComponents;
using BlazorStudio.ClassLib.FileSystem.Interfaces;
using BlazorStudio.ClassLib.Store.InputFileCase;
using BlazorStudio.ClassLib.TreeViewImplementations;
using Fluxor;

namespace BlazorStudio.RazorLib.InputFile.Classes;

public class InputFileTreeViewKeyboardEventHandler : TreeViewKeyboardEventHandler
{
    private readonly TreeViewStateKey _treeViewStateKey;
    private readonly ITreeViewService _treeViewService;
    private readonly IState<InputFileState> _inputFileStateWrap;
    private readonly IDispatcher _dispatcher;
    private readonly ICommonComponentRenderers _commonComponentRenderers;
    private readonly IFileSystemProvider _fileSystemProvider;
    private readonly IEnvironmentProvider _environmentProvider;
    private readonly Action<IAbsoluteFilePath> _setInputFileContentTreeViewRoot;
    private readonly Func<Task> _focusSearchInputElementFunc;
    private readonly Func<List<(TreeViewStateKey treeViewStateKey, TreeViewAbsoluteFilePath treeViewAbsoluteFilePath)>> _getSearchMatchTuplesFunc;

    public InputFileTreeViewKeyboardEventHandler(TreeViewStateKey treeViewStateKey,
        ITreeViewService treeViewService,
        IState<InputFileState> inputFileStateWrap,
        IDispatcher dispatcher,
        ICommonComponentRenderers commonComponentRenderers,
        IFileSystemProvider fileSystemProvider,
        IEnvironmentProvider environmentProvider,
        Action<IAbsoluteFilePath> setInputFileContentTreeViewRoot,
        Func<Task> focusSearchInputElementFunc,
        Func<List<(TreeViewStateKey treeViewStateKey, TreeViewAbsoluteFilePath treeViewAbsoluteFilePath)>> getSearchMatchTuplesFunc)
        : base(treeViewService)
    {
        _treeViewStateKey = treeViewStateKey;
        _treeViewService = treeViewService;
        _inputFileStateWrap = inputFileStateWrap;
        _dispatcher = dispatcher;
        _commonComponentRenderers = commonComponentRenderers;
        _fileSystemProvider = fileSystemProvider;
        _environmentProvider = environmentProvider;
        _setInputFileContentTreeViewRoot = setInputFileContentTreeViewRoot;
        _focusSearchInputElementFunc = focusSearchInputElementFunc;
        _getSearchMatchTuplesFunc = getSearchMatchTuplesFunc;
    }
    
    public override async Task<bool> OnKeyDownAsync(
        ITreeViewCommandParameter treeViewCommandParameter)
    {
        if (treeViewCommandParameter.KeyboardEventArgs is null)
            return false;
        
        switch (treeViewCommandParameter.KeyboardEventArgs.Code)
        {
            case KeyboardKeyFacts.WhitespaceCodes.ENTER_CODE:
                await SetInputFileContentTreeViewRootAsync(treeViewCommandParameter);
                return true;
            case KeyboardKeyFacts.WhitespaceCodes.SPACE_CODE:
                await SetSelectedTreeViewModelAsync(treeViewCommandParameter);
                return true;
        }

        switch (treeViewCommandParameter.KeyboardEventArgs.Key)
        {
            // Tried to have { "Ctrl" + "f" } => MoveFocusToSearchBar
            // however, the webview was ending up taking over
            // and displaying its search bar with focus being set to it.
            //
            // Doing preventDefault just for this one case would be a can of
            // worms as JSInterop is needed, as well a custom Blazor event.
            case "/":
            case "?":
                await MoveFocusToSearchBarAsync(treeViewCommandParameter);
                return true;
            // TODO: Add move to next match and move to previous match
            //
            // case "*":
            //     treeViewCommand = new TreeViewCommand(SetNextMatchAsActiveTreeViewNode);
            //     return true;
            // case "#":
            //     treeViewCommand = new TreeViewCommand(SetPreviousMatchAsActiveTreeViewNode);
            //     return true;
        }

        if (treeViewCommandParameter.KeyboardEventArgs.AltKey)
        {
            var wasMappedToAnAction = await AltModifiedKeymapAsync(treeViewCommandParameter);

            if (wasMappedToAnAction)
                return wasMappedToAnAction;
        }
        
        return await base.OnKeyDownAsync(treeViewCommandParameter);
    }
    
    private async Task<bool> AltModifiedKeymapAsync(
        ITreeViewCommandParameter treeViewCommandParameter)
    {
        if (treeViewCommandParameter.KeyboardEventArgs is null)
            return false;
        
        switch (treeViewCommandParameter.KeyboardEventArgs.Key)
        {
            case KeyboardKeyFacts.MovementKeys.ARROW_LEFT:
                await HandleBackButtonOnClickAsync(treeViewCommandParameter);
                break;
            case KeyboardKeyFacts.MovementKeys.ARROW_UP:
                await HandleUpwardButtonOnClick(treeViewCommandParameter);
                break;
            case KeyboardKeyFacts.MovementKeys.ARROW_RIGHT:
                await HandleForwardButtonOnClick(treeViewCommandParameter);
                break;
            case "r":
                await HandleRefreshButtonOnClick(treeViewCommandParameter);
                break;
        }

        return false;
    }
    
    private Task SetInputFileContentTreeViewRootAsync(
        ITreeViewCommandParameter treeViewCommandParameter)
    {
        var activeNode = treeViewCommandParameter.TreeViewState.ActiveNode;

        var treeViewAbsoluteFilePath = activeNode as TreeViewAbsoluteFilePath;

        if (treeViewAbsoluteFilePath?.Item is null)
            return Task.CompletedTask;
        
        _setInputFileContentTreeViewRoot.Invoke(treeViewAbsoluteFilePath.Item);
        return Task.CompletedTask;
    }
    
    private Task HandleBackButtonOnClickAsync(
        ITreeViewCommandParameter treeViewCommandParameter)
    {
        _dispatcher.Dispatch(new InputFileState.MoveBackwardsInHistoryAction());

        ChangeContentRootToOpenedTreeView(_inputFileStateWrap.Value);
        
        return Task.CompletedTask;
    }
    
    private Task HandleForwardButtonOnClick(
        ITreeViewCommandParameter treeViewCommandParameter)
    {
        _dispatcher.Dispatch(new InputFileState.MoveForwardsInHistoryAction());
        
        ChangeContentRootToOpenedTreeView(_inputFileStateWrap.Value);
        
        return Task.CompletedTask;
    }

    private Task HandleUpwardButtonOnClick(
        ITreeViewCommandParameter treeViewCommandParameter)
    {
        _dispatcher.Dispatch(new InputFileState.OpenParentDirectoryAction(
            _commonComponentRenderers,
            _fileSystemProvider,
            _environmentProvider));
        
        ChangeContentRootToOpenedTreeView(_inputFileStateWrap.Value);

        return Task.CompletedTask;
    }

    private Task HandleRefreshButtonOnClick(
        ITreeViewCommandParameter treeViewCommandParameter)
    {
        _dispatcher.Dispatch(new InputFileState.RefreshCurrentSelectionAction());
        
        ChangeContentRootToOpenedTreeView(_inputFileStateWrap.Value);
        
        return Task.CompletedTask;
    }
    
    private void ChangeContentRootToOpenedTreeView(
        InputFileState inputFileState)
    {
        var openedTreeView = inputFileState.GetOpenedTreeView();
        
        if (openedTreeView.Item is not null)
            _setInputFileContentTreeViewRoot.Invoke(openedTreeView.Item);
    }
    
    private Task SetSelectedTreeViewModelAsync(
        ITreeViewCommandParameter treeViewCommandParameter)
    {
        var activeNode = treeViewCommandParameter.TreeViewState.ActiveNode;
        
        var treeViewAbsoluteFilePath = activeNode as TreeViewAbsoluteFilePath;
        
        if (treeViewAbsoluteFilePath is null)
            return Task.CompletedTask;
        
        var setSelectedTreeViewModelAction = 
            new InputFileState.SetSelectedTreeViewModelAction(
                treeViewAbsoluteFilePath);
        
        _dispatcher.Dispatch(setSelectedTreeViewModelAction);
        
        return Task.CompletedTask;
    }
    
    private async Task MoveFocusToSearchBarAsync(
        ITreeViewCommandParameter treeViewCommandParameter)
    {
        await _focusSearchInputElementFunc.Invoke();
    }
}