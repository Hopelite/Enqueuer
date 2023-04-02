using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Data.Extensions;

public static class EnumerableExtensions
{
    /// <summary>
    /// Returns up to <paramref name="size"/> amount of <typeparamref name="T"/> from the <paramref name="source"/>
    /// as if they were on a specified <paramref name="page"/> with the specified <paramref name="size"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown, if <paramref name="source"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown, if <paramref name="page"/> or <paramref name="size"/> are less than zero.</exception>
    public static IEnumerable<T> Paginate<T>(this IEnumerable<T> source, int page, int size)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (page <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(page), "Page number must be greater than zero.");
        }

        if (size <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(size), "Page size must be greater than zero.");
        }

        return source.Skip((page - 1) * size).Take(size);
    }

    public static InlineKeyboardMarkup BuildTableMarkup<T>(this IEnumerable<T> source, int columns, Func<T, InlineKeyboardButton> factory)
    {
        var sourceValues = source.ToList();

        var buttonsAtTheLastRow = sourceValues.Count % columns;
        var rowsTotal = sourceValues.Count / columns + buttonsAtTheLastRow;
        var valuesIndex = 0;

        var replyButtons = new InlineKeyboardButton[rowsTotal][];
        for (int i = 0; i < rowsTotal - 1; i++)
        {
            replyButtons[i] = new InlineKeyboardButton[columns];
            for (int j = 0; j < replyButtons[i].Length; j++, valuesIndex++)
            {
                replyButtons[i][j] = factory(sourceValues[valuesIndex]);
            }
        }

        buttonsAtTheLastRow = buttonsAtTheLastRow == 0 ? columns : buttonsAtTheLastRow;
        replyButtons[^1] = new InlineKeyboardButton[buttonsAtTheLastRow];
        for (int i = 0; i < buttonsAtTheLastRow; i++, valuesIndex++)
        {
            replyButtons[^1][i] = factory(sourceValues[valuesIndex]);
        }

        return replyButtons;
    }
}
