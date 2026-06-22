using System.Text.Json;
using System.Net;
using System.Net.Sockets;

namespace WebExplorer.Services
{
    /// <summary>
    /// 应用配置模型
    /// </summary>
    public class AppSettings
    {
        public string FixedIP { get; set; } = "";
        public int Port { get; set; } = 80;
        public bool AutoStart { get; set; } = false;
        public bool ShowQRCode { get; set; } = true;
        public List<string> RecentIPs { get; set; } = new();
    }

    /// <summary>
    /// 设置持久化服务 - 使用 JSON 文件存储
    /// </summary>
    public class SettingsService : IDisposable
    {
        private readonly string _settingsPath;
        private AppSettings _settings = new();
        private readonly FileSystemWatcher? _watcher;
        private bool _disposed;
        private DateTime _lastSelfWriteUtc; // Save 时间戳，用于抑制 watcher 异步回环触发

        public AppSettings Current => _settings;

        public event Action<AppSettings>? OnSettingsChanged;

        public SettingsService()
        {
            var appDir = AppDomain.CurrentDomain.BaseDirectory;
            _settingsPath = Path.Combine(appDir, "webexplorer.settings.json");
            LoadSettings();

            try
            {
                if (File.Exists(_settingsPath))
                {
                    _watcher = new FileSystemWatcher(appDir, "webexplorer.settings.json")
                    {
                        NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
                        EnableRaisingEvents = true
                    };
                    _watcher.Changed += (s, e) =>
                    {
                        // 时间戳去抑制：Save 写入后 500ms 内的变更视为自身触发，跳过
                        if ((DateTime.UtcNow - _lastSelfWriteUtc).TotalMilliseconds < 500) return;
                        Thread.Sleep(100);
                        LoadSettings();
                    };
                }
            }
            catch { }
        }

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    var json = File.ReadAllText(_settingsPath);
                    var loaded = JsonSerializer.Deserialize<AppSettings>(json);
                    if (loaded != null) { _settings = loaded; OnSettingsChanged?.Invoke(_settings); return; }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载设置失败: {ex.Message}");
            }
            _settings = new AppSettings();
            OnSettingsChanged?.Invoke(_settings);
        }

        public void Save(AppSettings settings)
        {
            try
            {
                _settings = settings ?? throw new ArgumentNullException(nameof(settings));
                _lastSelfWriteUtc = DateTime.UtcNow; // 记录写入时间戳，watcher 异步回调据此跳过
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                var json = JsonSerializer.Serialize(settings, options);
                var tempPath = _settingsPath + ".tmp";
                File.WriteAllText(tempPath, json);
                File.Move(tempPath, _settingsPath, overwrite: true);
                OnSettingsChanged?.Invoke(_settings);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存设置失败: {ex.Message}");
                throw;
            }
        }

        public static List<string> GetAvailableIPs()
        {
            var ips = new List<string>();
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork))
                    ips.Add(ip.ToString());
            }
            catch { }
            if (ips.Count == 0) ips.Add("127.0.0.1");
            return ips;
        }

        public static bool IsValidIP(string ip) => System.Net.IPAddress.TryParse(ip, out _);

        /// <summary>
        /// 解析显示用地址：将 Kestrel 返回的 0.0.0.0 替换为本机实际可用 IP
        /// </summary>
        public static string ResolveDisplayAddress(string rawAddress, int port)
        {
            if (rawAddress.Contains("0.0.0.0"))
            {
                var realIp = GetAvailableIPs().FirstOrDefault(ip => ip != "127.0.0.1") ?? "localhost";
                return $"http://{realIp}:{port}";
            }
            return rawAddress;
        }

        public void Dispose()
        {
            if (!_disposed) { _watcher?.Dispose(); _disposed = true; }
        }
    }
}
