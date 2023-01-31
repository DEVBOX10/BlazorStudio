﻿using System.Collections.Immutable;
using BlazorStudio.ClassLib.FileSystem.Interfaces;
using BlazorStudio.ClassLib.InputFile;
using BlazorStudio.ClassLib.TreeViewImplementations;
using Fluxor;

namespace BlazorStudio.ClassLib.Store.InputFileCase;

[FeatureState]
public partial record InputFileState(
    int IndexInHistory,
    ImmutableList<TreeViewAbsoluteFilePath> OpenedTreeViewModelHistory,
    TreeViewAbsoluteFilePath? SelectedTreeViewModel,
    Func<IAbsoluteFilePath?, Task> OnAfterSubmitFunc,
    Func<IAbsoluteFilePath?, Task<bool>> SelectionIsValidFunc,
    ImmutableArray<InputFilePattern> InputFilePatterns,
    InputFilePattern? SelectedInputFilePattern,
    string SearchQuery,
    string Message)
{
    private InputFileState() : this(
        -1,
        ImmutableList<TreeViewAbsoluteFilePath>.Empty,
        null,
        _ => Task.CompletedTask,
        _ => Task.FromResult(false),
        ImmutableArray<InputFilePattern>.Empty,
        null,
        string.Empty,
        string.Empty) 
    {
    }
}