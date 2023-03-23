﻿using System.Collections.Immutable;
using BlazorCommon.RazorLib.Clipboard;
using BlazorCommon.RazorLib.Menu;
using BlazorCommon.RazorLib.Notification;
using BlazorCommon.RazorLib.Store.NotificationCase;
using BlazorStudio.ClassLib.Clipboard;
using BlazorStudio.ClassLib.CommandLine;
using BlazorStudio.ClassLib.CommonComponents;
using BlazorStudio.ClassLib.FileConstants;
using BlazorStudio.ClassLib.FileSystem.Classes.FilePath;
using BlazorStudio.ClassLib.FileSystem.Interfaces;
using BlazorStudio.ClassLib.FileTemplates;
using BlazorStudio.ClassLib.InputFile;
using BlazorStudio.ClassLib.Namespaces;
using BlazorStudio.ClassLib.Store.InputFileCase;
using BlazorStudio.ClassLib.Store.TerminalCase;
using BlazorStudio.ClassLib.TreeViewImplementations;
using Fluxor;

namespace BlazorStudio.ClassLib.Menu;

public class CommonMenuOptionsFactory : ICommonMenuOptionsFactory
{
    private readonly ICommonComponentRenderers _commonComponentRenderers;
    private readonly IFileSystemProvider _fileSystemProvider;
    private readonly IEnvironmentProvider _environmentProvider;
    private readonly IClipboardService _clipboardService;

    /// <summary>
    /// I could not get a dependency injected <see cref="IDispatcher"/>
    /// to work and instead added <see cref="IDispatcher"/> as an argument
    /// to methods in this file that need an <see cref="IDispatcher"/>
    /// </summary>
    public CommonMenuOptionsFactory(
        ICommonComponentRenderers commonComponentRenderers,
        IFileSystemProvider fileSystemProvider,
        IEnvironmentProvider environmentProvider,
        IClipboardService clipboardService)
    {
        _commonComponentRenderers = commonComponentRenderers;
        _fileSystemProvider = fileSystemProvider;
        _environmentProvider = environmentProvider;
        _clipboardService = clipboardService;
    }
    
    public MenuOptionRecord NewEmptyFile(
        IAbsoluteFilePath parentDirectory,
        Func<Task> onAfterCompletion)
    {
        return new MenuOptionRecord(
            "New Empty File",
            MenuOptionKind.Create,
            WidgetRendererType: _commonComponentRenderers.FileFormRendererType,
            WidgetParameters: new Dictionary<string, object?>
            {
                {
                    nameof(IFileFormRendererType.FileName),
                    string.Empty
                },
                {
                    nameof(IFileFormRendererType.CheckForTemplates),
                    false
                },
                {
                    nameof(IFileFormRendererType.OnAfterSubmitAction),
                    new Action<string, IFileTemplate?, ImmutableArray<IFileTemplate>>(
                        (fileName, exactMatchFileTemplate, relatedMatchFileTemplates) => 
                            PerformNewFileAction(
                                fileName, 
                                exactMatchFileTemplate, 
                                relatedMatchFileTemplates, 
                                new NamespacePath(
                                    string.Empty, 
                                    parentDirectory), 
                                onAfterCompletion))
                },
            });
    }

    public MenuOptionRecord NewTemplatedFile(
        NamespacePath parentDirectory,
        Func<Task> onAfterCompletion)
    {
        return new MenuOptionRecord(
            "New Templated File",
            MenuOptionKind.Create,
            WidgetRendererType: _commonComponentRenderers.FileFormRendererType,
            WidgetParameters: new Dictionary<string, object?>
            {
                {
                    nameof(IFileFormRendererType.FileName),
                    string.Empty
                },
                {
                    nameof(IFileFormRendererType.CheckForTemplates),
                    true
                },
                {
                    nameof(IFileFormRendererType.OnAfterSubmitAction),
                    new Action<string, IFileTemplate?, ImmutableArray<IFileTemplate>>(
                        (fileName, exactMatchFileTemplate, relatedMatchFileTemplates) => 
                            PerformNewFileAction(
                                fileName,
                                exactMatchFileTemplate,
                                relatedMatchFileTemplates,
                                parentDirectory,
                                onAfterCompletion))
                },
            });
    }
    
    public MenuOptionRecord NewDirectory(
        IAbsoluteFilePath parentDirectory,
        Func<Task> onAfterCompletion)
    {
        return new MenuOptionRecord(
            "New Directory",
            MenuOptionKind.Create,
            WidgetRendererType: _commonComponentRenderers.FileFormRendererType,
            WidgetParameters: new Dictionary<string, object?>
            {
                {
                    nameof(IFileFormRendererType.FileName),
                    string.Empty
                },
                {
                    nameof(IFileFormRendererType.IsDirectory),
                    true
                },
                {
                    nameof(IFileFormRendererType.OnAfterSubmitAction),
                    new Action<string, IFileTemplate?, ImmutableArray<IFileTemplate>>(
                        (directoryName, _, _) => 
                            PerformNewDirectoryAction(
                                directoryName,
                                parentDirectory,
                                onAfterCompletion))
                },
            });
    }

    public MenuOptionRecord DeleteFile(
        IAbsoluteFilePath absoluteFilePath, 
        Func<Task> onAfterCompletion)
    {
        return new MenuOptionRecord(
            "Delete",
            MenuOptionKind.Delete,
            WidgetRendererType: _commonComponentRenderers.DeleteFileFormRendererType,
            WidgetParameters: new Dictionary<string, object?>
            {
                {
                    nameof(IDeleteFileFormRendererType.AbsoluteFilePath),
                    absoluteFilePath
                },
                {
                    nameof(IDeleteFileFormRendererType.IsDirectory),
                    true
                },
                {
                    nameof(IDeleteFileFormRendererType.OnAfterSubmitAction),
                    new Action<IAbsoluteFilePath>(afp => 
                        PerformDeleteFileAction(afp, onAfterCompletion))
                },
            });
    }
    
    public MenuOptionRecord RenameFile(
        IAbsoluteFilePath sourceAbsoluteFilePath,
        IDispatcher dispatcher,
        Func<Task> onAfterCompletion)
    {
        return new MenuOptionRecord(
            "Rename",
            MenuOptionKind.Update,
            WidgetRendererType: _commonComponentRenderers.FileFormRendererType,
            WidgetParameters: new Dictionary<string, object?>
            {
                {
                    nameof(IFileFormRendererType.FileName),
                    sourceAbsoluteFilePath.IsDirectory
                        ? sourceAbsoluteFilePath.FileNameNoExtension
                        : sourceAbsoluteFilePath.FilenameWithExtension
                },
                {
                    nameof(IFileFormRendererType.IsDirectory),
                    sourceAbsoluteFilePath.IsDirectory
                },
                {
                    nameof(IFileFormRendererType.OnAfterSubmitAction),
                    new Action<string, IFileTemplate?, ImmutableArray<IFileTemplate>>(
                        (nextName, _, _) => 
                            PerformRenameAction(
                                sourceAbsoluteFilePath,
                                nextName,
                                dispatcher,
                                onAfterCompletion))
                },
            });
    }

    public MenuOptionRecord CopyFile(
        IAbsoluteFilePath absoluteFilePath,
        Func<Task> onAfterCompletion)
    {
        return new MenuOptionRecord(
            "Copy",
            MenuOptionKind.Update,
            OnClick: () => PerformCopyFileAction(
                absoluteFilePath, 
                onAfterCompletion));
    }
    
    public MenuOptionRecord CutFile(
        IAbsoluteFilePath absoluteFilePath,
        Func<Task> onAfterCompletion)
    {
        return new MenuOptionRecord(
            "Cut",
            MenuOptionKind.Update,
            OnClick: () => PerformCutFileAction(
                absoluteFilePath, 
                onAfterCompletion));
    }
    
    public MenuOptionRecord PasteClipboard(
        IAbsoluteFilePath directoryAbsoluteFilePath,
        Func<Task> onAfterCompletion)
    {
        return new MenuOptionRecord(
            "Paste",
            MenuOptionKind.Update,
            OnClick: () => PerformPasteFileAction(
                directoryAbsoluteFilePath, 
                onAfterCompletion));
    }

    public MenuOptionRecord RemoveCSharpProjectReferenceFromSolution(
        TreeViewNamespacePath? solutionNode,
        TreeViewNamespacePath projectNode,
        TerminalSession terminalSession,
        IDispatcher dispatcher,
        Func<Task> onAfterCompletion)
    {
        return new MenuOptionRecord(
            "Remove (no files are deleted)",
            MenuOptionKind.Delete,
            WidgetRendererType: _commonComponentRenderers.RemoveCSharpProjectFromSolutionRendererType,
            WidgetParameters: new Dictionary<string, object?>
            {
                {
                    nameof(IRemoveCSharpProjectFromSolutionRendererType.AbsoluteFilePath),
                    projectNode.Item?.AbsoluteFilePath
                },
                {
                    nameof(IDeleteFileFormRendererType.OnAfterSubmitAction),
                    new Action<IAbsoluteFilePath>(_ => 
                        PerformRemoveCSharpProjectReferenceFromSolutionAction(
                            solutionNode, 
                            projectNode,
                            terminalSession,
                            dispatcher,
                            onAfterCompletion))
                },
            });
    }

    public MenuOptionRecord AddProjectToProjectReference(
        TreeViewNamespacePath projectReceivingReference,
        TerminalSession terminalSession,
        IDispatcher dispatcher,
        Func<Task> onAfterCompletion)
    {
        return new MenuOptionRecord(
            "Add Project Reference",
            MenuOptionKind.Other,
            OnClick: () => PerformProjectToProjectReferenceAction(
                projectReceivingReference,
                terminalSession,
                dispatcher,
                onAfterCompletion));
    }

    private void PerformNewFileAction(
        string fileName,
        IFileTemplate? exactMatchFileTemplate,
        ImmutableArray<IFileTemplate> relatedMatchFileTemplates,
        NamespacePath namespacePath, 
        Func<Task> onAfterCompletion)
    {
        _ = Task.Run(async () =>
        {
            if (exactMatchFileTemplate is null)
            {
                var emptyFileAbsoluteFilePathString = namespacePath.AbsoluteFilePath
                                                          .GetAbsoluteFilePathString() +
                                                      fileName;

                var emptyFileAbsoluteFilePath = new AbsoluteFilePath(
                    emptyFileAbsoluteFilePathString, 
                    false,
                    _environmentProvider);
                
                await _fileSystemProvider.File.WriteAllTextAsync(
                    emptyFileAbsoluteFilePath.GetAbsoluteFilePathString(),
                    string.Empty,
                    CancellationToken.None); 
            }
            else
            {
                var allTemplates = new[] { exactMatchFileTemplate }
                    .Union(relatedMatchFileTemplates)
                    .ToArray();
                
                foreach (var fileTemplate in allTemplates)
                {
                    var templateResult = fileTemplate.ConstructFileContents.Invoke(
                        new FileTemplateParameter(
                            fileName,
                            namespacePath,
                            _environmentProvider));
                    
                    await _fileSystemProvider.File.WriteAllTextAsync(
                        templateResult.FileNamespacePath.AbsoluteFilePath
                            .GetAbsoluteFilePathString(),
                        templateResult.Contents,
                        CancellationToken.None); 
                }
            }

            await onAfterCompletion.Invoke();
        });
    }
    
    private void PerformNewDirectoryAction(
        string directoryName,
        IAbsoluteFilePath parentDirectory, 
        Func<Task> onAfterCompletion)
    {
        var directoryAbsoluteFilePathString = parentDirectory.GetAbsoluteFilePathString() +
                                              directoryName;

        var directoryAbsoluteFilePath = new AbsoluteFilePath(
            directoryAbsoluteFilePathString, 
            true,
            _environmentProvider);

        _ = Task.Run(async () =>
        {
            await _fileSystemProvider.Directory.CreateDirectoryAsync(
                directoryAbsoluteFilePath.GetAbsoluteFilePathString(),
                CancellationToken.None);

            await onAfterCompletion.Invoke();
        });
    }
    
    private void PerformDeleteFileAction(
        IAbsoluteFilePath absoluteFilePath, 
        Func<Task> onAfterCompletion)
    {
        _ = Task.Run(async () =>
        {
            if (absoluteFilePath.IsDirectory)
            {
                await _fileSystemProvider.Directory.DeleteAsync(
                    absoluteFilePath.GetAbsoluteFilePathString(),
                    true,
                    CancellationToken.None);    
            }
            else
            {
                await _fileSystemProvider.File.DeleteAsync(
                    absoluteFilePath.GetAbsoluteFilePathString());
            }

            await onAfterCompletion.Invoke();
        });
    }
    
    private void PerformCopyFileAction(
        IAbsoluteFilePath absoluteFilePath, 
        Func<Task> onAfterCompletion)
    {
        _ = Task.Run(async () =>
        { 
            await _clipboardService
                .SetClipboard(
                    ClipboardFacts.FormatPhrase(
                        ClipboardFacts.CopyCommand,
                        ClipboardFacts.AbsoluteFilePathDataType,
                        absoluteFilePath.GetAbsoluteFilePathString()));

            await onAfterCompletion.Invoke();
        });
    }
    
    private void PerformCutFileAction(
        IAbsoluteFilePath absoluteFilePath, 
        Func<Task> onAfterCompletion)
    {
        _ = Task.Run(async () =>
        { 
            await _clipboardService
                .SetClipboard(
                    ClipboardFacts.FormatPhrase(
                        ClipboardFacts.CutCommand,
                        ClipboardFacts.AbsoluteFilePathDataType,
                        absoluteFilePath.GetAbsoluteFilePathString()));

            await onAfterCompletion.Invoke();
        });
    }
    
    private void PerformPasteFileAction(
        IAbsoluteFilePath receivingDirectory, 
        Func<Task> onAfterCompletion)
    {
        _ = Task.Run(async () =>
        {
            var clipboardContents = await _clipboardService
                .ReadClipboard();

            if (ClipboardFacts.TryParseString(
                    clipboardContents, out var clipboardPhrase))
            {
                if (clipboardPhrase is not null &&
                    clipboardPhrase.DataType == ClipboardFacts.AbsoluteFilePathDataType)
                {
                    if (clipboardPhrase.Command == ClipboardFacts.CopyCommand ||
                        clipboardPhrase.Command == ClipboardFacts.CutCommand)
                    {

                        IAbsoluteFilePath? clipboardAbsoluteFilePath = null;

                        if (await _fileSystemProvider.Directory.ExistsAsync(clipboardPhrase.Value))
                        {
                            clipboardPhrase.Value = FilePathHelper.StripEndingDirectorySeparatorIfExists(
                                clipboardPhrase.Value,
                                _environmentProvider);
                            
                            clipboardAbsoluteFilePath = new AbsoluteFilePath(
                                clipboardPhrase.Value,
                                true,
                                _environmentProvider);
                        }
                        else if (await _fileSystemProvider.File.ExistsAsync(clipboardPhrase.Value))
                        {
                            clipboardAbsoluteFilePath = new AbsoluteFilePath(
                                clipboardPhrase.Value,
                                false,
                                _environmentProvider);
                        }

                        if (clipboardAbsoluteFilePath is not null)
                        {
                            var successfullyPasted = true;
                            
                            try
                            {
                                if (clipboardAbsoluteFilePath.IsDirectory)
                                {
                                    var clipboardDirectoryInfo =
                                        new DirectoryInfo(
                                            clipboardAbsoluteFilePath
                                                .GetAbsoluteFilePathString());
                                    
                                    var receivingDirectoryInfo =
                                        new DirectoryInfo(
                                            receivingDirectory
                                                .GetAbsoluteFilePathString());
                                    
                                    CopyFilesRecursively(
                                        clipboardDirectoryInfo,
                                        receivingDirectoryInfo);
                                }
                                else
                                {
                                    var destinationAbsoluteFilePathString = receivingDirectory.GetAbsoluteFilePathString() +
                                                              clipboardAbsoluteFilePath.FilenameWithExtension;
                                
                                    var sourceAbsoluteFilePathString = clipboardAbsoluteFilePath
                                        .GetAbsoluteFilePathString();

                                    await _fileSystemProvider.File.CopyAsync(
                                        sourceAbsoluteFilePathString,
                                        destinationAbsoluteFilePathString);
                                }
                            }
                            catch (Exception)
                            {
                                successfullyPasted = false; 
                            }

                            if (successfullyPasted &&
                                clipboardPhrase.Command == ClipboardFacts.CutCommand)
                            {
                                // TODO: Rerender the parent of the deleted due to cut file
                                PerformDeleteFileAction(
                                    clipboardAbsoluteFilePath,
                                    onAfterCompletion);    
                            }
                            else
                            {
                                await onAfterCompletion.Invoke();
                            }
                        }
                    }
                }
            }
        });
    }
    
    private IAbsoluteFilePath? PerformRenameAction(
        IAbsoluteFilePath sourceAbsoluteFilePath, 
        string nextName,
        IDispatcher dispatcher,
        Func<Task> onAfterCompletion)
    {
        // If the current and next name match when compared
        // with case insensitivity
        if (string.Compare(
                sourceAbsoluteFilePath.FilenameWithExtension, 
                nextName, 
                StringComparison.OrdinalIgnoreCase)
                    == 0)
        {
            var temporaryNextName = _environmentProvider.GetRandomFileName();
            
            var temporaryRenameResult = PerformRenameAction(
                sourceAbsoluteFilePath, 
                temporaryNextName, 
                dispatcher,
                () => Task.CompletedTask);

            if (temporaryRenameResult is null)
            {
                onAfterCompletion.Invoke();
                return null;
            }
            else
                sourceAbsoluteFilePath = temporaryRenameResult;
        }
        
        var sourceAbsoluteFilePathString = sourceAbsoluteFilePath.GetAbsoluteFilePathString();
        
        var parentOfSource = (IAbsoluteFilePath)sourceAbsoluteFilePath.Directories.Last();

        var destinationAbsoluteFilePathString = parentOfSource.GetAbsoluteFilePathString() +
                                  nextName;

        sourceAbsoluteFilePathString = FilePathHelper.StripEndingDirectorySeparatorIfExists(
            sourceAbsoluteFilePathString,
            _environmentProvider);
        
        destinationAbsoluteFilePathString = FilePathHelper.StripEndingDirectorySeparatorIfExists(
            destinationAbsoluteFilePathString,
            _environmentProvider);
        
        try
        {
            if (sourceAbsoluteFilePath.IsDirectory)
            {
                _fileSystemProvider.Directory.MoveAsync(
                    sourceAbsoluteFilePathString, 
                    destinationAbsoluteFilePathString);    
            }
            else
            {
                _fileSystemProvider.File.MoveAsync(
                    sourceAbsoluteFilePathString, 
                    destinationAbsoluteFilePathString);
            }
        }
        catch (Exception e)
        {
            var notificationError  = new NotificationRecord(
                NotificationKey.NewNotificationKey(), 
                "Rename Action",
                _commonComponentRenderers.ErrorNotificationRendererType,
                new Dictionary<string, object?>
                {
                    {
                        nameof(IErrorNotificationRendererType.Message), 
                        $"ERROR: {e.Message}"
                    },
                },
                TimeSpan.FromSeconds(15));
        
            dispatcher.Dispatch(
                new NotificationRecordsCollection.RegisterAction(
                    notificationError));
            
            onAfterCompletion.Invoke();
            return null;
        }

        onAfterCompletion.Invoke();
        
        return new AbsoluteFilePath(
            destinationAbsoluteFilePathString,
            sourceAbsoluteFilePath.IsDirectory,
            _environmentProvider);
    }
    
    private void PerformRemoveCSharpProjectReferenceFromSolutionAction(TreeViewNamespacePath? solutionNode,
        TreeViewNamespacePath? projectNode,
        TerminalSession terminalSession,
        IDispatcher dispatcher,
        Func<Task> onAfterCompletion)
    {
        _ = Task.Run(async () =>
        { 
            if (solutionNode?.Item is not null &&
                projectNode?.Item is not null)
            {
                var workingDirectory = (IAbsoluteFilePath)solutionNode
                    .Item.AbsoluteFilePath.Directories.Last();

                var removeCSharpProjectReferenceFromSolutionCommandString = 
                    DotNetCliFacts.FormatRemoveCSharpProjectReferenceFromSolutionAction(
                        solutionNode.Item.AbsoluteFilePath
                            .GetAbsoluteFilePathString(),
                        projectNode.Item.AbsoluteFilePath
                            .GetAbsoluteFilePathString());
            
                var removeCSharpProjectReferenceFromSolutionCommand = new TerminalCommand(
                    TerminalCommandKey.NewTerminalCommandKey(), 
                    removeCSharpProjectReferenceFromSolutionCommandString,
                    workingDirectory.GetAbsoluteFilePathString(),
                    CancellationToken.None,
                    () => Task.CompletedTask);
        
                await terminalSession
                    .EnqueueCommandAsync(
                        removeCSharpProjectReferenceFromSolutionCommand);
            }

            await onAfterCompletion.Invoke();
        });
    }
    
    public void PerformProjectToProjectReferenceAction(TreeViewNamespacePath projectReceivingReference,
        TerminalSession terminalSession,
        IDispatcher dispatcher,
        Func<Task> onAfterCompletion)
    {
        _ = Task.Run(async () =>
        {
            if (projectReceivingReference.Item is null)
            {
                await onAfterCompletion.Invoke();
                return;
            }

            var requestInputFileStateFormAction = new InputFileState.RequestInputFileStateFormAction(
                $"Add Project reference to {projectReceivingReference.Item.AbsoluteFilePath.FilenameWithExtension}",
                async referencedProject =>
                {
                    if (referencedProject is null)
                        return;

                    var interpolatedCommand = DotNetCliFacts
                        .FormatAddProjectToProjectReference(
                            projectReceivingReference.Item.AbsoluteFilePath.GetAbsoluteFilePathString(),
                            referencedProject.GetAbsoluteFilePathString());

                    var addExistingProjectToSolutionTerminalCommand = new TerminalCommand(
                        TerminalCommandKey.NewTerminalCommandKey(),
                        interpolatedCommand,
                        null,
                        CancellationToken.None,
                        async () =>
                        {
                            var notificationInformative = new NotificationRecord(
                                NotificationKey.NewNotificationKey(),
                                "Add Project Reference",
                                _commonComponentRenderers.InformativeNotificationRendererType,
                                new Dictionary<string, object?>
                                {
                                    {
                                        nameof(IInformativeNotificationRendererType.Message),
                                        $"Modified {projectReceivingReference.Item.AbsoluteFilePath.FilenameWithExtension}" +
                                        $" to have a reference to {referencedProject.FilenameWithExtension}"
                                    },
                                },
                                TimeSpan.FromSeconds(7));

                            dispatcher.Dispatch(
                                new NotificationRecordsCollection.RegisterAction(
                                    notificationInformative));

                            await onAfterCompletion.Invoke();
                        });

                    await terminalSession
                        .EnqueueCommandAsync(addExistingProjectToSolutionTerminalCommand);
                },
                afp =>
                {
                    if (afp is null ||
                        afp.IsDirectory)
                    {
                        return Task.FromResult(false);
                    }

                    return Task.FromResult(
                        afp.ExtensionNoPeriod
                            .EndsWith(ExtensionNoPeriodFacts.C_SHARP_PROJECT));
                },
                new[]
                {
                    new InputFilePattern(
                        "C# Project",
                        afp =>
                            afp.ExtensionNoPeriod
                                .EndsWith(ExtensionNoPeriodFacts.C_SHARP_PROJECT))
                }.ToImmutableArray());
        
            dispatcher.Dispatch(
                requestInputFileStateFormAction);
        });
    }
    
    /// <summary>
    /// Looking into copying and pasting a directory
    /// https://stackoverflow.com/questions/58744/copy-the-entire-contents-of-a-directory-in-c-sharp
    /// </summary>
    public static DirectoryInfo CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
    {
        var newDirectoryInfo = target.CreateSubdirectory(source.Name);
        foreach (var fileInfo in source.GetFiles())
            fileInfo.CopyTo(Path.Combine(newDirectoryInfo.FullName, fileInfo.Name));

        foreach (var childDirectoryInfo in source.GetDirectories())
            CopyFilesRecursively(childDirectoryInfo, newDirectoryInfo);

        return newDirectoryInfo;
    }
}