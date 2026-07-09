using WebExplorer.Utils;

namespace WebExplorer.Services
{
    /// <summary>
    /// 下载处理服务
    /// </summary>
    public class DownloadService
    {
        private readonly LogService _logger;

        public DownloadService(LogService logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取文件信息用于下载（使用 PhysicalFile 零拷贝传输）
        /// </summary>
        public async Task<(string? FilePath, string? ContentType, string? FileName, long FileSize, string? Error)> GetFileInfoAsync(string filePath)
        {
            try
            {
                if (!PathSecurity.IsValidPath(filePath))
                {
                    _logger.Warn($"下载路径无效: {filePath}");
                    return (null, null, null, 0, "文件路径无效");
                }

                var safePath = PathSecurity.GetSafeFullPath(filePath)!;

                if (!File.Exists(safePath))
                {
                    _logger.Warn($"下载文件不存在: {safePath}");
                    return (null, null, null, 0, "文件不存在");
                }

                var fileInfo = new FileInfo(safePath);
                var contentType = MimeTypes.GetMimeType(safePath);
                var fileName = fileInfo.Name;

                _logger.Info($"开始下载: {safePath} ({UploadService.FormatFileSize(fileInfo.Length)})");

                return (safePath, contentType, fileName, fileInfo.Length, null);
            }
            catch (Exception ex)
            {
                _logger.Error($"获取下载文件信息失败: {ex.Message}");
                return (null, null, null, 0, $"下载失败: {ex.Message}");
            }
        }
    }
}
