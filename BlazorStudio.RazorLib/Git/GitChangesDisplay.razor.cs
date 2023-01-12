﻿using BlazorStudio.ClassLib.CommandLine;
using BlazorStudio.ClassLib.CommonComponents;
using BlazorStudio.ClassLib.Store.GitCase;
using BlazorStudio.ClassLib.Store.TerminalCase;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using Microsoft.AspNetCore.Components;

namespace BlazorStudio.RazorLib.Git;

public partial class GitChangesDisplay : FluxorComponent, IGitDisplayRendererType
{
    [Inject]
    private IState<GitState> GitStateWrap { get; set; } = null!;
    [Inject]
    private IState<TerminalSessionsState> TerminalSessionsStateWrap { get; set; } = null!;
    [Inject]
    private IDispatcher Dispatcher { get; set; } = null!;

    private readonly TerminalCommandKey _gitInitTerminalCommandKey = 
        TerminalCommandKey.NewTerminalCommandKey();
    
    private async Task GitInitOnClickAsync()
    {
        var gitState = GitStateWrap.Value;
        
        if (gitState.MostRecentTryFindGitFolderInDirectoryAction is null)
            return;
        
        var gitInitCommand = new TerminalCommand(
            _gitInitTerminalCommandKey,
            GitCliFacts.GIT_INIT_COMMAND,
            gitState.MostRecentTryFindGitFolderInDirectoryAction.DirectoryAbsoluteFilePath.GetAbsoluteFilePathString(),
            CancellationToken.None);
        
        var generalTerminalSession = TerminalSessionsStateWrap.Value.TerminalSessionMap[
            TerminalSessionFacts.GENERAL_TERMINAL_SESSION_KEY];
        
        await generalTerminalSession
            .EnqueueCommandAsync(gitInitCommand);
    }

    private async Task RefreshGitOnClickAsync()
    {
        var gitState = GitStateWrap.Value;

        if (gitState.GitFolderAbsoluteFilePath is null ||
            !Directory.Exists(gitState.GitFolderAbsoluteFilePath.GetAbsoluteFilePathString()) ||
            gitState.GitFolderAbsoluteFilePath.ParentDirectory is null)
        {
            return;
        }
        
        var gitStatusCommand = new TerminalCommand(
            _gitInitTerminalCommandKey,
            GitCliFacts.GIT_STATUS_COMMAND,
            gitState.GitFolderAbsoluteFilePath.ParentDirectory.GetAbsoluteFilePathString(),
            CancellationToken.None);
        
        var generalTerminalSession = TerminalSessionsStateWrap.Value.TerminalSessionMap[
            TerminalSessionFacts.GENERAL_TERMINAL_SESSION_KEY];
        
        await generalTerminalSession
            .EnqueueCommandAsync(gitStatusCommand);
    }
}