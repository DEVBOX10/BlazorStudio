﻿using Blazor.Text.Editor.Analysis.Html.ClassLib.InjectLanguage;
using Blazor.Text.Editor.Analysis.Shared;

namespace Blazor.Text.Editor.Analysis.Html.ClassLib.ExtensionMethods;

public static class StringWalkerExtensions
{
    public static bool CheckForInjectedLanguageCodeBlockTag(
        this StringWalker stringWalker,
        InjectedLanguageDefinition injectedLanguageDefinition)
    {
        return stringWalker.CheckForSubstring(injectedLanguageDefinition.InjectedLanguageCodeBlockTag) &&
               !stringWalker.CheckForSubstring(injectedLanguageDefinition.InjectedLanguageCodeBlockTagEscaped);
    }
}