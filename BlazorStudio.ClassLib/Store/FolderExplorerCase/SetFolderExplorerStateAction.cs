using BlazorStudio.ClassLib.FileSystem.Interfaces;

namespace BlazorStudio.ClassLib.Store.FolderExplorerCase;

public record SetFolderExplorerStateAction(IAbsoluteFilePath? AbsoluteFilePath);