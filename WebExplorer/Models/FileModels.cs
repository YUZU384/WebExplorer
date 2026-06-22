namespace WebExplorer.Models
{
    /// <summary>
    /// 文件项信息
    /// </summary>
    public class FileItem
    {
        public string Name { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public bool IsDirectory { get; set; }
        public long Size { get; set; }
        public string LastModified { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
    }

    /// <summary>
    /// 目录内容响应
    /// </summary>
    public class DirectoryResponse
    {
        public string Path { get; set; } = string.Empty;
        public string? ParentPath { get; set; }
        public List<FileItem> Items { get; set; } = new();
        public long TotalSize { get; set; }
    }

    /// <summary>
    /// 驱动器信息
    /// </summary>
    public class DriveInfoModel
    {
        public string Letter { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public long TotalSpace { get; set; }
        public long FreeSpace { get; set; }
        public long UsedSpace { get; set; }
        public int DriveType { get; set; }
    }

    /// <summary>
    /// 快速访问路径
    /// </summary>
    public class QuickAccessItem
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
    }

    /// <summary>
    /// 统一 API 响应
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        public static ApiResponse<T> Ok(T data, string message = "操作成功")
        {
            return new ApiResponse<T> { Success = true, Message = message, Data = data };
        }

        public static ApiResponse<T> Fail(string message)
        {
            return new ApiResponse<T> { Success = false, Message = message };
        }
    }
}
