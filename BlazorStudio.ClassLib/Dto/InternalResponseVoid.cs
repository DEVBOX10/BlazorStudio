namespace BlazorStudio.ClassLib.Dto;

public class InternalResponseVoid
{
    public InternalResponseVoid(
        string message)
    {
        Message = message;
    }

    public string Message { get; }
}