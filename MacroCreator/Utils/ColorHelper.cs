using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MacroCreator.Utils;

public static partial class ColorHelper
{
    public static string ToHexString(this Color color, bool alpha = false)
    {
        if (alpha)
        {
            return $"{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
        }
        else
        {
            return $"{color.R:X2}{color.G:X2}{color.B:X2}";
        }
    }

    public static string ToHtmlColorString(this Color color, bool alpha)
    {
        return $"#{color.ToHexString(alpha)}";
    }

    public static string ExpressAsRgbColor(this Color color)
    {
        if (color.A != 255)
        {
            return $"ARGB({color.A},{color.R},{color.G},{color.B})";
        }
        return $"RGB({color.R},{color.G},{color.B})";
    }

    public static string ExpressAsRgbColor(this int argb)
    {
        return Color.FromArgb(argb).ExpressAsRgbColor();
    }

    /// <summary>
    /// 用于匹配十六进制颜色代码的正则表达式
    /// </summary>
    public static readonly Regex HexColorRegex = _HexColorRegex();
    /// <summary>
    /// 用于匹配RGB颜色表达式的正则表达式
    /// </summary>
    public static readonly Regex RgbColorRegex1 = _RgbColorRegex();
    /// <summary>
    /// 用于匹配RGB颜色表达式的正则表达式（空格分隔）
    /// </summary>
    public static readonly Regex RgbColorRegex2 = _RgbColorRegex2();
    /// <summary>
    /// 用于匹配ARGB颜色表达式的正则表达式
    /// </summary>
    public static readonly Regex ArgbColorRegex1 = _ArgbColorRegex();
    /// <summary>
    /// 用于匹配ARGB颜色表达式的正则表达式（空格分隔）
    /// </summary>
    public static readonly Regex ArgbColorRegex2 = _ArgbColorRegex2();

    public static bool TryParseExpressionColor(string input, out Color color)
    {
        color = Color.Empty;
        input = input.Trim().ToUpperInvariant();

        if (HexColorRegex.IsMatch(input))
        {
            try
            {
                color = ColorTranslator.FromHtml(input);
                return true;
            }
            catch
            {
                return false;
            }
        }
        else if (RgbColorRegex1.IsMatch(input) || RgbColorRegex2.IsMatch(input))
        {
            var match = RgbColorRegex1.IsMatch(input) ? RgbColorRegex1.Match(input) : RgbColorRegex2.Match(input);
            color = Color.FromArgb(
                Convert.ToInt32(match.Groups["r"].Value),
                Convert.ToInt32(match.Groups["g"].Value),
                Convert.ToInt32(match.Groups["b"].Value));
            return true;
        }
        else if (ArgbColorRegex1.IsMatch(input) || ArgbColorRegex2.IsMatch(input))
        {
            var match = ArgbColorRegex1.IsMatch(input) ? ArgbColorRegex1.Match(input) : ArgbColorRegex2.Match(input);
            color = Color.FromArgb(
                Convert.ToInt32(match.Groups["a"].Value),
                Convert.ToInt32(match.Groups["r"].Value),
                Convert.ToInt32(match.Groups["g"].Value),
                Convert.ToInt32(match.Groups["b"].Value));
            return true;
        }
        else if (int.TryParse(input, out int argb))
        {
            color = Color.FromArgb(argb);
            return true;
        }
        else
        {
            if (!Enum.TryParse<KnownColor>(input, true, out var knownColorEnum)) return false;

            if (Color.FromKnownColor(knownColorEnum) is Color knownColor && !knownColor.IsEmpty)
            {
                color = knownColor;
                return true;
            }
        }

        return false;
    }

    [GeneratedRegex(@"^#(?<hex>[0-9A-F]{6}|[0-9A-F]{8})$", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex _HexColorRegex(); // #RRGGBB or #AARRGGBB

    [GeneratedRegex(@"^RGB\(\s*(?<r>\d{1,3})\s*,\s*(?<g>\d{1,3})\s*,\s*(?<b>\d{1,3})\s*\)$", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex _RgbColorRegex(); // RGB(r, g, b)

    [GeneratedRegex(@"^RGB\(\s*(?<r>\d{1,3})\s+(?<g>\d{1,3})\s+(?<b>\d{1,3})\s*\)$", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex _RgbColorRegex2(); // RGB(r g b)

    [GeneratedRegex(@"^ARGB\(\s*(?<a>\d{1,3})\s*,\s*(?<r>\d{1,3})\s*,\s*(?<g>\d{1,3})\s*,\s*(?<b>\d{1,3})\s*\)$", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex _ArgbColorRegex(); // RGB(a, r, g, b)

    [GeneratedRegex(@"^ARGB\(\s*(?<a>\d{1,3})\s+(?<r>\d{1,3})\s+(?<g>\d{1,3})\s+(?<b>\d{1,3})\s*\)$", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex _ArgbColorRegex2(); // RGB(a r g b)
}
