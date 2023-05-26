using System.Diagnostics.CodeAnalysis;
using Enqueuer.Telegram.Core.Extensions;
using Enqueuer.Telegram.Core.Types.Common;
using Newtonsoft.Json;
using Telegram.Bot.Types;
using User = Enqueuer.Telegram.Core.Types.Common.User;

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
    /// 
    /// </summary>
    public User Sender { get; set; } = null!;

    /// <summary>
    /// 
    /// </summary>
    public Group From { get; set; } = null!;

    /// <summary>
    /// The entire message text, including the command, if is specified.
    /// </summary>
    public string Text { get; set; } = null!;

    public static bool TryCreate(Message message, [NotNullWhen(returnValue: true)] out MessageContext? messageContext)
    {
        messageContext = null;
        if (message?.Text == null || message.From == null)
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

        messageContext.Sender = new User
        {
            Id = message.From.Id,
            FirstName = message.From.FirstName,
            LastName = message.From.LastName,
        };

        messageContext.From = new Group
        {
            Id = message.Chat.Id,
            Title = message.Chat.Title!
        };

        return true;
    }
}
