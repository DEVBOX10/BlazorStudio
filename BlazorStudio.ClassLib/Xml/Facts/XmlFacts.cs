﻿using System.Collections.Immutable;
using BlazorTextEditor.RazorLib.Analysis;

namespace BlazorStudio.ClassLib.Xml.Facts;

public static class XmlFacts
{
    public const char SPECIAL_HTML_TAG = '!';

    public const char OPEN_TAG_BEGINNING = '<';

    public const string OPEN_TAG_WITH_CHILD_CONTENT_ENDING = ">";
    public const string OPEN_TAG_SELF_CLOSING_ENDING = "/>";

    public const string CLOSE_TAG_WITH_CHILD_CONTENT_BEGINNING = "</";
    public const string CLOSE_TAG_WITH_CHILD_CONTENT_ENDING = ">";
    
    public const char SEPARATOR_FOR_ATTRIBUTE_NAME_AND_ATTRIBUTE_VALUE = '=';
    
    public const char ATTRIBUTE_VALUE_STARTING = '"';
    public const char ATTRIBUTE_VALUE_ENDING = '"';
    
    public const string COMMENT_TAG_BEGINNING = "<!--";
    public const string COMMENT_TAG_ENDING = "-->";

    public static readonly ImmutableArray<string> OPEN_TAG_ENDING_OPTIONS = new[]
    {
        OPEN_TAG_WITH_CHILD_CONTENT_ENDING,
        OPEN_TAG_SELF_CLOSING_ENDING,
    }.ToImmutableArray();

    public static readonly ImmutableArray<string> TAG_NAME_STOP_DELIMITERS = new[]
        {
            ParserFacts.END_OF_FILE.ToString(),
        }
        .Union(WhitespaceFacts.ALL.Select(x => x.ToString()))
        .Union(OPEN_TAG_ENDING_OPTIONS)
        .ToImmutableArray();
}