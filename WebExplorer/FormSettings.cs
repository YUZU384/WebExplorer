using WebExplorer.Services;
using WebExplorer.Utils;

namespace WebExplorer
{
    public partial class FormSettings : Form
    {
        private readonly SettingsService _settingsService;
        private AppSettings _workingCopy;

        public FormSettings(SettingsService settingsService)
        {
            _settingsService = settingsService;
            InitializeComponent();

            // 设计模式下跳过数据加载，避免设计器崩溃
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
                return;

            _workingCopy = new AppSettings
            {
                FixedIP = settingsService.Current.FixedIP,
                Port = settingsService.Current.Port,
                AutoStart = settingsService.Current.AutoStart,
                ShowQRCode = settingsService.Current.ShowQRCode,
                RecentIPs = new List<string>(settingsService.Current.RecentIPs)
            };

            LoadData();
            BindEvents();
            UpdateQRCode();
            UpdateQRVisibility();
        }

        private void LoadData()
        {
            cboIP.Text = _workingCopy.FixedIP;
            nudPort.Value = _workingCopy.Port;
            chkAutoStart.Checked = _workingCopy.AutoStart;
            chkShowQR.Checked = _workingCopy.ShowQRCode;

            // 填充历史 IP（最近5次）
            foreach (var ip in _workingCopy.RecentIPs)
                if (!cboIP.Items.Contains(ip)) cboIP.Items.Add(ip);
        }

        private void BindEvents()
        {
            btnSave.Click += BtnSave_Click;
            btnReset.Click += BtnReset_Click;
        }

        /// <summary>
        /// 获取当前配置的访问 URL
        /// </summary>
        private string GetAccessUrl()
        {
            string ip;
            if (!string.IsNullOrWhiteSpace(cboIP.Text.Trim()))
                ip = cboIP.Text.Trim();
            else
            {
                var ips = SettingsService.GetAvailableIPs();
                ip = ips.FirstOrDefault(x => x != "127.0.0.1") ?? "localhost";
            }
            return $"http://{ip}:{(int)nudPort.Value}";
        }

        /// <summary>
        /// 更新二维码显示
        /// </summary>
        private void UpdateQRCode()
        {
            try
            {
                var url = GetAccessUrl();
                lblQRUrl.Text = url.Length > 30 ? url.Substring(0, 27) + "..." : url;

                // 生成 QR 码图像
                var qrImage = QRCodeGenerator.Generate(url, pixelSize: 5);
                picQRCode.Image?.Dispose();
                picQRCode.Image = qrImage;
            }
            catch (Exception ex)
            {
                picQRCode.Image?.Dispose();
                picQRCode.Image = null;
                lblQRUrl.Text = $"生成失败: {ex.Message}";
            }
        }

        /// <summary>
        /// 根据复选框控制二维码区域显示/隐藏
        /// </summary>
        private void UpdateQRVisibility()
        {
            grpQR.Visible = chkShowQR.Checked;
        }

        #region 事件处理

        private void chkShowQR_CheckedChanged(object? sender, EventArgs e)
        {
            UpdateQRVisibility();
        }

        private void nudPort_ValueChanged(object? sender, EventArgs e)
        {
            UpdateQRCode();
        }

        private void cboIP_TextChanged(object? sender, EventArgs e)
        {
            UpdateQRCode();
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            var ipText = cboIP.Text.Trim();
            if (!string.IsNullOrEmpty(ipText) && !SettingsService.IsValidIP(ipText))
            {
                MessageBox.Show("IP 格式不正确！\n\n请输入有效 IPv4，如 192.168.1.18\n或留空使用自动检测。", "格式错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboIP.Focus();
                return;
            }

            _workingCopy.FixedIP = ipText;
            _workingCopy.Port = (int)nudPort.Value;
            _workingCopy.AutoStart = chkAutoStart.Checked;
            _workingCopy.ShowQRCode = chkShowQR.Checked;

            // 记录历史 IP（最近5条）
            if (!string.IsNullOrEmpty(ipText) && SettingsService.IsValidIP(ipText))
            {
                _workingCopy.RecentIPs.Remove(ipText);
                _workingCopy.RecentIPs.Insert(0, ipText);
                if (_workingCopy.RecentIPs.Count > 5)
                    _workingCopy.RecentIPs = _workingCopy.RecentIPs.Take(5).ToList();
            }

            try
            {
                _settingsService.Save(_workingCopy);
                MessageBox.Show("设置已保存！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnReset_Click(object? sender, EventArgs e)
        {
            if (MessageBox.Show("恢复所有设置为默认值？", "确认",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _workingCopy = new AppSettings();
                LoadData();
                UpdateQRCode();
                UpdateQRVisibility();
            }
        }

        #endregion

        /// <summary>
        /// 关闭时释放二维码图片资源
        /// </summary>
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            picQRCode.Image?.Dispose();
        }
    }
}
