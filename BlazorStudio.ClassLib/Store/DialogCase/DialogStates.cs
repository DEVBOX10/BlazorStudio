﻿using System.Collections.Immutable;
using Fluxor;

namespace BlazorStudio.ClassLib.Store.DialogCase;

[FeatureState]
public record DialogStates(ImmutableList<DialogRecord> List)
{
    private DialogStates() : this(ImmutableList<DialogRecord>.Empty)
    {
    }

    public DialogKey? DialogKeyWithOverridenZIndex { get; set; }
    public DialogKey? MostRecentlyFocusedDialogKey { get; set; }
}