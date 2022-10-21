using BlazorStudio.ClassLib.FileSystem.Classes;
using Microsoft.CodeAnalysis;

namespace BlazorStudio.ClassLib.RoslynHelpers;

public class IndexedDocument
{
    public IndexedDocument(Document document, AbsoluteFilePathDotNet absoluteFilePathDotNet)
    {
        Document = document;
        AbsoluteFilePathDotNet = absoluteFilePathDotNet;
    }
    
    public Document Document { get; set; }
    public AbsoluteFilePathDotNet AbsoluteFilePathDotNet { get; set; }
}