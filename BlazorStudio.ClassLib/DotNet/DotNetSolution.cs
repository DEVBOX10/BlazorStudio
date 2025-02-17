﻿using System.Collections.Immutable;
using BlazorStudio.ClassLib.Namespaces;

namespace BlazorStudio.ClassLib.DotNet;

public record DotNetSolution(
    NamespacePath NamespacePath,
    ImmutableList<IDotNetProject> DotNetProjects,
    ImmutableList<DotNetSolutionFolder> SolutionFolders,
    DotNetSolutionGlobalSection DotNetSolutionGlobalSection)
{
    public Guid Id { get; init; } = Guid.NewGuid();
}