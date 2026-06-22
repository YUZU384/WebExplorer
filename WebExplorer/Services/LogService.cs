using WebExplorer.Utils;

namespace WebExplorer.Services
{
    /// <summary>
    /// 日志服务 - 输出到 WinForms RichTextBox（支持彩色文本）
    /// </summary>
    public class LogService : IDisposable
    {
        private readonly RichTextBox _logTextBox;
        private readonly object _lock = new();
        private bool _disposed;

        public LogService(RichTextBox logTextBox)
        {
            _logTextBox = logTextBox;
        }

        /// <summary>
        /// 记录信息日志（绿色）
        /// </summary>
        public void Info(string message)
        {
            Log(message, Color.FromArgb(0, 180, 0));
        }

        /// <summary>
        /// 记录警告日志（橙色）
        /// </summary>
        public void Warn(string message)
        {
            Log(message, Color.FromArgb(255, 165, 0));
        }

        /// <summary>
        /// 记录错误日志（红色）
        /// </summary>
        public void Error(string message)
        {
            Log(message, Color.FromArgb(220, 50, 50));
        }

        /// <summary>
        /// 记录调试日志（灰色）
        /// </summary>
        public void Debug(string message)
        {
            Log(message, Color.FromArgb(128, 128, 128));
        }

        /// <summary>
        /// 记录普通日志（默认颜色）
        /// </summary>
        public void Log(string message, Color? color = null)
        {
            if (_disposed) return;

            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            var logLine = $"[{timestamp}] {message}{Environment.NewLine}";

            if (_logTextBox.InvokeRequired)
            {
                _logTextBox.Invoke(() => AppendLog(logLine, color ?? _logTextBox.ForeColor));
            }
            else
            {
                AppendLog(logLine, color ?? _logTextBox.ForeColor);
            }
        }

        private void AppendLog(string text, Color color)
        {
            lock (_lock)
            {
                _logTextBox.SelectionStart = _logTextBox.TextLength;
                _logTextBox.SelectionLength = 0;
                _logTextBox.SelectionColor = color;
                _logTextBox.AppendText(text);
                _logTextBox.ScrollToCaret();

                // 限制日志长度，防止内存过大
                if (_logTextBox.TextLength > 100000)
                {
                    var excess = _logTextBox.TextLength - 80000;
                    _logTextBox.Select(0, excess);
                    _logTextBox.SelectedText = string.Empty;
                }
            }
        }

        public void Dispose()
        {
            _disposed = true;
        }
    }
}
