﻿using System.Diagnostics;
using BlazorStudio.ClassLib.FileSystem.Interfaces;

namespace BlazorStudio.ClassLib.Store.TerminalCase;

/// <summary>
/// Do not mix OnAnyDataReceivedAsync and OnAnyDataReceived it is one or the other
/// </summary>
public record EnqueueProcessOnTerminalEntryAction(TerminalEntryKey TerminalEntryKey,
    string Command,
    IAbsoluteFilePath? WorkingDirectoryAbsoluteFilePath,
    Action OnStart,
    Action<Process> OnEnd,
    Action? OnCancelled,
    Action<object, DataReceivedEventArgs>? OnAnyDataReceivedAsync,
    Action<string>? OnAnyDataReceived,
    CancellationToken CancellationToken)
{
    public event EventHandler KillRequestedEventHandler;

    public void InvokeKillRequestedEventHandler()
    {
        KillRequestedEventHandler?.Invoke(this, EventArgs.Empty);
    }
}