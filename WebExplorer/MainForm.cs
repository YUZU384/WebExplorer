using WebExplorer.Services;

namespace WebExplorer
{
    public partial class MainForm : Form
    {
        private HttpServerService? _server;
        private LogService? _logService;
        private SettingsService? _settingsService;

        /// <summary>
        /// 标记是否真正退出（托盘菜单的"退出程序"才设为 true）
        /// </summary>
        private bool _isExiting = false;

        public MainForm()
        {
            InitializeComponent();

            // 设计模式下跳过服务初始化，避免设计器崩溃
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
                return;

            InitServices();
            BindEvents();
            UpdateIPDisplay();

            // 绑定 Load 事件用于自动启动
            this.Load += MainForm_Load;
        }

        /// <summary>
        /// 初始化服务
        /// </summary>
        private void InitServices()
        {
            _logService = new LogService(txtLog);
            _settingsService = new SettingsService();
            _settingsService.OnSettingsChanged += OnSettingsChanged;
        }

        /// <summary>
        /// 绑定事件
        /// </summary>
        private void BindEvents()
        {
            btnSettings.Click += BtnSettings_Click;
            btnStartStop.Click += BtnStartStop_Click;
            nudPort.ValueChanged += (s, e) => UpdateIPDisplay();

            // IP 地址点击复制
            lblIPAddress.Click += (s, e) =>
            {
                var text = lblIPAddress.Text;
                if (text == "已复制到剪贴板" || text == "未配置" || text == "获取中...")
                    return;
                Clipboard.SetText(text);
                // 复制后短暂显示提示
                var original = text; // 捕获真实地址，避免提示期间重复点击复制到提示文字
                lblIPAddress.Text = "已复制到剪贴板";
                _ = Task.Run(async () =>
                {
                    await Task.Delay(1200);
                    if (InvokeRequired)
                        Invoke(() => { if (lblIPAddress.Text == "已复制到剪贴板") lblIPAddress.Text = original; });
                    else if (lblIPAddress.Text == "已复制到剪贴板")
                        lblIPAddress.Text = original;
                });
            };
        }

        /// <summary>
        /// 设置变更回调
        /// </summary>
        private void OnSettingsChanged(AppSettings settings)
        {
            if (InvokeRequired) { Invoke(() => OnSettingsChanged(settings)); return; }
            nudPort.Value = settings.Port;
            UpdateIPDisplay();
        }

        /// <summary>
        /// 更新 IP 地址显示
        /// </summary>
        private void UpdateIPDisplay()
        {
            if (_settingsService == null) return;

            var settings = _settingsService.Current;
            var port = (int)nudPort.Value;

            string displayIP;
            if (!string.IsNullOrWhiteSpace(settings.FixedIP))
                displayIP = settings.FixedIP.Trim();
            else
            {
                var ips = SettingsService.GetAvailableIPs();
                displayIP = ips.FirstOrDefault(ip => ip != "127.0.0.1") ?? "localhost";
            }

            lblIPAddress.Text = $"http://{displayIP}:{port}";
        }

        /// <summary>
        /// 窗口加载时检查是否需要自动启动服务
        /// </summary>
        private async void MainForm_Load(object? sender, EventArgs e)
        {
            if (_settingsService?.Current.AutoStart == true)
            {
                _logService?.Info("检测到自动启动配置，正在启动服务...");
                await Task.Delay(500); // 稍等窗口完全显示
                BtnStartStop_Click(sender, e);
            }
        }

        /// <summary>
        /// 设置按钮点击 - 打开设置窗口
        /// </summary>
        private void BtnSettings_Click(object? sender, EventArgs e)
        {
            using var settingsForm = new FormSettings(_settingsService!);
            if (settingsForm.ShowDialog(this) == DialogResult.OK)
            {
                nudPort.Value = _settingsService!.Current.Port;
                UpdateIPDisplay();
                _logService?.Info($"设置已更新: IP={_settingsService.Current.FixedIP}(空=自动), 端口={_settingsService.Current.Port}");
            }
        }

        /// <summary>
        /// 启动/停止按钮点击事件
        /// </summary>
        private async void BtnStartStop_Click(object? sender, EventArgs e)
        {
            if (_server?.IsRunning == true)
            {
                btnStartStop.Enabled = false;
                btnStartStop.Text = "⏹ 停止中...";
                await _server.StopAsync();

                btnStartStop.Text = "▶ 启动服务";
                btnStartStop.BackColor = Color.FromArgb(0, 120, 212);
                grpStatus.Text = "  服务器状态 - 已停止  ";
                btnStartStop.Enabled = true;

                // 更新托盘图标提示
                notifyIcon.Text = "WebExplorer - 服务已停止";
            }
            else
            {
                var port = (int)nudPort.Value;
                btnStartStop.Enabled = false;
                btnStartStop.Text = "⏳ 启动中...";

                _server = new HttpServerService(_logService!);
                _server.OnServerStarted += OnServerStarted;

                try
                {
                    await _server.StartAsync(port);
                    btnStartStop.Enabled = true;
                }
                catch (Exception ex)
                {
                    btnStartStop.Text = "▶ 启动服务";
                    btnStartStop.BackColor = Color.FromArgb(0, 120, 212);
                    grpStatus.Text = "  服务器状态 - 启动失败  ";
                    btnStartStop.Enabled = true;
                    _logService?.Error($"启动失败: {ex.Message}");
                }
            }
        }

        private void OnServerStarted(string address)
        {
            if (InvokeRequired) { Invoke(() => OnServerStarted(address)); return; }

            btnStartStop.Text = "⏹ 停止服务";
            btnStartStop.BackColor = Color.FromArgb(220, 50, 50);
            grpStatus.Text = "  服务器状态 - 运行中  ";

            var settings = _settingsService?.Current;
            if (!string.IsNullOrWhiteSpace(settings?.FixedIP))
                address = $"http://{settings.FixedIP.Trim()}:{settings.Port}";
            else
                address = SettingsService.ResolveDisplayAddress(address, (int)nudPort.Value);

            lblIPAddress.Text = address;
            _logService?.Info($"服务已启动: {address}");

            // 更新托盘图标提示
            notifyIcon.Text = $"WebExplorer - {address}";
        }

        #region 托盘图标交互

        /// <summary>
        /// 关闭窗口时：如果非真正退出，则最小化到托盘
        /// </summary>
        private async void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (!_isExiting)
            {
                // 点击 X 按钮或 Alt+F4 → 最小化到托盘，不退出
                e.Cancel = true;
                Hide();
            }
            else
            {
                // 真正退出 → 停止服务、释放资源
                if (_server?.IsRunning == true) await _server.StopAsync();
                _logService?.Dispose();
                _settingsService?.Dispose();
                _server?.Dispose();
                notifyIcon.Visible = false;
                notifyIcon.Dispose();
            }
        }

        /// <summary>
        /// 双击托盘图标 → 显示主窗口
        /// </summary>
        private void notifyIcon_DoubleClick(object? sender, EventArgs e)
        {
            ShowWindowFromTray();
        }

        /// <summary>
        /// 托盘菜单 - 显示主窗口
        /// </summary>
        private void mnuTrayShow_Click(object? sender, EventArgs e)
        {
            ShowWindowFromTray();
        }

        /// <summary>
        /// 从托盘恢复窗口显示
        /// </summary>
        private void ShowWindowFromTray()
        {
            Show();
            WindowState = FormWindowState.Normal;
            Activate();
            BringToFront();
        }

        /// <summary>
        /// 托盘菜单 - 退出程序（真正退出）
        /// </summary>
        private async void mnuTrayExit_Click(object? sender, EventArgs e)
        {
            _isExiting = true;

            // 先停止服务
            if (_server?.IsRunning == true)
            {
                _logService?.Info("正在停止服务...");
                await _server.StopAsync();
            }

            Application.Exit();
        }

        /// <summary>
        /// 托盘菜单 - 重启服务
        /// </summary>
        private async void mnuTrayRestart_Click(object? sender, EventArgs e)
        {
            _logService?.Info("正在重启服务...");

            // 先停止
            if (_server?.IsRunning == true)
            {
                await _server.StopAsync();
                await Task.Delay(300);
            }

            // 再启动
            var port = (int)nudPort.Value;
            _server = new HttpServerService(_logService!);
            _server.OnServerStarted += OnServerStarted;
            await _server.StartAsync(port);
        }

        #endregion
    }
}
