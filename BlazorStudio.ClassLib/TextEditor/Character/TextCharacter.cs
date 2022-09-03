namespace BlazorStudio.ClassLib.TextEditor.Character;

/// <summary>
/// I got the idea to have the characters as 'rich characters' instead
/// of Tokens from https://www.scintilla.org/ScintillaDoc.html.
/// <br/>
/// I may have misinterpreted their documentation but they seem
/// to use 2 bytes for each character. The first is the character
/// and the second is the decoration?
/// <br/>
/// They surely are doing it more correct than I am.
/// </summary>
public record TextCharacter(char Value)
{
    public byte DecorationByte { get; set; }
}