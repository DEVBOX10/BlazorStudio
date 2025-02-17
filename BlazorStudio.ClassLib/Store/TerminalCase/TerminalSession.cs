﻿using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Reactive.Linq;
using System.Text;
using BlazorCommon.RazorLib.BackgroundTaskCase;
using BlazorCommon.RazorLib.ComponentRenderers;
using BlazorCommon.RazorLib.ComponentRenderers.Types;
using BlazorCommon.RazorLib.Notification;
using BlazorCommon.RazorLib.Store.NotificationCase;
using BlazorStudio.ClassLib.FileSystem.Interfaces;
using BlazorStudio.ClassLib.State;
using BlazorTextEditor.RazorLib.Model;
using BlazorTextEditor.RazorLib.ViewModel;
using CliWrap;
using CliWrap.EventStream;
using Fluxor;

namespace BlazorStudio.ClassLib.Store.TerminalCase;

public class TerminalSession
{
    private readonly IDispatcher _dispatcher;
    private readonly IFileSystemProvider _fileSystemProvider;
    private readonly IBackgroundTaskQueue _backgroundTaskQueue;
    private readonly IBlazorCommonComponentRenderers _blazorCommonComponentRenderers;
    private readonly List<TerminalCommand> _terminalCommandsHistory = new();
    private CancellationTokenSource _commandCancellationTokenSource = new();

    private readonly ConcurrentQueue<TerminalCommand> _terminalCommandsConcurrentQueue = new();

    /// <summary>
    /// TODO: Prove that standard error is correctly being redirected to standard out
    /// </summary>
    private readonly Dictionary<TerminalCommandKey, StringBuilder> _standardOutBuilderMap = new();

    public TerminalSession(
        string? workingDirectoryAbsoluteFilePathString, 
        IDispatcher dispatcher,
        IFileSystemProvider fileSystemProvider,
        IBackgroundTaskQueue backgroundTaskQueue,
        IBlazorCommonComponentRenderers blazorCommonComponentRenderers)
    {
        _dispatcher = dispatcher;
        _fileSystemProvider = fileSystemProvider;
        _backgroundTaskQueue = backgroundTaskQueue;
        _blazorCommonComponentRenderers = blazorCommonComponentRenderers;
        WorkingDirectoryAbsoluteFilePathString = workingDirectoryAbsoluteFilePathString;
    }

    public TerminalSessionKey TerminalSessionKey { get; init; } = 
        TerminalSessionKey.NewTerminalSessionKey();
    
    public TextEditorModelKey TextEditorModelKey => new(TerminalSessionKey.Guid);
    public TextEditorViewModelKey TextEditorViewModelKey => new(TerminalSessionKey.Guid);

    public string? WorkingDirectoryAbsoluteFilePathString { get; private set; }
    
    public TerminalCommand? ActiveTerminalCommand { get; private set; }
    
    public ImmutableArray<TerminalCommand> TerminalCommandsHistory => _terminalCommandsHistory.ToImmutableArray();
    
    // NOTE: the following did not work => _process?.HasExited ?? false;
    public bool HasExecutingProcess { get; private set; }
    
    public string ReadStandardOut()
    {
        return string
            .Join(string.Empty, _standardOutBuilderMap
                .Select(x => x.Value.ToString())
                .ToArray());
    }
    
    public string? ReadStandardOut(TerminalCommandKey terminalCommandKey)
    {
        if (_standardOutBuilderMap
            .TryGetValue(terminalCommandKey, out var output))
        {
            return output.ToString();
        }

        return null;
    }

    public async Task EnqueueCommandAsync(TerminalCommand terminalCommand)
    {
        var backgroundTask = new BackgroundTask(
            async cancellationToken =>
            {
                if (terminalCommand.ChangeWorkingDirectoryTo is not null)
                    WorkingDirectoryAbsoluteFilePathString = terminalCommand.ChangeWorkingDirectoryTo;

                if (terminalCommand.TargetFilePath == "cd" &&
                    terminalCommand.Arguments.Any())
                {
                    // TODO: Don't keep this logic as it is hacky. I'm trying to set myself up to be able to run "gcc" to compile ".c" files. Then I can work on adding symbol related logic like "go to definition" or etc.
                    WorkingDirectoryAbsoluteFilePathString = terminalCommand.Arguments.ElementAt(0);
                }
                
                _terminalCommandsHistory.Add(terminalCommand);
                ActiveTerminalCommand = terminalCommand;

                var command = Cli.Wrap(terminalCommand.TargetFilePath);

                if (terminalCommand.Arguments.Any())
                    command = command.WithArguments(terminalCommand.Arguments);

                if (terminalCommand.ChangeWorkingDirectoryTo is not null)
                {
                    command = command
                        .WithWorkingDirectory(terminalCommand.ChangeWorkingDirectoryTo);
                }
                else if (WorkingDirectoryAbsoluteFilePathString is not null)
                {
                    command = command
                        .WithWorkingDirectory(WorkingDirectoryAbsoluteFilePathString);
                }

                // Push-based event stream
                {
                    var terminalCommandKey = terminalCommand.TerminalCommandKey;
                    
                    _standardOutBuilderMap.TryAdd(
                        terminalCommand.TerminalCommandKey,
                        new StringBuilder());
                    
                    HasExecutingProcess = true;
                    DispatchNewStateKey();

                    try
                    {
                        await command
                            .Observe(_commandCancellationTokenSource.Token)
                            .ForEachAsync(cmdEvent =>
                            {
                                switch (cmdEvent)
                                {
                                    case StartedCommandEvent started:
                                        _standardOutBuilderMap[terminalCommandKey]
                                            .AppendLine($"Process started; ID: {started.ProcessId}");

                                        DispatchNewStateKey();
                                        break;
                                    case StandardOutputCommandEvent stdOut:
                                        _standardOutBuilderMap[terminalCommandKey]
                                            .AppendLine($"Out> {stdOut.Text}");

                                        DispatchNewStateKey();
                                        break;
                                    case StandardErrorCommandEvent stdErr:
                                        _standardOutBuilderMap[terminalCommandKey]
                                            .AppendLine($"Err> {stdErr.Text}");

                                        DispatchNewStateKey();
                                        break;
                                    case ExitedCommandEvent exited:
                                        _standardOutBuilderMap[terminalCommandKey]
                                            .AppendLine($"Process exited; Code: {exited.ExitCode}");

                                        DispatchNewStateKey();
                                        break;
                                }
                            });
                    }
                    catch (Exception e)
                    {
                        var notificationRecord = new NotificationRecord(
                            NotificationKey.NewNotificationKey(), 
                            "Terminal Exception",
                            _blazorCommonComponentRenderers.ErrorNotificationRendererType,
                            new Dictionary<string, object?>
                            {
                                {
                                    nameof(IErrorNotificationRendererType.Message),
                                    e.ToString()
                                }
                            },
                            TimeSpan.FromSeconds(10),
                            IErrorNotificationRendererType.CSS_CLASS_STRING);
                        
                        _dispatcher.Dispatch(
                            new NotificationRecordsCollection.RegisterAction(
                                notificationRecord));
                    }
                    finally
                    {
                        HasExecutingProcess = false;
                        DispatchNewStateKey();
                    
                        if (terminalCommand.ContinueWith is not null)
                            await terminalCommand.ContinueWith.Invoke();
                    }
                }
            },
            "EnqueueCommandAsyncTask",
            "TODO: Describe this task",
            false,
            _ =>  Task.CompletedTask,
            _dispatcher,
            CancellationToken.None);

        _backgroundTaskQueue.QueueBackgroundWorkItem(backgroundTask);
    }
    
    public void ClearStandardOut()
    {
        // TODO: Rewrite this - see contiguous comment block
        //
        // This is awkward but concurrency exceptions I believe might occur
        // otherwise given the current way the code is written (2022-02-11)
        //
        // If one tries to write to standard out dictionary they need a key value entry
        // to exist or they add one
        // 
        // If one sees a key value entry exists they can use the existing StringBuilder
        // but I am tempted to write _standardOutBuilderMap.Clear() thereby
        // clearing all the key value pairs as they write to the StringBuilder.
        foreach (var stringBuilder in _standardOutBuilderMap.Values)
        {
            stringBuilder.Clear();
        }
    }
    
    public void KillProcess()
    {
        _commandCancellationTokenSource.Cancel();
        _commandCancellationTokenSource = new();
    }

    private void DispatchNewStateKey()
    {
        _dispatcher.Dispatch(
            new TerminalSessionWasModifiedStateReducer.SetTerminalSessionStateKeyAction(
                TerminalSessionKey, 
                StateKey.NewStateKey()));
    }
}