using BlazorStudio.ClassLib.Store.TerminalCase;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using Microsoft.AspNetCore.Components;

namespace BlazorStudio.RazorLib.Terminal;

public partial class TerminalWellKnownTab : FluxorComponent
{
    [Inject]
    private IState<WellKnownTerminalSessionsState> WellKnownTerminalSessionsStateWrap { get; set; } = null!;
    [Inject]
    private IStateSelection<TerminalSessionsState, TerminalSession?> TerminalSessionsStateSelection { get; set; } = null!;
    [Inject]
    private IState<TerminalSessionWasModifiedState> TerminalSessionWasModifiedStateWrap { get; set; } = null!;
    [Inject]
    private IDispatcher Dispatcher { get; set; } = null!;

    [Parameter, EditorRequired]
    public TerminalSessionKey WellKnownTerminalSessionKey { get; set; } = null!;

    private string CssClassString => 
        $"bstudio_terminal-tab {ActiveTerminalCommandKeyCssClassString}";
    
    private string ActiveTerminalCommandKeyCssClassString => 
        IsActiveTerminalCommandKey
            ? "bstudio_active"
            : string.Empty;

    private bool IsActiveTerminalCommandKey => 
        WellKnownTerminalSessionsStateWrap.Value.ActiveTerminalSessionKey == 
        WellKnownTerminalSessionKey;

    protected override void OnInitialized()
    {
        TerminalSessionsStateSelection
            .Select(x =>
            {
                if (x.TerminalSessionMap.TryGetValue(
                        WellKnownTerminalSessionKey, out var wellKnownTerminalSession))
                {
                    return wellKnownTerminalSession;
                }
                
                return null;
            });
        
        base.OnInitialized();
    }
    
    private Task DispatchSetActiveTerminalCommandKeyActionOnClick()
    {
        Dispatcher.Dispatch(
            new WellKnownTerminalSessionsState.SetActiveWellKnownTerminalSessionKey(
                WellKnownTerminalSessionKey));

        return Task.CompletedTask;
    }

    private Task ClearStandardOutOnClick()
    {
        TerminalSessionsStateSelection.Value
            .ClearStandardOut();

        return Task.CompletedTask;
    }

    private Task KillProcessOnClick()
    {
        TerminalSessionsStateSelection.Value
            .KillProcess();

        return Task.CompletedTask;
    }
}