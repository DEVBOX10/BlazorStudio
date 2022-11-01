﻿using BlazorStudio.ClassLib.FileSystem.Classes;
using BlazorStudio.ClassLib.FileSystem.Interfaces;
using Fluxor;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace BlazorStudio.ClassLib.Store.WorkspaceCase;

[FeatureState]
public record WorkspaceState(
    Workspace? Workspace,
    VisualStudioInstance? VisualStudioInstance,
    IAbsoluteFilePath? MsBuildAbsoluteFilePath)
{
    public WorkspaceState() : this(
        null,
        null,
        null)
    {
        
    }
}

public record SetWorkspaceStateAction;

public class WorkspaceStateReducer
{
    [ReducerMethod]
    public static WorkspaceState ReduceSetWorkspaceStateAction(
        WorkspaceState inWorkspaceState,
        SetWorkspaceStateAction setWorkspaceStateAction)
    {
        // Attempt to set the version of MSBuild.
        VisualStudioInstance[] visualStudioInstances = MSBuildLocator
            .QueryVisualStudioInstances()
            .ToArray();

        // TODO: Let user choose MSBuild Version
        var visualStudioInstance = visualStudioInstances[0];

        if (!MSBuildLocator.IsRegistered) MSBuildLocator
            .RegisterInstance(visualStudioInstance);
        
        var workspace = MSBuildWorkspace.Create();
        
        var noTrailingSlashMsBuildPath = visualStudioInstance.MSBuildPath;

        if (visualStudioInstance.MSBuildPath.EndsWith(Path.DirectorySeparatorChar) ||
            visualStudioInstance.MSBuildPath.EndsWith(Path.AltDirectorySeparatorChar))
        {
            noTrailingSlashMsBuildPath =
                noTrailingSlashMsBuildPath
                    .Substring(
                        0, 
                        noTrailingSlashMsBuildPath.Length - 1);
        }

        var msBuildAbsoluteFilePath = new AbsoluteFilePath(
            noTrailingSlashMsBuildPath, 
            true);

        return inWorkspaceState with
        {
            Workspace = workspace,
            VisualStudioInstance = visualStudioInstance,
            MsBuildAbsoluteFilePath = msBuildAbsoluteFilePath,
        };
    }
}