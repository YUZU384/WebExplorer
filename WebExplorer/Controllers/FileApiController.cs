using Microsoft.AspNetCore.Mvc;
using System.Net;
using WebExplorer.Models;
using WebExplorer.Services;

namespace WebExplorer.Controllers
{
    /// <summary>
    /// 文件 API 控制器
    /// </summary>
    [ApiController]
    [Route("api")]
    public class FileApiController : ControllerBase
    {
        private readonly FileService _fileService;
        private readonly UploadService _uploadService;
        private readonly DownloadService _downloadService;
        private readonly LogService _logger;

        public FileApiController(FileService fileService, UploadService uploadService, DownloadService downloadService, LogService logger)
        {
            _fileService = fileService;
            _uploadService = uploadService;
            _downloadService = downloadService;
            _logger = logger;
        }

        /// <summary>
        /// 安全解码并规范化路径参数
        /// </summary>
        private static string DecodePath(string rawPath)
        {
            // ASP.NET Core 模型绑定已自动做了一次 URL 解码，这里处理残余编码 + 规范化
            var decoded = WebUtility.UrlDecode(rawPath);
            // 统一为 Windows 反斜杠
            decoded = decoded.Replace('/', '\\');
            // 移除非法控制字符（ASCII 0x00-0x1F，保留 Tab）
            var cleaned = new string(decoded.Where(c => c >= 32 || c == '\t').ToArray());
            // 压缩连续反斜杠为单个（防止 \\ 变成非法路径段）
            while (cleaned.Contains("\\\\"))
                cleaned = cleaned.Replace("\\\\", "\\");
            return cleaned.Trim();
        }

        /// <summary>
        /// 获取目录内容
        /// GET /api/files?path=C:\Users
        /// </summary>
        [HttpGet("files")]
        public ActionResult<ApiResponse<DirectoryResponse>> GetFiles([FromQuery] string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return Ok(ApiResponse<DirectoryResponse>.Fail("路径参数不能为空"));

            var safePath = DecodePath(path);

            var result = _fileService.GetDirectoryContent(safePath);
            if (result == null)
                return Ok(ApiResponse<DirectoryResponse>.Fail("无法读取目录"));

            return Ok(ApiResponse<DirectoryResponse>.Ok(result));
        }

        /// <summary>
        /// 下载文件
        /// GET /api/download?path=C:\file.txt
        /// </summary>
        [HttpGet("download")]
        public async Task<IActionResult> DownloadFile([FromQuery] string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return BadRequest(ApiResponse<object>.Fail("路径参数不能为空"));

            var safePath = DecodePath(path);

            var (filePath, contentType, fileName, fileSize, error) = await _downloadService.GetFileInfoAsync(safePath);

            if (error != null || filePath == null)
                return BadRequest(ApiResponse<object>.Fail(error ?? "下载失败"));

            // RFC 5987: filename* 用于非 ASCII 文件名，filename 作为 ASCII 回退
            var encodedName = Uri.EscapeDataString(fileName ?? "download");
            var asciiFallback = string.IsNullOrEmpty(fileName) ? "download" :
                new string(fileName.Where(c => c < 128 && !"'\"\\;".Contains(c)).ToArray());
            if (string.IsNullOrWhiteSpace(asciiFallback)) asciiFallback = "download";
            Response.Headers.ContentDisposition =
                $"attachment; filename=\"{asciiFallback}\"; filename*=UTF-8''{encodedName}";

            // 使用 PhysicalFile 实现零拷贝传输，性能更优
            return PhysicalFile(filePath, contentType ?? "application/octet-stream", enableRangeProcessing: true);
        }

        /// <summary>
        /// 上传文件
        /// POST /api/upload
        /// Content-Type: multipart/form-data
        /// Fields: file, targetPath
        /// </summary>
        [HttpPost("upload")]
        public async Task<ActionResult<ApiResponse<object>>> UploadFile(IFormFile file, [FromForm] string targetPath)
        {
            if (file == null || file.Length == 0)
                return Ok(ApiResponse<object>.Fail("请选择要上传的文件"));

            var safeTargetPath = DecodePath(targetPath);
            var result = await _uploadService.UploadAsync(file, safeTargetPath);
            return Ok(result);
        }

        /// <summary>
        /// 删除文件或文件夹
        /// DELETE /api/delete
        /// Body: { "path": "C:\file.txt" }
        /// </summary>
        [HttpDelete("delete")]
        public ActionResult<ApiResponse<object>> DeleteItem([FromBody] DeleteRequest request)
        {
            if (request?.Path == null)
                return Ok(ApiResponse<object>.Fail("路径参数不能为空"));

            var success = _fileService.Delete(DecodePath(request.Path));
            if (!success)
                return Ok(ApiResponse<object>.Fail("删除失败"));

            return Ok(ApiResponse<object>.Ok(new { }, "删除成功"));
        }

        /// <summary>
        /// 创建新文件夹
        /// POST /api/newfolder
        /// Body: { "path": "C:\Users", "name": "NewFolder" }
        /// </summary>
        [HttpPost("newfolder")]
        public ActionResult<ApiResponse<object>> CreateFolder([FromBody] NewFolderRequest request)
        {
            if (request?.Path == null || string.IsNullOrWhiteSpace(request.Name))
                return Ok(ApiResponse<object>.Fail("参数不完整"));

            var success = _fileService.CreateFolder(DecodePath(request.Path), request.Name);
            if (!success)
                return Ok(ApiResponse<object>.Fail("创建文件夹失败"));

            return Ok(ApiResponse<object>.Ok(new { }, "文件夹创建成功"));
        }

        /// <summary>
        /// 获取所有驱动器列表
        /// GET /api/drives
        /// </summary>
        [HttpGet("drives")]
        public ActionResult<ApiResponse<List<DriveInfoModel>>> GetDrives()
        {
            var drives = _fileService.GetDrives();
            return Ok(ApiResponse<List<DriveInfoModel>>.Ok(drives));
        }

        /// <summary>
        /// 获取快速访问路径列表
        /// GET /api/quickaccess
        /// </summary>
        [HttpGet("quickaccess")]
        public ActionResult<ApiResponse<List<QuickAccessItem>>> GetQuickAccess()
        {
            var paths = _fileService.GetQuickAccessPaths();
            return Ok(ApiResponse<List<QuickAccessItem>>.Ok(paths));
        }
    }

    public class DeleteRequest
    {
        public string Path { get; set; } = string.Empty;
    }

    public class NewFolderRequest
    {
        public string Path { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
