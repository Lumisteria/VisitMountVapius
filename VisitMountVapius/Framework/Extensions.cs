﻿using System.Diagnostics;
using System.Diagnostics.Contracts;

using Microsoft.Xna.Framework;

using StardewModdingAPI;

using StardewValley;
using StardewValley.TokenizableStrings;

namespace VisitMountVapius.Framework;

/// <summary>
/// Extension methods on strings.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Gets the Nth occurrence of a specific Unicode char in a string.
    /// </summary>
    /// <param name="str">String to search in.</param>
    /// <param name="item">Char to search for.</param>
    /// <param name="count">N.</param>
    /// <returns>Index of the char, or -1 if not found.</returns>
    [Pure]
    public static int NthOccurrenceOf(this string str, char item, int count = 1)
    {
        for (int i = 0; i < str.Length; i++)
        {
            if (str[i] == item && --count <= 0)
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Gets the Nth occurrence from the end of a specific Unicode char in a string.
    /// </summary>
    /// <param name="str">String to search in.</param>
    /// <param name="item">Char to search for.</param>
    /// <param name="count">N.</param>
    /// <returns>Index of the char, or -1 if not found.</returns>
    [Pure]
    public static int NthOccurrenceFromEnd(this string str, char item, int count = 1)
    {
        for (int i = str.Length - 1; i >= 0; i--)
        {
            if (str[i] == item && --count <= 0)
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Faster replacement for str.Split()[index];.
    /// </summary>
    /// <param name="str">String to search in.</param>
    /// <param name="deliminator">deliminator to use.</param>
    /// <param name="index">index of the chunk to get.</param>
    /// <returns>a readonlyspan char with the chunk, or an empty readonlyspan for failure.</returns>
    [Pure]
    public static ReadOnlySpan<char> GetNthChunk(this string str, char deliminator, int index = 0)
    {
        int start = 0;
        int ind = 0;
        while (index-- >= 0)
        {
            ind = str.IndexOf(deliminator, start);
            if (ind == -1)
            {
                // since we've previously decremented index, check against -1;
                // this means we're done.
                if (index == -1)
                {
                    return str.AsSpan()[start..];
                }

                // else, we've run out of entries
                // and return an empty span to mark as failure.
                return ReadOnlySpan<char>.Empty;
            }

            if (index > -1)
            {
                start = ind + 1;
            }
        }
        return str.AsSpan()[start..ind];
    }

    /// <summary>
    /// Faster replacement for str.Split()[index];.
    /// </summary>
    /// <param name="str">String to search in.</param>
    /// <param name="deliminators">deliminators to use.</param>
    /// <param name="index">index of the chunk to get.</param>
    /// <returns>a readonlyspan char with the chunk, or an empty readonlyspan for failure.</returns>
    /// <remarks>Inspired by the lovely Wren.</remarks>
    [Pure]
    public static ReadOnlySpan<char> GetNthChunk(this string str, char[] deliminators, int index = 0)
    {

        int start = 0;
        int ind = 0;
        while (index-- >= 0)
        {
            ind = str.IndexOfAny(deliminators, start);
            if (ind == -1)
            {
                // since we've previously decremented index, check against -1;
                // this means we're done.
                if (index == -1)
                {
                    return str.AsSpan()[start..];
                }

                // else, we've run out of entries
                // and return an empty span to mark as failure.
                return ReadOnlySpan<char>.Empty;
            }

            if (index > -1)
            {
                start = ind + 1;
            }
        }
        return str.AsSpan()[start..ind];
    }

    /// <summary>
    /// Gets the index of the next whitespace character.
    /// </summary>
    /// <param name="str">String to search in.</param>
    /// <returns>Index of the whitespace character, or -1 if not found.</returns>
    [Pure]
    public static int GetIndexOfWhiteSpace(this string str)
        => str.AsSpan().GetIndexOfWhiteSpace();

    /// <summary>
    /// Gets the index of the next whitespace character.
    /// </summary>
    /// <param name="chars">ReadOnlySpan to search in.</param>
    /// <returns>Index of the whitespace character, or -1 if not found.</returns>
    [Pure]
    public static int GetIndexOfWhiteSpace(this ReadOnlySpan<char> chars)
    {
        for (int i = 0; i < chars.Length; i++)
        {
            if (char.IsWhiteSpace(chars[i]))
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Gets the index of the last whitespace character.
    /// </summary>
    /// <param name="str">String to search in.</param>
    /// <returns>Index of the whitespace character, or -1 if not found.</returns>
    [Pure]
    public static int GetLastIndexOfWhiteSpace(this string str)
        => str.AsSpan().GetLastIndexOfWhiteSpace();

    /// <summary>
    /// Gets the index of the last whitespace character.
    /// </summary>
    /// <param name="chars">ReadOnlySpan to search in.</param>
    /// <returns>Index of the whitespace character, or -1 if not found.</returns>
    [Pure]
    public static int GetLastIndexOfWhiteSpace(this ReadOnlySpan<char> chars)
    {
        for (int i = chars.Length - 1; i >= 0; i--)
        {
            if (char.IsWhiteSpace(chars[i]))
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Tries to split once by a deliminator.
    /// </summary>
    /// <param name="str">Text to split.</param>
    /// <param name="deliminator">Deliminator to split by.</param>
    /// <param name="first">The part that precedes the deliminator, or the whole text if not found.</param>
    /// <param name="second">The part that is after the deliminator.</param>
    /// <returns>True if successful, false otherwise.</returns>
    [Pure]
    public static bool TrySplitOnce(this string str, char? deliminator, out ReadOnlySpan<char> first, out ReadOnlySpan<char> second)
    {
        ArgumentNullException.ThrowIfNull(str);
        return str.AsSpan().TrySplitOnce(deliminator, out first, out second);
    }

    /// <summary>
    /// Tries to split once by a deliminator.
    /// </summary>
    /// <param name="str">Text to split.</param>
    /// <param name="deliminator">Deliminator to split by.</param>
    /// <param name="first">The part that precedes the deliminator, or the whole text if not found.</param>
    /// <param name="second">The part that is after the deliminator.</param>
    /// <returns>True if successful, false otherwise.</returns>
    [Pure]
    public static bool TrySplitOnce(this ReadOnlySpan<char> str, char? deliminator, out ReadOnlySpan<char> first, out ReadOnlySpan<char> second)
    {
        int idx = deliminator is null ? str.GetIndexOfWhiteSpace() : str.IndexOf(deliminator.Value);

        if (idx < 0)
        {
            first = str;
            second = ReadOnlySpan<char>.Empty;
            return false;
        }

        first = str[..idx];
        second = str[(idx + 1)..];
        return true;
    }

    public static string? ParseTokens(this string? tokenized) => TokenParser.ParseText(tokenized);

    /// <summary>
    /// Logs an exception.
    /// </summary>
    /// <param name="monitor">Logging instance to use.</param>
    /// <param name="action">The current actions being taken when the exception happened.</param>
    /// <param name="ex">The exception.</param>
    [DebuggerHidden]
    public static void LogError(this IMonitor monitor, string action, Exception? ex)
    {
        monitor.Log($"Mod failed while {action}, see log for details.", LogLevel.Error);
        if (ex is not null)
        {
            monitor.Log(ex.ToString());
        }
    }

    /// <summary>
    /// Given a Rectangle area, clamps it to the current map.
    /// </summary>
    /// <param name="rectangle">rectangle.</param>
    /// <param name="location">map to clamp to.</param>
    /// <returns>clamped rectangle.</returns>
    internal static Rectangle ClampMap(this Rectangle rectangle, GameLocation location)
    {
        if (location?.Map?.GetLayer("Back") is not { } layer)
        {
            ModEntry.ModMonitor.LogOnce($"{location?.NameOrUniqueName ?? "Unknown Location"} appears to be missing 'back' layer.", LogLevel.Warn);
            return Rectangle.Empty;
        }
        else
        {
            if (rectangle.Width <= 0)
            {
                rectangle.Width = layer.LayerWidth - rectangle.X;
            }
            if (rectangle.Height <= 0)
            {
                rectangle.Height = layer.LayerHeight - rectangle.Y;
            }

            return new Rectangle()
            {
                X = Math.Clamp(rectangle.X, 0, layer.LayerWidth),
                Y = Math.Clamp(rectangle.Y, 0, layer.LayerHeight),
                Width = Math.Clamp(rectangle.Width, 0, layer.LayerHeight - rectangle.X),
                Height = Math.Clamp(rectangle.Height, 0, layer.LayerHeight - rectangle.Y),
            };
        }
    }

    public static Point GetRandomTile(this Rectangle rectangle)
        => new(Random.Shared.Next(rectangle.Left, rectangle.Right + 1), Random.Shared.Next(rectangle.Top, rectangle.Bottom + 1));
}