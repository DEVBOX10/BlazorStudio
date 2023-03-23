﻿using Microsoft.JSInterop;

namespace BlazorStudio.Tests.Basics.FileSystem;

public class DoNothingJsRuntime : IJSRuntime
{
    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
    {
        return default;
    }

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
    {
        return default;
    }
}