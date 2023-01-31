﻿using BlazorStudio.ClassLib.FileSystem.Interfaces;

namespace BlazorStudio.ClassLib.InputFile;

public class InputFilePattern
{
    public InputFilePattern(
        string patternName,
        Func<IAbsoluteFilePath, bool> matchesPatternFunc)
    {
        PatternName = patternName;
        MatchesPatternFunc = matchesPatternFunc;
    }

    public string PatternName { get; }
    public Func<IAbsoluteFilePath, bool> MatchesPatternFunc { get; }
}