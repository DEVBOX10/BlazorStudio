﻿namespace BlazorStudio.ClassLib.DotNet;

public class DotNetSolutionFacts
{
    // Example text: Project("{9A19103F-16F7-4668-BE54-9A1E7A4F7556}")
    public const string START_OF_PROJECT_DEFINITION = "Project(";
    // Example text: Project(...\nEndProject\n
    public const string END_OF_PROJECT_DEFINITION = "EndProject";
    
    // Example text: Project("{9A19103F-16F7-4668-BE54-9A1E7A4F7556}")
    public const string START_OF_PROJECT_DEFINITION_MEMBER = "\"";
    // Example text: Project("{9A19103F-16F7-4668-BE54-9A1E7A4F7556}")
    public const string END_OF_PROJECT_DEFINITION_MEMBER = "\"";
    
    // Example text: Project("{9A19103F-16F7-4668-BE54-9A1E7A4F7556}")
    public const string START_OF_GUID = "{";
    // Example text: Project("{9A19103F-16F7-4668-BE54-9A1E7A4F7556}")
    public const string END_OF_GUID = "}";
}