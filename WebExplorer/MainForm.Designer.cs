namespace WebExplorer
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblTitle = new Label();
            btnSettings = new Button();
            grpStatus = new GroupBox();
            btnStartStop = new Button();
            nudPort = new NumericUpDown();
            lblPortLabel = new Label();
            lblIPAddress = new Label();
            lblIPLabel = new Label();
            grpLog = new GroupBox();
            txtLog = new RichTextBox();
            notifyIcon = new NotifyIcon();
            cmsTray = new ContextMenuStrip();
            mnuTrayShow = new ToolStripMenuItem();
            mnuTrayRestart = new ToolStripMenuItem();
            mnuTraySeparator = new ToolStripSeparator();
            mnuTrayExit = new ToolStripMenuItem();
            grpStatus.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudPort).BeginInit();
            grpLog.SuspendLayout();
            cmsTray.SuspendLayout();
            SuspendLayout();
            //
            // lblTitle
            //
            lblTitle.Font = new Font("Microsoft Sans Serif", 18F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(0, 120, 212);
            lblTitle.Location = new Point(25, 10);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(200, 35);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "🌐 WebExplorer";
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            //
            // btnSettings
            //
            btnSettings.BackColor = Color.Transparent;
            btnSettings.Cursor = Cursors.Hand;
            btnSettings.FlatStyle = FlatStyle.Flat;
            btnSettings.Font = new Font("Microsoft Sans Serif", 9.5F);
            btnSettings.ForeColor = Color.FromArgb(100, 100, 100);
            btnSettings.Location = new Point(590, 10);
            btnSettings.Name = "btnSettings";
            btnSettings.Size = new Size(85, 35);
            btnSettings.TabIndex = 1;
            btnSettings.Text = "⚙ 设置";
            btnSettings.UseVisualStyleBackColor = false;
            //
            // grpStatus
            //
            grpStatus.Controls.Add(btnStartStop);
            grpStatus.Controls.Add(nudPort);
            grpStatus.Controls.Add(lblPortLabel);
            grpStatus.Controls.Add(lblIPAddress);
            grpStatus.Controls.Add(lblIPLabel);
            grpStatus.Font = new Font("Microsoft Sans Serif", 10F);
            grpStatus.Location = new Point(25, 55);
            grpStatus.Name = "grpStatus";
            grpStatus.Size = new Size(650, 75);
            grpStatus.TabIndex = 2;
            grpStatus.TabStop = false;
            grpStatus.Text = "  服务器状态  ";
            //
            // btnStartStop
            //
            btnStartStop.BackColor = Color.FromArgb(0, 120, 212);
            btnStartStop.Cursor = Cursors.Hand;
            btnStartStop.FlatStyle = FlatStyle.Flat;
            btnStartStop.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Bold);
            btnStartStop.ForeColor = Color.White;
            btnStartStop.Location = new Point(505, 20);
            btnStartStop.Name = "btnStartStop";
            btnStartStop.Size = new Size(130, 40);
            btnStartStop.TabIndex = 5;
            btnStartStop.Text = "▶ 启动服务";
            btnStartStop.UseVisualStyleBackColor = false;
            //
            // nudPort
            //
            nudPort.Font = new Font("Microsoft Sans Serif", 10F);
            nudPort.Location = new Point(395, 30);
            nudPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            nudPort.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudPort.Name = "nudPort";
            nudPort.Size = new Size(65, 23);
            nudPort.TabIndex = 3;
            nudPort.Value = new decimal(new int[] { 80, 0, 0, 0 });
            //
            // lblPortLabel
            //
            lblPortLabel.Font = new Font("Microsoft Sans Serif", 9F);
            lblPortLabel.Location = new Point(360, 30);
            lblPortLabel.Name = "lblPortLabel";
            lblPortLabel.Size = new Size(35, 20);
            lblPortLabel.TabIndex = 2;
            lblPortLabel.Text = "端口:";
            lblPortLabel.TextAlign = ContentAlignment.MiddleCenter;
            //
            // lblIPAddress
            //
            lblIPAddress.BackColor = Color.Transparent;
            lblIPAddress.Cursor = Cursors.Hand;
            lblIPAddress.Font = new Font("Consolas", 14F, FontStyle.Bold);
            lblIPAddress.ForeColor = Color.FromArgb(0, 120, 212);
            lblIPAddress.Location = new Point(80, 30);
            lblIPAddress.Name = "lblIPAddress";
            lblIPAddress.Size = new Size(260, 25);
            lblIPAddress.TabIndex = 1;
            lblIPAddress.Text = "未配置";
            lblIPAddress.TextAlign = ContentAlignment.MiddleLeft;
            //
            // lblIPLabel
            //
            lblIPLabel.Font = new Font("Microsoft Sans Serif", 9F);
            lblIPLabel.Location = new Point(15, 30);
            lblIPLabel.Name = "lblIPLabel";
            lblIPLabel.Size = new Size(60, 20);
            lblIPLabel.TabIndex = 0;
            lblIPLabel.Text = "访问地址:";
            lblIPLabel.TextAlign = ContentAlignment.MiddleCenter;
            //
            // grpLog
            //
            grpLog.Controls.Add(txtLog);
            grpLog.Font = new Font("Microsoft Sans Serif", 10F);
            grpLog.Location = new Point(25, 140);
            grpLog.Name = "grpLog";
            grpLog.Size = new Size(650, 450);
            grpLog.TabIndex = 3;
            grpLog.TabStop = false;
            grpLog.Text = "  运行日志  ";
            //
            // txtLog
            //
            txtLog.BackColor = Color.FromArgb(30, 30, 30);
            txtLog.Font = new Font("Consolas", 9.5F);
            txtLog.ForeColor = Color.FromArgb(220, 220, 220);
            txtLog.Location = new Point(15, 30);
            txtLog.Name = "txtLog";
            txtLog.ReadOnly = true;
            txtLog.ScrollBars = RichTextBoxScrollBars.Vertical;
            txtLog.ShortcutsEnabled = true;
            txtLog.Size = new Size(620, 415);
            txtLog.TabIndex = 0;
            txtLog.Text = "";
            //
            // notifyIcon - 系统托盘图标
            //
            notifyIcon.ContextMenuStrip = cmsTray;
            notifyIcon.Icon = SystemIcons.Application;
            notifyIcon.Text = "WebExplorer - 局域网文件传输工具";
            notifyIcon.Visible = true;
            notifyIcon.DoubleClick += notifyIcon_DoubleClick;
            //
            // cmsTray - 托盘右键菜单
            //
            cmsTray.Items.AddRange(new ToolStripItem[] { mnuTrayShow, mnuTrayRestart, mnuTraySeparator, mnuTrayExit });
            cmsTray.Name = "cmsTray";
            cmsTray.ShowImageMargin = true;
            cmsTray.Size = new Size(150, 76);
            //
            // mnuTrayShow
            //
            mnuTrayShow.Name = "mnuTrayShow";
            mnuTrayShow.ShortcutKeys = Keys.None;
            mnuTrayShow.Size = new Size(149, 22);
            mnuTrayShow.Text = "显示主窗口";
            mnuTrayShow.Click += mnuTrayShow_Click;
            //
            // mnuTrayRestart
            //
            mnuTrayRestart.Name = "mnuTrayRestart";
            mnuTrayRestart.ShortcutKeys = Keys.None;
            mnuTrayRestart.Size = new Size(149, 22);
            mnuTrayRestart.Text = "重启服务";
            mnuTrayRestart.Click += mnuTrayRestart_Click;
            //
            // mnuTraySeparator
            //
            mnuTraySeparator.Name = "mnuTraySeparator";
            mnuTraySeparator.Size = new Size(146, 6);
            //
            // mnuTrayExit
            //
            mnuTrayExit.Name = "mnuTrayExit";
            mnuTrayExit.ShortcutKeys = Keys.None;
            mnuTrayExit.Size = new Size(149, 22);
            mnuTrayExit.Text = "退出程序";
            mnuTrayExit.Click += mnuTrayExit_Click;
            //
            // MainForm
            //
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(243, 243, 243);
            ClientSize = new Size(700, 615);
            Controls.Add(grpLog);
            Controls.Add(grpStatus);
            Controls.Add(btnSettings);
            Controls.Add(lblTitle);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "MainForm";
            ShowInTaskbar = true;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "WebExplorer - 局域网文件传输工具";
            FormClosing += MainForm_FormClosing;
            grpStatus.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)nudPort).EndInit();
            grpLog.ResumeLayout(false);
            cmsTray.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnSettings;
        private System.Windows.Forms.GroupBox grpStatus;
        private System.Windows.Forms.Label lblIPLabel;
        private System.Windows.Forms.Label lblIPAddress;
        private System.Windows.Forms.Label lblPortLabel;
        private System.Windows.Forms.NumericUpDown nudPort;
        private System.Windows.Forms.Button btnStartStop;
        private System.Windows.Forms.GroupBox grpLog;
        public System.Windows.Forms.RichTextBox txtLog;
        public System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenuStrip cmsTray;
        private System.Windows.Forms.ToolStripMenuItem mnuTrayShow;
        private System.Windows.Forms.ToolStripMenuItem mnuTrayRestart;
        private System.Windows.Forms.ToolStripSeparator mnuTraySeparator;
        private System.Windows.Forms.ToolStripMenuItem mnuTrayExit;
    }
}
