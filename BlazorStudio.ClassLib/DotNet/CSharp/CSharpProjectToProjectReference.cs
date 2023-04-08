﻿using BlazorStudio.ClassLib.FileSystem.Interfaces;
using BlazorStudio.ClassLib.Namespaces;

namespace BlazorStudio.ClassLib.DotNet.CSharp;

public class CSharpProjectToProjectReference
{
    public CSharpProjectToProjectReference(
        NamespacePath modifyProjectNamespacePath,
        IAbsoluteFilePath referenceProjectAbsoluteFilePath)
    {
        ModifyProjectNamespacePath = modifyProjectNamespacePath;
        ReferenceProjectAbsoluteFilePath = referenceProjectAbsoluteFilePath;
    }
    
    /// <summary>The <see cref="ModifyProjectNamespacePath"/> is the <see cref="NamespacePath"/> of the Project which will have its XML data modified.</summary>
    public NamespacePath ModifyProjectNamespacePath { get; }
    public IAbsoluteFilePath ReferenceProjectAbsoluteFilePath { get; }
}