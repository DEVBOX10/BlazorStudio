using Fluxor;

namespace BlazorStudio.ClassLib.Store.TerminalCase;

public class WellKnownTerminalSessionsStateReducer
{
    [ReducerMethod]
    public static WellKnownTerminalSessionsState ReduceSetActiveTerminalCommandKeyAction(
        WellKnownTerminalSessionsState inWellKnownTerminalSessionsState,
        WellKnownTerminalSessionsState.SetActiveWellKnownTerminalSessionKey setActiveWellKnownTerminalCommandKeyAction)
    {
        return inWellKnownTerminalSessionsState with
        {
            ActiveTerminalSessionKey = setActiveWellKnownTerminalCommandKeyAction
                .TerminalCommandKey
        };
    }
}