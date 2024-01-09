using Enqueuer.Messaging.Core.Serialization;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Telegram.Callbacks.Helpers.Markup;

/// <summary>
/// Composes and builds the markup.
/// </summary>
internal interface IMarkupBuilder
{
    /// <summary>
    /// Adds <paramref name="markupButton"/> to the current row.
    /// </summary>
    IMarkupBuilder Add(CreateMarkupButtonDelegate createMarkupButton);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IMarkupBuilder FromNewRow(CreateMarkupButtonDelegate createMarkupButton);

    /// <summary>
    /// Builds the composed markup.
    /// </summary>
    InlineKeyboardMarkup Build();
}


internal delegate InlineKeyboardButton CreateMarkupButtonDelegate(ICallbackDataSerializer dataSerializer);
