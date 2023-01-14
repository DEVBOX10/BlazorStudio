﻿namespace BlazorStudio.ClassLib.Store.GitCase;

public record GitTask(
    Guid Id,
    string DisplayName,
    object Action,
    CancellationToken CancellationToken);