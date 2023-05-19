﻿namespace BlazorStudio.ClassLib.Parsing.C;

public partial class CLanguageFacts
{
    public class Types
    {
        public static readonly (string name, Type type) Int = ("int", typeof(int));
        public static readonly (string name, Type type) String = ("string", typeof(string));
        public static readonly (string name, Type type) Void = ("void", typeof(void));
    }
}