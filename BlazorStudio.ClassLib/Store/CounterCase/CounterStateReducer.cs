using Fluxor;

namespace BlazorStudio.ClassLib.Store.CounterCase;

public class CounterStateReducer
{
    [ReducerMethod(typeof(IncrementCounterStateAction))]
    public static CounterState ReduceIncrementCounterStateAction(CounterState previousCounterState)
    {
        return previousCounterState with
        {
            Count = previousCounterState.Count + 1
        };
    }
}