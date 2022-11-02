﻿namespace BlazorStudio.ClassLib.Store.TerminalCase;

public record TerminalCommandKey(Guid Guid, string? DisplayName)
{
    public static TerminalCommandKey Empty { get; } = new(Guid.Empty, null);
    
    public static TerminalCommandKey NewTerminalCommandKey(string? displayName = null)
    {
        return new(Guid.NewGuid(), displayName);
    }
}