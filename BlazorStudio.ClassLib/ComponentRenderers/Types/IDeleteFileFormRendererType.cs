﻿using BlazorStudio.ClassLib.FileSystem.Interfaces;

namespace BlazorStudio.ClassLib.ComponentRenderers;

public interface IDeleteFileFormRendererType
{
    public IAbsoluteFilePath AbsoluteFilePath { get; set; }
    public bool IsDirectory { get; set; }
    public Action<IAbsoluteFilePath> OnAfterSubmitAction { get; set; }
}