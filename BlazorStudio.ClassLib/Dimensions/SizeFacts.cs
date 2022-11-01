﻿namespace BlazorStudio.ClassLib.Dimensions;

public static class SizeFacts
{
    public static class Bstudio
    {
        public static class Header
        {
            public static readonly DimensionUnit Height = new()
            {
                Value = 3,
                DimensionUnitKind = DimensionUnitKind.RootCharacterHeight
            };
        }
    }
}