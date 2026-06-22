namespace WebExplorer.Utils
{
    /// <summary>
    /// MIME 类型映射
    /// </summary>
    public static class MimeTypes
    {
        private static readonly Dictionary<string, string> MimeTypeMap = new()
        {
            { ".html", "text/html" },
            { ".htm", "text/html" },
            { ".css", "text/css" },
            { ".js", "application/javascript" },
            { ".json", "application/json" },
            { ".txt", "text/plain" },
            { ".xml", "application/xml" },
            { ".pdf", "application/pdf" },
            { ".zip", "application/zip" },
            { ".rar", "application/x-rar-compressed" },
            { ".7z", "application/x-7z-compressed" },
            { ".jpg", "image/jpeg" },
            { ".jpeg", "image/jpeg" },
            { ".png", "image/png" },
            { ".gif", "image/gif" },
            { ".bmp", "image/bmp" },
            { ".svg", "image/svg+xml" },
            { ".ico", "image/x-icon" },
            { ".webp", "image/webp" },
            { ".mp3", "audio/mpeg" },
            { ".wav", "audio/wav" },
            { ".mp4", "video/mp4" },
            { ".avi", "video/x-msvideo" },
            { ".mkv", "video/x-matroska" },
            { ".doc", "application/msword" },
            { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
            { ".xls", "application/vnd.ms-excel" },
            { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
            { ".ppt", "application/vnd.ms-powerpoint" },
            { ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
            { ".exe", "application/octet-stream" },
            { ".dll", "application/octet-stream" },
            { ".msi", "application/octet-stream" },
        };

        /// <summary>
        /// 根据文件扩展名获取 MIME 类型
        /// </summary>
        public static string GetMimeType(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            return MimeTypeMap.TryGetValue(ext, out var mimeType) ? mimeType : "application/octet-stream";
        }
    }
}
