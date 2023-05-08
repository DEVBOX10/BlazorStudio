using System.Collections.Immutable;
using BlazorCommon.RazorLib.ComponentRenderers;
using BlazorStudio.ClassLib.Nuget;
using BlazorStudio.ClassLib.Store.TerminalCase;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using Microsoft.AspNetCore.Components;
using BlazorCommon.RazorLib.ComponentRenderers.Types;
using BlazorCommon.RazorLib.Notification;
using BlazorCommon.RazorLib.Store.NotificationCase;
using BlazorStudio.ClassLib.CommandLine;
using BlazorStudio.ClassLib.Store.DotNetSolutionCase;
using BlazorStudio.ClassLib.Store.NugetPackageManagerCase;

namespace BlazorStudio.RazorLib.NuGet;

public partial class NugetPackageDisplay : FluxorComponent
{
    [Inject]
    private IState<NuGetPackageManagerState> NuGetPackageManagerStateWrap { get; set; } = null!;
    [Inject]
    private IState<DotNetSolutionState> DotNetSolutionStateWrap { get; set; } = null!;
    [Inject]
    private IState<TerminalSessionsState> TerminalSessionsStateWrap { get; set; } = null!;
    [Inject]
    private IBlazorCommonComponentRenderers BlazorCommonComponentRenderers { get; set; } = null!;
    [Inject]
    private IDispatcher Dispatcher { get; set; } = null!;

    [Parameter, EditorRequired]
    public NugetPackageRecord NugetPackageRecord { get; set; } = null!;

    private static readonly TerminalCommandKey AddNugetPackageTerminalCommandKey = TerminalCommandKey.NewTerminalCommandKey();
    
    private string _nugetPackageVersionString;

    private ImmutableArray<NugetPackageVersionRecord> _nugetPackageVersionsOrdered = ImmutableArray<NugetPackageVersionRecord>.Empty;
    private string? _previousNugetPackageId;
    
    protected override void OnParametersSet()
    {
        if (_previousNugetPackageId is null ||
            _previousNugetPackageId != NugetPackageRecord.Id)
        {
            _previousNugetPackageId = NugetPackageRecord.Id;
            
            _nugetPackageVersionsOrdered = NugetPackageRecord.Versions
                .OrderByDescending(x => x.Version)
                .ToImmutableArray();

            _nugetPackageVersionString = _nugetPackageVersionsOrdered
                .FirstOrDefault()?
                .Version
                    ?? string.Empty;
        }
        
        base.OnParametersSet();
    }

    private void SelectedNugetVersionChanged(ChangeEventArgs changeEventArgs)
    {
        _nugetPackageVersionString = changeEventArgs.Value?.ToString() ?? string.Empty;
    }
    
    private bool ValidateSolutionContainsSelectedProject()
    {
        var dotNetSolutionState = DotNetSolutionStateWrap.Value;
        var nuGetPackageManagerState = NuGetPackageManagerStateWrap.Value;
        
        if (dotNetSolutionState.DotNetSolution is null ||
            nuGetPackageManagerState.SelectedProjectToModify is null)
            return false;
        
        return dotNetSolutionState.DotNetSolution.DotNetProjects
            .Exists(x =>
                x.ProjectIdGuid == nuGetPackageManagerState.SelectedProjectToModify.ProjectIdGuid);
    }
    
    private async Task AddNugetPackageReferenceOnClick()
    {
        var targetProject = NuGetPackageManagerStateWrap.Value.SelectedProjectToModify;
        var targetNugetPackage = NugetPackageRecord;
        var targetNugetVersion = _nugetPackageVersionString;

        if (!ValidateSolutionContainsSelectedProject() ||
            targetProject is null)
        {
            return;
        }
        
        var parentDirectory = targetProject.AbsoluteFilePath.ParentDirectory;

        if (parentDirectory is null)
            return;

        var formattedCommand = DotNetCliFacts.FormatAddNugetPackageReferenceToProject(
            targetProject.AbsoluteFilePath.GetAbsoluteFilePathString(),
            targetNugetPackage.Id,
            targetNugetVersion);
        
        var addNugetPackageReferenceCommand = new TerminalCommand(
            AddNugetPackageTerminalCommandKey,
            formattedCommand.targetFileName,
            formattedCommand.arguments,
            parentDirectory.GetAbsoluteFilePathString(),
            CancellationToken.None, () =>
            {
                if (BlazorCommonComponentRenderers.InformativeNotificationRendererType is not null)
                {
                    var notificationInformative  = new NotificationRecord(
                        NotificationKey.NewNotificationKey(), 
                        "Add Nuget Package Reference",
                        BlazorCommonComponentRenderers.InformativeNotificationRendererType,
                        new Dictionary<string, object?>
                        {
                            {
                                nameof(IInformativeNotificationRendererType.Message), 
                                $"{targetNugetPackage.Title}, {targetNugetVersion} was added to {targetProject.DisplayName}"
                            },
                        },
                        TimeSpan.FromSeconds(6),
                        null);
        
                    Dispatcher.Dispatch(
                        new NotificationRecordsCollection.RegisterAction(
                            notificationInformative));
                }
                
                return Task.CompletedTask;
            });
        
        var generalTerminalSession = TerminalSessionsStateWrap.Value.TerminalSessionMap[
            TerminalSessionFacts.GENERAL_TERMINAL_SESSION_KEY];
        
        await generalTerminalSession
            .EnqueueCommandAsync(addNugetPackageReferenceCommand);
    }
}