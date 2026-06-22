using WebExplorer.Models;
using WebExplorer.Utils;

namespace WebExplorer.Services
{
    /// <summary>
    /// 上传处理服务
    /// </summary>
    public class UploadService
    {
        private readonly LogService _logger;

        public UploadService(LogService logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 处理文件上传
        /// </summary>
        public async Task<ApiResponse<object>> UploadAsync(IFormFile file, string targetPath, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!PathSecurity.IsValidPath(targetPath))
                {
                    _logger.Warn($"上传目标路径无效: {targetPath}");
                    return ApiResponse<object>.Fail("目标路径无效");
                }

                var safeTargetPath = PathSecurity.GetSafeFullPath(targetPath)!;
                if (!Directory.Exists(safeTargetPath))
                {
                    _logger.Warn($"目标目录不存在: {safeTargetPath}");
                    return ApiResponse<object>.Fail("目标目录不存在");
                }

                var fileName = Path.GetFileName(file.FileName);
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    return ApiResponse<object>.Fail("文件名为空");
                }

                var filePath = Path.Combine(safeTargetPath, fileName);

                // 如果文件已存在，添加时间戳避免覆盖
                if (File.Exists(filePath))
                {
                    var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                    var ext = Path.GetExtension(fileName);
                    fileName = $"{nameWithoutExt}_{DateTime.Now:yyyyMMddHHmmss}{ext}";
                    filePath = Path.Combine(safeTargetPath, fileName);
                }

                _logger.Info($"开始上传: {fileName} ({FormatFileSize(file.Length)}) 到 {safeTargetPath}");

                using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous | FileOptions.WriteThrough))
                {
                    await file.CopyToAsync(stream, cancellationToken);
                }

                _logger.Info($"上传完成: {filePath}");

                return ApiResponse<object>.Ok(new { filePath }, $"文件 {fileName} 上传成功");
            }
            catch (OperationCanceledException)
            {
                _logger.Warn("上传被取消");
                return ApiResponse<object>.Fail("上传被取消");
            }
            catch (Exception ex)
            {
                _logger.Error($"上传失败: {ex.Message}");
                return ApiResponse<object>.Fail($"上传失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 格式化文件大小
        /// </summary>
        public static string FormatFileSize(long bytes)
        {
            string[] sizes = ["B", "KB", "MB", "GB", "TB"];
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}
