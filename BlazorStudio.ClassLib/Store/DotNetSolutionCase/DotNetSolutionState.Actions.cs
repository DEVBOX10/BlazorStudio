﻿namespace BlazorStudio.ClassLib.Store.DotNetSolutionCase;

public partial record DotNetSolutionState
{
    public record WithAction(Func<DotNetSolutionState, DotNetSolutionState> WithFunc);
}