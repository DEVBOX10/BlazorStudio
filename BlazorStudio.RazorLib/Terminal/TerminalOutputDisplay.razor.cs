using System.Collections.Immutable;
using System.Text;
using BlazorCommon.RazorLib.ComponentRenderers;
using BlazorCommon.RazorLib.ComponentRenderers.Types;
using BlazorCommon.RazorLib.Keyboard;
using BlazorCommon.RazorLib.Notification;
using BlazorStudio.ClassLib.Html;
using BlazorStudio.ClassLib.State;
using BlazorStudio.ClassLib.Store.TerminalCase;
using BlazorTextEditor.RazorLib;
using BlazorTextEditor.RazorLib.Cursor;
using BlazorTextEditor.RazorLib.HelperComponents;
using BlazorTextEditor.RazorLib.Model;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorStudio.RazorLib.Terminal;

public partial class TerminalOutputDisplay : FluxorComponent
{
    [Inject]
    private IStateSelection<TerminalSessionsState, TerminalSession?> TerminalSessionsStateSelection { get; set; } = null!;
    [Inject]
    private IState<TerminalSessionWasModifiedState> TerminalSessionWasModifiedStateWrap { get; set; } = null!;
    // TODO: Don't inject TerminalSessionsStateWrap. It causes too many unnecessary re-renders
    [Inject]
    private IState<TerminalSessionsState> TerminalSessionsStateWrap { get; set; } = null!;
    [Inject]
    private ITextEditorService TextEditorService { get; set; } = null!;
    [Inject]
    private INotificationService NotificationService { get; set; } = null!;
    [Inject]
    private IBlazorCommonComponentRenderers BlazorCommonComponentRenderers { get; set; } = null!;

    /// <summary>
    /// <see cref="TerminalSessionKey"/> is used to narrow down the terminal
    /// session.
    /// </summary>
    [Parameter, EditorRequired]
    public TerminalSessionKey TerminalSessionKey { get; set; } = null!;
    /// <summary>
    /// <see cref="TerminalCommandKey"/> is used to narrow down even further
    /// to the output of a specific command that was executed in a specific
    /// terminal session.
    /// <br/><br/>
    /// Optional
    /// </summary>
    [Parameter]
    public TerminalCommandKey? TerminalCommandKey { get; set; }

    protected override void OnInitialized()
    {
        TerminalSessionsStateSelection
            .Select(x =>
            {
                if (x.TerminalSessionMap
                    .TryGetValue(TerminalSessionKey, out var terminalSession))
                {
                    return terminalSession;
                }

                return null;
            });

        base.OnInitialized();
    }
    
    protected override void OnAfterRender(bool firstRender)
    {
        var terminalSession = TerminalSessionsStateSelection.Value;

        if (terminalSession is not null)
        {
            var textEditorModel = TextEditorService.Model
                .ModelFindOrDefault(terminalSession.TextEditorModelKey);

            if (textEditorModel is null)
            {
                TextEditorService.Model.ModelRegisterTemplatedModel(
                    terminalSession.TextEditorModelKey,
                    WellKnownModelKind.TerminalGeneric,
                    terminalSession.TerminalSessionKey.DisplayName
                    ?? "__terminal-display-name-fallback__",
                    DateTime.UtcNow,
                    "TERMINAL",
                    string.Empty);
                
                TextEditorService.ViewModel.Register(
                    terminalSession.TextEditorViewModelKey,
                    terminalSession.TextEditorModelKey);
            }
        }
        
        base.OnAfterRender(firstRender);
    }

    private MarkupString GetAllTextEscaped(string value)
    {
        return (MarkupString)value
            .Replace("\r\n", "\\r\\n<br/>")
            .Replace("\r", "\\r<br/>")
            .Replace("\n", "\\n<br/>")
            .Replace("\t", "--->")
            .Replace(" ", "·");
    }
    
    private static MarkupString ParseHttpLinks(string input)
    {
        var outputBuilder = new StringBuilder();

        var indexOfHttp = input.IndexOf("http");

        if (indexOfHttp > 0)
        {
            var firstSubstring = input.Substring(0, indexOfHttp);

            var httpBuilder = new StringBuilder();

            var position = indexOfHttp;

            while (position < input.Length)
            {
                var currentCharacter = input[position++];

                if (currentCharacter == ' ') break;

                httpBuilder.Append(currentCharacter);
            }

            var aTag = $"<a href=\"{httpBuilder}\" target=\"_blank\">{httpBuilder}</a>";

            var result = firstSubstring.EscapeHtml()
                         + aTag;

            if (position != input.Length - 1) result += input.Substring(position);

            outputBuilder.Append(result + "<br />");
        }
        else
            outputBuilder.Append(input.EscapeHtml() + "<br />");

        return (MarkupString)(outputBuilder.ToString());
    }

    private async Task TextEditorAfterOnKeyDownAsync(
        TextEditorModel textEditor,
        ImmutableArray<TextEditorCursorSnapshot> cursorSnapshots,
        KeyboardEventArgs keyboardEventArgs,
        Func<TextEditorMenuKind, bool, Task> setTextEditorMenuKind)
    {
        if (keyboardEventArgs.Code == KeyboardKeyFacts.WhitespaceCodes.ENTER_CODE ||
            keyboardEventArgs.Code == KeyboardKeyFacts.WhitespaceCodes.CARRIAGE_RETURN_CODE)
        {
            var text = textEditor.GetAllText();
            textEditor.SetContent(string.Empty);

            var generalTerminalSession = TerminalSessionsStateWrap.Value.TerminalSessionMap[
                TerminalSessionFacts.GENERAL_TERMINAL_SESSION_KEY];
            
            var whitespace = new[]
            {
                KeyboardKeyFacts.WhitespaceCharacters.SPACE,
                KeyboardKeyFacts.WhitespaceCharacters.TAB,
                KeyboardKeyFacts.WhitespaceCharacters.NEW_LINE,
                KeyboardKeyFacts.WhitespaceCharacters.CARRIAGE_RETURN,
            };

            var indexOfFirstWordEndingExclusive = text.IndexOfAny(whitespace);

            var targetFileName = text.Substring(
                0,
                indexOfFirstWordEndingExclusive);

            if (targetFileName.StartsWith('.'))
            {
                targetFileName = (generalTerminalSession.WorkingDirectoryAbsoluteFilePathString ?? string.Empty) +
                                 targetFileName;
            }

            var arguments = text
                .Substring(indexOfFirstWordEndingExclusive + 1)
                .Split(whitespace)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToArray();

            var terminalCommand = new TerminalCommand(
                TerminalCommandKey.NewTerminalCommandKey(), 
                targetFileName,
                arguments,
                null,
                CancellationToken.None,
                () => Task.CompletedTask);
        
            await generalTerminalSession
                .EnqueueCommandAsync(terminalCommand);
        }
    }
}