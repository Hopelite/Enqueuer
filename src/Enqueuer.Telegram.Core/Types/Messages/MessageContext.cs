using System.Diagnostics.CodeAnalysis;
using Enqueuer.Telegram.Core.Extensions;
using Enqueuer.Telegram.Core.Types.Common;
using Newtonsoft.Json;
using Telegram.Bot.Types;

namespace Enqueuer.Telegram.Core.Types.Messages;

public class MessageContext
{
    [JsonIgnore]
    /// <summary>
    /// The <see cref="MessageType"/> of the Telegram message.
    /// </summary>
    public MessageType Type => Command == null
        ? MessageType.PlainText
        : MessageType.Command;

    /// <summary>
    /// Optional. The command specified in the message. 
    /// </summary>
    public CommandContext? Command { get; set; }

    /// <summary>
    /// The entire message text, including the command, if is specified.
    /// </summary>
    public string Text { get; set; } = null!;

    public static bool TryCreate(Message message, [NotNullWhen(returnValue: true)] out MessageContext? messageContext)
    {
        messageContext = null;
        if (message?.Text == null)
        {
            return false;
        }

        messageContext = new MessageContext
        {
            Text = message.Text,
        };

        if (message.Text.TryGetCommand(out CommandContext? command))
        {
            messageContext.Command = command;
        }

        return true;
    }
}
