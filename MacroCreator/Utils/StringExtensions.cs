using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacroCreator.Utils;

public static class StringExtensions
{
    public static string Ellipsize(this string str, int maxLength)
    {
        if (str.Length <= maxLength) return str;
        if (maxLength <= 3) return new string('.', maxLength);
        return string.Concat(str.AsSpan(0, maxLength - 3), "...");
    }
}
