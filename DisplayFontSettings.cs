using System;
using System.Drawing;

namespace MarkupEditor;

/// <summary>
/// Font appearance settings for one displayed text element.
/// </summary>
internal sealed class DisplayFontSettings
{
    /// <summary>
    /// Initializes a font appearance configuration.
    /// </summary>
    /// <param name="fontFamily">The selected font family name.</param>
    /// <param name="fontColor">The selected text color.</param>
    /// <param name="fontSize">The font size in points.</param>
    internal DisplayFontSettings(String fontFamily, Color fontColor, Single fontSize)
    {
        FontFamily = fontFamily;
        FontColor = fontColor;
        FontSize = fontSize;
    }

    /// <summary>Gets the selected font family name.</summary>
    internal String FontFamily { get; }

    /// <summary>Gets the selected text color.</summary>
    internal Color FontColor { get; }

    /// <summary>Gets the font size in points.</summary>
    internal Single FontSize { get; }
}