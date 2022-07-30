﻿using BlazorStudio.ClassLib.FileSystem.Classes;
using BlazorStudio.ClassLib.FileSystemApi;

namespace BlazorStudio.Tests.FileHandleTests;

public class FILE_HANDLE_TESTS : PLAIN_TEXT_EDITOR_STATES_TESTS
{
    [Fact]
    public void READ_FROM_START()
    {
        var inputAbsoluteFilePath = new AbsoluteFilePath(
            "C:\\BlazorStudioTestGround\\TestFiles\\helloWorld-Normal.c",
            false);

        var fileSystemProvider = new FileSystemProvider();

        var fileHandle = fileSystemProvider.Open(inputAbsoluteFilePath);

        var characterLengthOfLongestRow = (Int32) Math.Min(Int32.MaxValue, fileHandle.PhysicalCharacterLengthOfLongestRow);
        
        var rowContent = fileHandle.Read(new FileHandleReadRequest(
            0, 0, 5, characterLengthOfLongestRow,
            CancellationToken.None));

        var content = string.Join(string.Empty, rowContent);

        Assert.Equal("#include <stdlib.h>\n#include <stdio.h>\n\nint main() {\n    printf(\"Hello World!\\n\");\n", content);

        fileHandle.Dispose();
    }
    
    [Fact]
    public void READ_FROM_RANDOM()
    {
        var inputAbsoluteFilePath = new AbsoluteFilePath(
            "C:\\BlazorStudioTestGround\\TestFiles\\helloWorld-Normal.c",
            false);

        var fileSystemProvider = new FileSystemProvider();

        var fileHandle = fileSystemProvider.Open(inputAbsoluteFilePath);

        var characterLengthOfLongestRow = (Int32) Math.Min(Int32.MaxValue, fileHandle.PhysicalCharacterLengthOfLongestRow);
        
        var rowContent = fileHandle.Read(new FileHandleReadRequest(
            1, 2, 1000, characterLengthOfLongestRow,
            CancellationToken.None));

        var content = string.Join(string.Empty, rowContent);

        Assert.Equal("nclude <stdio.h>\nt main() {\n  printf(\"Hello World!\\n\");\n  return 0;\n", content);

        fileHandle.Dispose();
    }
    
    [Fact]
    public void READ_THEN_INSERT_LINE_BEFORE_READ_AGAIN()
    {
        var inputAbsoluteFilePath = new AbsoluteFilePath(
            "C:\\BlazorStudioTestGround\\TestFiles\\helloWorld-Normal.c",
            false);

        var fileSystemProvider = new FileSystemProvider();

        var fileHandle = fileSystemProvider.Open(inputAbsoluteFilePath);

        var characterLengthOfLongestRow = (Int32) Math.Min(Int32.MaxValue, fileHandle.PhysicalCharacterLengthOfLongestRow);
        
        var firstReadRows = fileHandle.Read(new FileHandleReadRequest(1, 0, 2, characterLengthOfLongestRow, 
            CancellationToken.None));

        var firstReadContent = string.Join(string.Empty, firstReadRows);
        
        fileHandle.Edit.Insert(1, 0, "READ_THEN_INSERT_LINE_BEFORE_READ_AGAIN()");

        var secondReadRows = fileHandle.Read(new FileHandleReadRequest(1, 0, 2, characterLengthOfLongestRow,
            CancellationToken.None));

        var secondReadContent = string.Join(string.Empty, secondReadRows);

        fileHandle.Dispose();
    }
}