﻿using BlazorStudio.ClassLib.Store.FooterCase;
using BlazorStudio.ClassLib.Views;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using Microsoft.AspNetCore.Components;

namespace BlazorStudio.RazorLib.Shared;

public partial class ViewTabDisplay : FluxorComponent
{
    [Inject]
    private IState<FooterState> FooterStateWrap { get; set; } = null!;
    [Inject]
    private IDispatcher Dispatcher { get; set; } = null!;
    
    [Parameter, EditorRequired]
    public View View { get; set; } = null!;

    private bool IsActive => FooterStateWrap.Value.ActiveView.ViewKey ==
                             View.ViewKey;
    
    private string IsActiveCssClass => IsActive
        ? "bcrl_active"
        : string.Empty;

    private void DispatchSetActiveViewAction()
    {
        Dispatcher.Dispatch(new FooterState.SetFooterStateViewAction(
            View));
    }
}