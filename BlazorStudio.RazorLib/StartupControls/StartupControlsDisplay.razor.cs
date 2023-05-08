﻿using System.Text;
using BlazorStudio.ClassLib.CommandLine;
using BlazorStudio.ClassLib.Store.ProgramExecutionCase;
using BlazorStudio.ClassLib.Store.TerminalCase;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using Microsoft.AspNetCore.Components;

namespace BlazorStudio.RazorLib.StartupControls;

public partial class StartupControlsDisplay : FluxorComponent
{
    [Inject]
    private IState<ProgramExecutionState> ProgramExecutionStateWrap { get; set; } = null!;
    [Inject]
    private IState<TerminalSessionsState> TerminalSessionsStateWrap { get; set; } = null!;

    private readonly TerminalCommandKey _newDotNetSolutionTerminalCommandKey = 
        TerminalCommandKey.NewTerminalCommandKey();

    private readonly CancellationTokenSource _newDotNetSolutionCancellationTokenSource = new();
    
    private async Task StartProgramWithoutDebuggingOnClick()
    {
        var programExecutionState = ProgramExecutionStateWrap.Value;

        if (programExecutionState.StartupProjectAbsoluteFilePath is null)
            return;

        var parentDirectoryAbsoluteFilePathString = programExecutionState.StartupProjectAbsoluteFilePath.ParentDirectory?.GetAbsoluteFilePathString();

        if (parentDirectoryAbsoluteFilePathString is null)
            return;

        var formattedCommand = DotNetCliFacts
            .FormatStartProjectWithoutDebugging(
                programExecutionState.StartupProjectAbsoluteFilePath);
        
        var startProgramWithoutDebuggingCommand = new TerminalCommand(
            _newDotNetSolutionTerminalCommandKey,
            formattedCommand.targetFileName,
            formattedCommand.arguments,
            parentDirectoryAbsoluteFilePathString,
            _newDotNetSolutionCancellationTokenSource.Token);
        
        var executionTerminalSession = TerminalSessionsStateWrap.Value.TerminalSessionMap[
            TerminalSessionFacts.EXECUTION_TERMINAL_SESSION_KEY];
        
        await executionTerminalSession
            .EnqueueCommandAsync(startProgramWithoutDebuggingCommand);
    }
}