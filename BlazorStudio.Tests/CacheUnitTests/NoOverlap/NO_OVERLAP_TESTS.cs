﻿using BlazorStudio.ClassLib.FileSystem.Classes;
using BlazorStudio.ClassLib.Store.PlainTextEditorCase;

namespace BlazorStudio.Tests.CacheUnitTests.NoOverlap;

/// <summary>
/// For these tests 'EAST_TEST' describes the position of the request relative to the existing chunk.
/// </summary>
public class NO_OVERLAP_TESTS : PLAIN_TEXT_EDITOR_STATES_TESTS
{
    [Fact]
    public void EAST_TEST()
    {
        var inputAbsoluteFilePath = new AbsoluteFilePath(AbsoluteFilePathToThisCSharpProject +
            "CacheTests\\THE TELL-TALE HEART.txt",
            false);

        var requestOne = new FileCoordinateGridRequest(
            0,
            30,
            0,
            17,
            CancellationToken.None);

        var requestTwo = new FileCoordinateGridRequest(
            5,
            5,
            25,
            20,
            CancellationToken.None);

        var expectedResultOne = File.ReadAllText(AbsoluteFilePathToThisCSharpProject +
                                                 "CacheTestsData\\NoOverlap\\East\\resultOne_NoOverlap_East.txt");

        var expectedResultTwo = File.ReadAllText(AbsoluteFilePathToThisCSharpProject +
                                                 "CacheTestsData\\NoOverlap\\East\\resultTwo_NoOverlap_East.txt");

        var plainTextEditorKey = PlainTextEditorKey.NewPlainTextEditorKey();

        Dispatcher.Dispatch(new ConstructMemoryMappedFilePlainTextEditorRecordAction(plainTextEditorKey,
            inputAbsoluteFilePath));

        var plainTextEditor = State.Value.Map[plainTextEditorKey];

        var actionOne = new MemoryMappedFileExactReadRequestAction(plainTextEditorKey,
            requestOne);

        Dispatcher.Dispatch(actionOne);

        var actualResultOne = plainTextEditor.GetPlainText();

        Assert.Equal(expectedResultOne, actualResultOne);

        var actionTwo = new MemoryMappedFileExactReadRequestAction(plainTextEditorKey,
            requestTwo);

        Dispatcher.Dispatch(actionTwo);

        var actualResultTwo = plainTextEditor.GetPlainText();

        Assert.Equal(expectedResultTwo, actualResultTwo);
    }

    // EAST_WEST_TEST() is impossible with 1 request it would overlap
    // 
    //[Fact]
    //public void EAST_WEST_TEST()

    [Fact]
    public void NORTH_TEST()
    {
        var inputAbsoluteFilePath = new AbsoluteFilePath(AbsoluteFilePathToThisCSharpProject +
                                                         "CacheTests\\THE TELL-TALE HEART.txt",
            false);

        var requestOne = new FileCoordinateGridRequest(
            10,
            30,
            20,
            5,
            CancellationToken.None);

        var requestTwo = new FileCoordinateGridRequest(
            1,
            2,
            30,
            5,
            CancellationToken.None);

        var expectedResultOne = File.ReadAllText(AbsoluteFilePathToThisCSharpProject +
                                                 "CacheTestsData\\NoOverlap\\North\\resultOne_NoOverlap_North.txt");

        var expectedResultTwo = File.ReadAllText(AbsoluteFilePathToThisCSharpProject +
                                                 "CacheTestsData\\NoOverlap\\North\\resultTwo_NoOverlap_North.txt");

        var plainTextEditorKey = PlainTextEditorKey.NewPlainTextEditorKey();

        Dispatcher.Dispatch(new ConstructMemoryMappedFilePlainTextEditorRecordAction(plainTextEditorKey,
            inputAbsoluteFilePath));

        var plainTextEditor = State.Value.Map[plainTextEditorKey];

        var actionOne = new MemoryMappedFileExactReadRequestAction(plainTextEditorKey,
            requestOne);

        Dispatcher.Dispatch(actionOne);

        var actualResultOne = plainTextEditor.GetPlainText();

        Assert.Equal(expectedResultOne, actualResultOne);

        var actionTwo = new MemoryMappedFileExactReadRequestAction(plainTextEditorKey,
            requestTwo);

        Dispatcher.Dispatch(actionTwo);

        var actualResultTwo = plainTextEditor.GetPlainText();

        Assert.Equal(expectedResultTwo, actualResultTwo);
    }
    
    [Fact]
    public void NORTH_EAST_TEST()
    {
        var inputAbsoluteFilePath = new AbsoluteFilePath(AbsoluteFilePathToThisCSharpProject +
                                                         "CacheTests\\THE TELL-TALE HEART.txt",
            false);

        var requestOne = new FileCoordinateGridRequest(
            10,
            30,
            20,
            5,
            CancellationToken.None);

        var requestTwo = new FileCoordinateGridRequest(
            1,
            2,
            30,
            5,
            CancellationToken.None);

        var expectedResultOne = File.ReadAllText(AbsoluteFilePathToThisCSharpProject +
                                                 "CacheTestsData\\NoOverlap\\NorthEast\\resultOne_NoOverlap_NorthEast.txt");

        var expectedResultTwo = File.ReadAllText(AbsoluteFilePathToThisCSharpProject +
                                                 "CacheTestsData\\NoOverlap\\NorthEast\\resultTwo_NoOverlap_NorthEast.txt");

        var plainTextEditorKey = PlainTextEditorKey.NewPlainTextEditorKey();

        Dispatcher.Dispatch(new ConstructMemoryMappedFilePlainTextEditorRecordAction(plainTextEditorKey,
            inputAbsoluteFilePath));

        var plainTextEditor = State.Value.Map[plainTextEditorKey];

        var actionOne = new MemoryMappedFileExactReadRequestAction(plainTextEditorKey,
            requestOne);

        Dispatcher.Dispatch(actionOne);

        var actualResultOne = plainTextEditor.GetPlainText();

        Assert.Equal(expectedResultOne, actualResultOne);

        var actionTwo = new MemoryMappedFileExactReadRequestAction(plainTextEditorKey,
            requestTwo);

        Dispatcher.Dispatch(actionTwo);

        var actualResultTwo = plainTextEditor.GetPlainText();

        Assert.Equal(expectedResultTwo, actualResultTwo);
    }

    // NORTH_SOUTH_TEST() is impossible with 1 request it would overlap
    // 
    //[Fact]
    //public void NORTH_SOUTH_TEST()
    
    [Fact]
    public void NORTH_WEST_TEST()
    {
        var inputAbsoluteFilePath = new AbsoluteFilePath(AbsoluteFilePathToThisCSharpProject +
                                                         "CacheTests\\THE TELL-TALE HEART.txt",
            false);

        var requestOne = new FileCoordinateGridRequest(
            10,
            30,
            20,
            5,
            CancellationToken.None);

        var requestTwo = new FileCoordinateGridRequest(
            1,
            2,
            5,
            5,
            CancellationToken.None);

        var expectedResultOne = File.ReadAllText(AbsoluteFilePathToThisCSharpProject +
                                                 "CacheTestsData\\NoOverlap\\NorthWest\\resultOne_NoOverlap_NorthWest.txt");

        var expectedResultTwo = File.ReadAllText(AbsoluteFilePathToThisCSharpProject +
                                                 "CacheTestsData\\NoOverlap\\NorthWest\\resultTwo_NoOverlap_NorthWest.txt");

        var plainTextEditorKey = PlainTextEditorKey.NewPlainTextEditorKey();

        Dispatcher.Dispatch(new ConstructMemoryMappedFilePlainTextEditorRecordAction(plainTextEditorKey,
            inputAbsoluteFilePath));

        var plainTextEditor = State.Value.Map[plainTextEditorKey];

        var actionOne = new MemoryMappedFileExactReadRequestAction(plainTextEditorKey,
            requestOne);

        Dispatcher.Dispatch(actionOne);

        var actualResultOne = plainTextEditor.GetPlainText();

        Assert.Equal(expectedResultOne, actualResultOne);

        var actionTwo = new MemoryMappedFileExactReadRequestAction(plainTextEditorKey,
            requestTwo);

        Dispatcher.Dispatch(actionTwo);

        var actualResultTwo = plainTextEditor.GetPlainText();

        Assert.Equal(expectedResultTwo, actualResultTwo);
    }
    
    [Fact]
    public void SOUTH_TEST()
    {
        var inputAbsoluteFilePath = new AbsoluteFilePath(AbsoluteFilePathToThisCSharpProject +
                                                         "CacheTests\\THE TELL-TALE HEART.txt",
            false);

        var requestOne = new FileCoordinateGridRequest(
            10,
            30,
            20,
            5,
            CancellationToken.None);

        var requestTwo = new FileCoordinateGridRequest(
            70,
            30,
            22,
            1,
            CancellationToken.None);

        var expectedResultOne = File.ReadAllText(AbsoluteFilePathToThisCSharpProject +
                                                 "CacheTestsData\\NoOverlap\\South\\resultOne_NoOverlap_South.txt");

        var expectedResultTwo = File.ReadAllText(AbsoluteFilePathToThisCSharpProject +
                                                 "CacheTestsData\\NoOverlap\\South\\resultTwo_NoOverlap_South.txt");

        var plainTextEditorKey = PlainTextEditorKey.NewPlainTextEditorKey();

        Dispatcher.Dispatch(new ConstructMemoryMappedFilePlainTextEditorRecordAction(plainTextEditorKey,
            inputAbsoluteFilePath));

        var plainTextEditor = State.Value.Map[plainTextEditorKey];

        var actionOne = new MemoryMappedFileExactReadRequestAction(plainTextEditorKey,
            requestOne);

        Dispatcher.Dispatch(actionOne);

        var actualResultOne = plainTextEditor.GetPlainText();

        Assert.Equal(expectedResultOne, actualResultOne);

        var actionTwo = new MemoryMappedFileExactReadRequestAction(plainTextEditorKey,
            requestTwo);

        Dispatcher.Dispatch(actionTwo);

        var actualResultTwo = plainTextEditor.GetPlainText();

        Assert.Equal(expectedResultTwo, actualResultTwo);
    }
    
    [Fact]
    public void SOUTH_EAST_TEST()
    {
        var inputAbsoluteFilePath = new AbsoluteFilePath(AbsoluteFilePathToThisCSharpProject +
                                                         "CacheTests\\THE TELL-TALE HEART.txt",
            false);

        var requestOne = new FileCoordinateGridRequest(
            10,
            30,
            20,
            5,
            CancellationToken.None);

        var requestTwo = new FileCoordinateGridRequest(
            70,
            30,
            35,
            1,
            CancellationToken.None);

        var expectedResultOne = File.ReadAllText(AbsoluteFilePathToThisCSharpProject +
                                                 "CacheTestsData\\NoOverlap\\SouthEast\\resultOne_NoOverlap_SouthEast.txt");

        var expectedResultTwo = File.ReadAllText(AbsoluteFilePathToThisCSharpProject +
                                                 "CacheTestsData\\NoOverlap\\SouthEast\\resultTwo_NoOverlap_SouthEast.txt");

        var plainTextEditorKey = PlainTextEditorKey.NewPlainTextEditorKey();

        Dispatcher.Dispatch(new ConstructMemoryMappedFilePlainTextEditorRecordAction(plainTextEditorKey,
            inputAbsoluteFilePath));

        var plainTextEditor = State.Value.Map[plainTextEditorKey];

        var actionOne = new MemoryMappedFileExactReadRequestAction(plainTextEditorKey,
            requestOne);

        Dispatcher.Dispatch(actionOne);

        var actualResultOne = plainTextEditor.GetPlainText();

        Assert.Equal(expectedResultOne, actualResultOne);

        var actionTwo = new MemoryMappedFileExactReadRequestAction(plainTextEditorKey,
            requestTwo);

        Dispatcher.Dispatch(actionTwo);

        var actualResultTwo = plainTextEditor.GetPlainText();

        Assert.Equal(expectedResultTwo, actualResultTwo);
    }
    
    [Fact]
    public void SOUTH_WEST_TEST()
    {
        var inputAbsoluteFilePath = new AbsoluteFilePath(AbsoluteFilePathToThisCSharpProject +
                                                         "CacheTests\\THE TELL-TALE HEART.txt",
            false);

        var requestOne = new FileCoordinateGridRequest(
            10,
            30,
            20,
            5,
            CancellationToken.None);

        var requestTwo = new FileCoordinateGridRequest(
            70,
            30,
            0,
            1,
            CancellationToken.None);

        var expectedResultOne = File.ReadAllText(AbsoluteFilePathToThisCSharpProject +
                                                 "CacheTestsData\\NoOverlap\\SouthWest\\resultOne_NoOverlap_SouthWest.txt");

        var expectedResultTwo = File.ReadAllText(AbsoluteFilePathToThisCSharpProject +
                                                 "CacheTestsData\\NoOverlap\\SouthWest\\resultTwo_NoOverlap_SouthWest.txt");

        var plainTextEditorKey = PlainTextEditorKey.NewPlainTextEditorKey();

        Dispatcher.Dispatch(new ConstructMemoryMappedFilePlainTextEditorRecordAction(plainTextEditorKey,
            inputAbsoluteFilePath));

        var plainTextEditor = State.Value.Map[plainTextEditorKey];

        var actionOne = new MemoryMappedFileExactReadRequestAction(plainTextEditorKey,
            requestOne);

        Dispatcher.Dispatch(actionOne);

        var actualResultOne = plainTextEditor.GetPlainText();

        Assert.Equal(expectedResultOne, actualResultOne);

        var actionTwo = new MemoryMappedFileExactReadRequestAction(plainTextEditorKey,
            requestTwo);

        Dispatcher.Dispatch(actionTwo);

        var actualResultTwo = plainTextEditor.GetPlainText();

        Assert.Equal(expectedResultTwo, actualResultTwo);
    }
    
    [Fact]
    public void WEST_TEST()
    {
        var inputAbsoluteFilePath = new AbsoluteFilePath(AbsoluteFilePathToThisCSharpProject +
                                                         "CacheTests\\THE TELL-TALE HEART.txt",
            false);

        var requestOne = new FileCoordinateGridRequest(
            10,
            30,
            20,
            5,
            CancellationToken.None);

        var requestTwo = new FileCoordinateGridRequest(
            11,
            10,
            0,
            63,
            CancellationToken.None);

        var expectedResultOne = File.ReadAllText(AbsoluteFilePathToThisCSharpProject +
                                                 "CacheTestsData\\NoOverlap\\West\\resultOne_NoOverlap_West.txt");

        var expectedResultTwo = File.ReadAllText(AbsoluteFilePathToThisCSharpProject +
                                                 "CacheTestsData\\NoOverlap\\West\\resultTwo_NoOverlap_West.txt");

        var plainTextEditorKey = PlainTextEditorKey.NewPlainTextEditorKey();

        Dispatcher.Dispatch(new ConstructMemoryMappedFilePlainTextEditorRecordAction(plainTextEditorKey,
            inputAbsoluteFilePath));

        var plainTextEditor = State.Value.Map[plainTextEditorKey];

        var actionOne = new MemoryMappedFileExactReadRequestAction(plainTextEditorKey,
            requestOne);

        Dispatcher.Dispatch(actionOne);

        var actualResultOne = plainTextEditor.GetPlainText();

        Assert.Equal(expectedResultOne, actualResultOne);

        var actionTwo = new MemoryMappedFileExactReadRequestAction(plainTextEditorKey,
            requestTwo);

        Dispatcher.Dispatch(actionTwo);

        var actualResultTwo = plainTextEditor.GetPlainText();

        Assert.Equal(expectedResultTwo, actualResultTwo);
    }
}