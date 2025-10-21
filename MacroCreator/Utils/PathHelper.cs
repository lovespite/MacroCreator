using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacroCreator.Utils;

public static class PathHelper
{
    public static bool IsValidFilePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;
        // 检查路径中是否包含任何无效字符
        char[] invalidChars = Path.GetInvalidPathChars();
        if (path.IndexOfAny(invalidChars) >= 0)
            return false;
        // 进一步检查文件名部分是否有效
        string fileName = Path.GetFileName(path);
        char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
        if (fileName.IndexOfAny(invalidFileNameChars) >= 0)
            return false;
        return true;
    }
}
