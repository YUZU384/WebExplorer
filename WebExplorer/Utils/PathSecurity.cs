namespace WebExplorer.Utils
{
    /// <summary>
    /// 路径安全验证工具
    /// </summary>
    public static class PathSecurity
    {
        /// <summary>
        /// 检查规范化路径是否包含路径穿越段（.. 或 .）
        /// 防御性检查：Path.GetFullPath 已会规范化并消除 .. 段，正常情况下不会命中；
        /// 保留此检查作为双重保险，防止未来替换 GetFullPath 实现时遗漏。
        /// </summary>
        private static bool HasTraversalSegments(string fullPath)
        {
            var segments = fullPath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return segments.Any(s => s == ".." || s == ".");
        }

        /// <summary>
        /// 验证路径安全性（含存在性检查）
        /// 注意：当前方法混合了安全验证与存在性检查两个职责，
        /// 若调用方仅需安全判断可用 GetSafeFullPath，若仅判断存在可用 File/Directory.Exists。
        /// </summary>
        public static bool IsValidPath(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            try
            {
                var fullPath = Path.GetFullPath(path);

                if (HasTraversalSegments(fullPath))
                    return false;

                // 检查路径是否存在
                return Directory.Exists(fullPath) || File.Exists(fullPath);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取安全的完整路径
        /// </summary>
        public static string? GetSafeFullPath(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            try
            {
                var fullPath = Path.GetFullPath(path);

                if (HasTraversalSegments(fullPath))
                    return null;

                return fullPath;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 检查路径是否在允许的根目录下
        /// </summary>
        public static bool IsPathUnderRoot(string path, string rootPath)
        {
            try
            {
                var fullPath = Path.GetFullPath(path);
                var fullRoot = Path.GetFullPath(rootPath);
                return fullPath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 确保路径以目录分隔符结尾
        /// </summary>
        public static string EnsureTrailingSeparator(string path)
        {
            if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()) &&
                !path.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
            {
                return path + Path.DirectorySeparatorChar;
            }
            return path;
        }
    }
}
