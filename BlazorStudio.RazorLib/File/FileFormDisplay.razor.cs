﻿using System.Collections.Immutable;
using BlazorStudio.ClassLib.CommonComponents;
using BlazorStudio.ClassLib.FileTemplates;
using BlazorStudio.ClassLib.Keyboard;
using BlazorStudio.RazorLib.Menu;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorStudio.RazorLib.File;

public partial class FileFormDisplay 
    : ComponentBase, IFileFormRendererType
{
    [CascadingParameter]
    public MenuOptionWidgetParameters? MenuOptionWidgetParameters { get; set; }
    
    [Parameter, EditorRequired]
    public string FileName { get; set; } = string.Empty;
    [Parameter, EditorRequired]
    public Action<string, IFileTemplate, ImmutableArray<IFileTemplate>> OnAfterSubmitAction { get; set; } = null!;
    [Parameter]
    public bool IsDirectory { get; set; }
    [Parameter]
    public bool CheckForTemplates { get; set; }

    private string? _previousFileNameParameter;

    private string _fileName = string.Empty;
    private FileTemplatesDisplay? _fileTemplatesDisplay;
    private ElementReference? _inputElementReference;

    private string PlaceholderText => IsDirectory
        ? "Directory name"
        : "File name";
    
    public string InputFileName => _fileName;

    protected override Task OnParametersSetAsync()
    {
        if (_previousFileNameParameter is null ||
            _previousFileNameParameter != FileName)
        {
            _previousFileNameParameter = FileName;
            _fileName = FileName;
        }
        
        return base.OnParametersSetAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (MenuOptionWidgetParameters is not null && 
                _inputElementReference is not null)
            {
                await _inputElementReference.Value.FocusAsync();
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
                await MenuOptionWidgetParameters.CompleteWidgetAsync.Invoke(
                    () => OnAfterSubmitAction.Invoke(
                        _fileName, 
                        _fileTemplatesDisplay?.ExactMatchFileTemplate,
                        _fileTemplatesDisplay?.RelatedMatchFileTemplates 
                            ?? ImmutableArray<IFileTemplate>.Empty));
            }
        }
    }
}