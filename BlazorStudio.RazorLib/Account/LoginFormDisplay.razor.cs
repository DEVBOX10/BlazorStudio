using BlazorALaCarte.Shared.Keyboard;
using BlazorALaCarte.Shared.Menu;
using BlazorStudio.ClassLib.Store.AccountCase;
using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorStudio.RazorLib.Account;

public partial class LoginFormDisplay : ComponentBase
{
    [Inject]
    private IState<AccountState> AccountStateWrap { get; set; } = null!;
    [Inject]
    private IDispatcher Dispatcher { get; set; } = null!;
    
    [CascadingParameter]
    public MenuOptionWidgetParameters? MenuOptionWidgetParameters { get; set; }
    
    private ElementReference? _containerInputElementReference;

    private string GroupKey
    {
        get => AccountStateWrap.Value.GroupKey;
        set
        {
            Dispatcher.Dispatch(new AccountState.AccountStateWithAction(
                inAccountState => inAccountState with
                {
                    GroupKey = value
                }));
        }
    }

    private string ContainerKey
    {
        get => AccountStateWrap.Value.ContainerKey;
        set
        {
            Dispatcher.Dispatch(new AccountState.AccountStateWithAction(
                inAccountState => inAccountState with
                {
                    ContainerKey = value
                }));
        }
    }
    
    private string Alias
    {
        get => AccountStateWrap.Value.Alias;
        set
        {
            Dispatcher.Dispatch(new AccountState.AccountStateWithAction(
                inAccountState => inAccountState with
                {
                    Alias = value
                }));
        }
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (MenuOptionWidgetParameters is not null && 
                _containerInputElementReference is not null)
            {
                await _containerInputElementReference.Value.FocusAsync();
            }
        }
        
        await base.OnAfterRenderAsync(firstRender);
    }

    private async Task HandleOnKeyDown(KeyboardEventArgs keyboardEventArgs)
    {
        if (MenuOptionWidgetParameters is not null)
        {
            if (keyboardEventArgs.Key == KeyboardKeyFacts.MetaKeys.ESCAPE)
            {
                await MenuOptionWidgetParameters.HideWidgetAsync.Invoke();
            }
            else if (keyboardEventArgs.Code == KeyboardKeyFacts.WhitespaceCodes.ENTER_CODE)
            {
                await MenuOptionWidgetParameters.CompleteWidgetAsync.Invoke(() => {});
            }
        }
    }
}