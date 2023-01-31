using System.Collections.Immutable;
using BlazorStudio.ClassLib.FileSystem.Interfaces;
using BlazorStudio.ClassLib.InputFile;
using BlazorStudio.ClassLib.Store.InputFileCase;
using Fluxor;

namespace BlazorStudio.ClassLib.Store.FolderExplorerCase;

[FeatureState]
public record FolderExplorerState(IAbsoluteFilePath? AbsoluteFilePath)
{
    public FolderExplorerState() : this(default(IAbsoluteFilePath))
    {
        
    }
    
    public static Task ShowInputFileAsync(IDispatcher dispatcher)
    {
        dispatcher.Dispatch(
            new InputFileState.RequestInputFileStateFormAction(
                "FolderExplorer",
                afp =>
                {
                    dispatcher.Dispatch(
                        new SetFolderExplorerStateAction(afp));
                    
                    return Task.CompletedTask;
                },
                afp =>
                {
                    if (afp is null ||
                        !afp.IsDirectory)
                    {
                        return Task.FromResult(false);
                    }
                    
                    return Task.FromResult(true);
                },
                new []
                {
                    new InputFilePattern(
                        "Directory",
                        afp => afp.IsDirectory)
                }.ToImmutableArray()));
        
        return Task.CompletedTask;
    }
}