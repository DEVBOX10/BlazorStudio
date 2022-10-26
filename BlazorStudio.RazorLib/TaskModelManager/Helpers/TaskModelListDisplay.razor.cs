﻿using System.Collections.Immutable;
using BlazorStudio.ClassLib.TaskModelManager;
using Microsoft.AspNetCore.Components;

namespace BlazorStudio.RazorLib.TaskModelManager.Helpers;

public partial class TaskModelListDisplay : ComponentBase, IDisposable
{
    private bool _isExpanded = true;
    private ImmutableArray<ITaskModel> _taskModelCache = ImmutableArray<ITaskModel>.Empty;
    [Parameter]
    public Func<IEnumerable<ITaskModel>> GetTaskModelsFunc { get; set; } = null!;
    [Parameter]
    public Action ClearTaskModelsAction { get; set; } = null!;
    [Parameter]
    public string ListTitle { get; set; } = null!;
    [Parameter]
    public string ClassParameter { get; set; } = string.Empty;
    [Parameter]
    public string StyleParameter { get; set; } = string.Empty;
    [Parameter]
    public bool ShowCancelButton { get; set; }

    public void Dispose()
    {
        TaskModelManagerService.OnTasksStateHasChangedEventHandler -=
            TaskModelManagerServiceStateHasChangedEventHandler;
        TaskModelManagerService.OnTaskSeemsUnresponsiveEventHandler -=
            TaskModelManagerServiceStateHasChangedEventHandler;
    }

    protected override void OnInitialized()
    {
        _taskModelCache = GetTaskModelsFunc().ToImmutableArray();

        TaskModelManagerService.OnTasksStateHasChangedEventHandler +=
            TaskModelManagerServiceStateHasChangedEventHandler;
        TaskModelManagerService.OnTaskSeemsUnresponsiveEventHandler +=
            TaskModelManagerServiceStateHasChangedEventHandler;

        base.OnInitialized();
    }

    private async void TaskModelManagerServiceStateHasChangedEventHandler()
    {
        _taskModelCache = GetTaskModelsFunc().ToImmutableArray();
        await InvokeAsync(StateHasChanged);
    }

    private void ToggleIsExpandedOnClick()
    {
        _isExpanded = !_isExpanded;

        if (_isExpanded)
            _taskModelCache = GetTaskModelsFunc().ToImmutableArray();

        StateHasChanged();
    }
}