using System.Collections.Generic;
using WebExplorer.Models;
using WebExplorer.Utils;

namespace WebExplorer.Services
{
    /// <summary>
    /// 文件操作服务
    /// </summary>
    public class FileService
    {
        private readonly LogService _logger;

        public FileService(LogService logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取目录内容
        /// </summary>
        public DirectoryResponse? GetDirectoryContent(string path)
        {
            try
            {
                if (!PathSecurity.IsValidPath(path))
                {
                    _logger.Warn($"无效路径: {path}");
                    return null;
                }

                var fullPath = PathSecurity.GetSafeFullPath(path);
                if (fullPath == null || !Directory.Exists(fullPath))
                {
                    _logger.Warn($"目录不存在: {path}");
                    return null;
                }

                var dirInfo = new DirectoryInfo(fullPath);
                var response = new DirectoryResponse
                {
                    Path = fullPath,
                    ParentPath = dirInfo.Parent?.FullName
                };

                // 获取所有目录
                var directories = dirInfo.GetDirectories()
                    .OrderBy(d => d.Name)
                    .Select(d => new FileItem
                    {
                        Name = d.Name,
                        FullPath = d.FullName,
                        IsDirectory = true,
                        Size = 0,
                        LastModified = d.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        Extension = ""
                    });

                // 获取所有文件（ToList 物化，避免 AddRange + Sum 双重枚举）
                var files = dirInfo.GetFiles()
                    .OrderBy(f => f.Name)
                    .Select(f => new FileItem
                    {
                        Name = f.Name,
                        FullPath = f.FullName,
                        IsDirectory = false,
                        Size = f.Length,
                        LastModified = f.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        Extension = f.Extension.ToLowerInvariant()
                    })
                    .ToList();

                response.Items.AddRange(directories);
                response.Items.AddRange(files);
                response.TotalSize = files.Sum(f => f.Size);

                _logger.Debug($"读取目录: {fullPath} ({response.Items.Count} 项)");
                return response;
            }
            catch (Exception ex)
            {
                _logger.Error($"读取目录失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 创建新文件夹
        /// </summary>
        public bool CreateFolder(string parentPath, string folderName)
        {
            try
            {
                if (!PathSecurity.IsValidPath(parentPath))
                {
                    _logger.Warn($"无效父路径: {parentPath}");
                    return false;
                }

                // 验证文件夹名称合法性（缓存无效字符集，避免重复枚举）
                var invalidChars = new HashSet<char>(Path.GetInvalidFileNameChars());
                if (folderName.Any(c => invalidChars.Contains(c)))
                {
                    _logger.Warn($"非法文件夹名: {folderName}");
                    return false;
                }

                var fullPath = Path.Combine(parentPath, folderName);
                Directory.CreateDirectory(fullPath);

                _logger.Info($"创建文件夹: {fullPath}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"创建文件夹失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 删除文件或文件夹
        /// </summary>
        public bool Delete(string path)
        {
            try
            {
                if (!PathSecurity.IsValidPath(path))
                {
                    _logger.Warn($"无效路径: {path}");
                    return false;
                }

                var fullPath = PathSecurity.GetSafeFullPath(path)!;

                if (Directory.Exists(fullPath))
                {
                    Directory.Delete(fullPath, recursive: true);
                    _logger.Info($"删除文件夹: {fullPath}");
                }
                else if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.Info($"删除文件: {fullPath}");
                }
                else
                {
                    _logger.Warn($"路径不存在: {fullPath}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"删除失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取所有驱动器信息
        /// </summary>
        public List<DriveInfoModel> GetDrives()
        {
            try
            {
                return System.IO.DriveInfo.GetDrives()
                    .Where(d => d.IsReady)
                    .Select(d => new DriveInfoModel
                    {
                        Letter = d.Name.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                        Label = string.IsNullOrWhiteSpace(d.VolumeLabel) ? "本地磁盘" : d.VolumeLabel,
                        TotalSpace = d.TotalSize,
                        FreeSpace = d.AvailableFreeSpace,
                        UsedSpace = d.TotalSize - d.AvailableFreeSpace,
                        DriveType = (int)d.DriveType
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.Error($"获取驱动器列表失败: {ex.Message}");
                return new List<DriveInfoModel>();
            }
        }

        /// <summary>
        /// 获取快速访问路径
        /// </summary>
        public List<QuickAccessItem> GetQuickAccessPaths()
        {
            var items = new List<QuickAccessItem>();
            var userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            AddQuickAccess(items, "桌面", Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "desktop");
            AddQuickAccess(items, "文档", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "documents");
            AddQuickAccess(items, "下载", GetDownloadsPath(), "download");
            AddQuickAccess(items, "图片", Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "pictures");
            AddQuickAccess(items, "视频", Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), "videos");
            AddQuickAccess(items, "音乐", Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "music");

            return items;
        }

        private void AddQuickAccess(List<QuickAccessItem> items, string name, string path, string icon)
        {
            if (Directory.Exists(path))
            {
                items.Add(new QuickAccessItem { Name = name, Path = path, Icon = icon });
            }
        }

        /// <summary>
        /// 获取下载文件夹真实路径（通过注册表，支持用户重定向）
        /// </summary>
        private static string GetDownloadsPath()
        {
            try
            {
                // 读取注册表中 Downloads 已知文件夹的真实路径（支持重定向）
                using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders");
                var regPath = key?.GetValue("{374DE290-123F-4565-9164-39C4925E467B}") as string;
                if (!string.IsNullOrWhiteSpace(regPath))
                {
                    // 展开 %USER% 等环境变量
                    var expanded = Environment.ExpandEnvironmentVariables(regPath);
                    if (Directory.Exists(expanded)) return expanded;
                }
            }
            catch { }

            // 回退到默认路径
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        }
    }
}
