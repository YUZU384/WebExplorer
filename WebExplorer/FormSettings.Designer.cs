namespace WebExplorer
{
    partial class FormSettings
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

        private void InitializeComponent()
        {
            lblTitle = new Label();
            grpNetwork = new GroupBox();
            chkShowQR = new CheckBox();
            chkAutoStart = new CheckBox();
            nudPort = new NumericUpDown();
            cboIP = new ComboBox();
            lblPort = new Label();
            lblIP = new Label();
            btnSave = new Button();
            btnReset = new Button();
            btnCancel = new Button();
            lblHint = new Label();
            grpQR = new GroupBox();
            picQRCode = new PictureBox();
            lblQRUrl = new Label();
            grpNetwork.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudPort).BeginInit();
            grpQR.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picQRCode).BeginInit();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Microsoft Sans Serif", 16F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(0, 120, 212);
            lblTitle.Location = new Point(20, 15);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(137, 26);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "⚙ 网络设置";
            // 
            // grpNetwork
            // 
            grpNetwork.Controls.Add(chkShowQR);
            grpNetwork.Controls.Add(chkAutoStart);
            grpNetwork.Controls.Add(nudPort);
            grpNetwork.Controls.Add(cboIP);
            grpNetwork.Controls.Add(lblPort);
            grpNetwork.Controls.Add(lblIP);
            grpNetwork.Font = new Font("Microsoft Sans Serif", 10F);
            grpNetwork.Location = new Point(20, 55);
            grpNetwork.Name = "grpNetwork";
            grpNetwork.Size = new Size(310, 175);
            grpNetwork.TabIndex = 1;
            grpNetwork.TabStop = false;
            grpNetwork.Text = "  网络配置  ";
            // 
            // chkShowQR
            // 
            chkShowQR.AutoSize = true;
            chkShowQR.Checked = true;
            chkShowQR.CheckState = CheckState.Checked;
            chkShowQR.Font = new Font("Microsoft Sans Serif", 9.5F);
            chkShowQR.Location = new Point(16, 145);
            chkShowQR.Name = "chkShowQR";
            chkShowQR.Size = new Size(251, 20);
            chkShowQR.TabIndex = 5;
            chkShowQR.Text = "显示访问二维码（方便手机扫码）";
            chkShowQR.CheckedChanged += chkShowQR_CheckedChanged;
            // 
            // chkAutoStart
            // 
            chkAutoStart.AutoSize = true;
            chkAutoStart.Font = new Font("Microsoft Sans Serif", 9.5F);
            chkAutoStart.Location = new Point(16, 112);
            chkAutoStart.Name = "chkAutoStart";
            chkAutoStart.Size = new Size(191, 20);
            chkAutoStart.TabIndex = 4;
            chkAutoStart.Text = "程序启动时自动开启服务";
            // 
            // nudPort
            // 
            nudPort.Font = new Font("Consolas", 11F);
            nudPort.Location = new Point(70, 68);
            nudPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            nudPort.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudPort.Name = "nudPort";
            nudPort.Size = new Size(120, 25);
            nudPort.TabIndex = 3;
            nudPort.Value = new decimal(new int[] { 80, 0, 0, 0 });
            nudPort.ValueChanged += nudPort_ValueChanged;
            // 
            // cboIP
            // 
            cboIP.AutoCompleteSource = AutoCompleteSource.CustomSource;
            cboIP.DropDownHeight = 100;
            cboIP.Font = new Font("Consolas", 11F);
            cboIP.FormattingEnabled = true;
            cboIP.IntegralHeight = false;
            cboIP.Location = new Point(70, 28);
            cboIP.Name = "cboIP";
            cboIP.Size = new Size(220, 26);
            cboIP.TabIndex = 2;
            cboIP.TextChanged += cboIP_TextChanged;
            // 
            // lblPort
            // 
            lblPort.AutoSize = true;
            lblPort.Font = new Font("Microsoft Sans Serif", 9F);
            lblPort.Location = new Point(16, 72);
            lblPort.Name = "lblPort";
            lblPort.Size = new Size(34, 15);
            lblPort.TabIndex = 1;
            lblPort.Text = "端口:";
            // 
            // lblIP
            // 
            lblIP.AutoSize = true;
            lblIP.Font = new Font("Microsoft Sans Serif", 9F);
            lblIP.Location = new Point(16, 32);
            lblIP.Name = "lblIP";
            lblIP.Size = new Size(48, 15);
            lblIP.TabIndex = 0;
            lblIP.Text = "固定 IP:";
            // 
            // btnSave
            // 
            btnSave.BackColor = Color.FromArgb(0, 120, 212);
            btnSave.Cursor = Cursors.Hand;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.Font = new Font("Microsoft Sans Serif", 9.5F, FontStyle.Bold);
            btnSave.ForeColor = Color.White;
            btnSave.Location = new Point(230, 236);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(100, 39);
            btnSave.TabIndex = 2;
            btnSave.Text = "保存设置";
            btnSave.UseVisualStyleBackColor = false;
            // 
            // btnReset
            // 
            btnReset.Cursor = Cursors.Hand;
            btnReset.FlatStyle = FlatStyle.Flat;
            btnReset.Font = new Font("Microsoft Sans Serif", 9.5F);
            btnReset.Location = new Point(20, 236);
            btnReset.Name = "btnReset";
            btnReset.Size = new Size(90, 39);
            btnReset.TabIndex = 3;
            btnReset.Text = "恢复默认";
            // 
            // btnCancel
            // 
            btnCancel.Cursor = Cursors.Hand;
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Font = new Font("Microsoft Sans Serif", 9.5F);
            btnCancel.Location = new Point(125, 236);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(90, 39);
            btnCancel.TabIndex = 4;
            btnCancel.Text = "取消";
            // 
            // lblHint
            // 
            lblHint.Font = new Font("Microsoft Sans Serif", 8.5F);
            lblHint.ForeColor = Color.FromArgb(100, 100, 100);
            lblHint.Location = new Point(20, 290);
            lblHint.Name = "lblHint";
            lblHint.Size = new Size(520, 20);
            lblHint.TabIndex = 6;
            lblHint.Text = "💡 设置固定 IP 后，手机浏览器直接输入该地址即可访问   例如：http://192.168.1.18";
            lblHint.TextAlign = ContentAlignment.TopCenter;
            // 
            // grpQR
            // 
            grpQR.Controls.Add(picQRCode);
            grpQR.Controls.Add(lblQRUrl);
            grpQR.Font = new Font("Microsoft Sans Serif", 10F);
            grpQR.Location = new Point(340, 55);
            grpQR.Name = "grpQR";
            grpQR.Size = new Size(200, 220);
            grpQR.TabIndex = 7;
            grpQR.TabStop = false;
            grpQR.Text = "  访问二维码  ";
            // 
            // picQRCode
            // 
            picQRCode.BackColor = Color.White;
            picQRCode.BorderStyle = BorderStyle.FixedSingle;
            picQRCode.Location = new Point(35, 30);
            picQRCode.Name = "picQRCode";
            picQRCode.Size = new Size(130, 130);
            picQRCode.SizeMode = PictureBoxSizeMode.Zoom;
            picQRCode.TabIndex = 8;
            picQRCode.TabStop = false;
            // 
            // lblQRUrl
            // 
            lblQRUrl.Font = new Font("Consolas", 9F);
            lblQRUrl.ForeColor = Color.FromArgb(0, 120, 212);
            lblQRUrl.Location = new Point(10, 163);
            lblQRUrl.Name = "lblQRUrl";
            lblQRUrl.Size = new Size(180, 25);
            lblQRUrl.TabIndex = 9;
            lblQRUrl.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // FormSettings
            // 
            AcceptButton = btnSave;
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(243, 243, 243);
            CancelButton = btnCancel;
            ClientSize = new Size(559, 316);
            Controls.Add(grpQR);
            Controls.Add(lblHint);
            Controls.Add(btnCancel);
            Controls.Add(btnReset);
            Controls.Add(btnSave);
            Controls.Add(grpNetwork);
            Controls.Add(lblTitle);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormSettings";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "设置 - WebExplorer";
            grpNetwork.ResumeLayout(false);
            grpNetwork.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudPort).EndInit();
            grpQR.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picQRCode).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.GroupBox grpNetwork;
        public System.Windows.Forms.ComboBox cboIP;
        public System.Windows.Forms.NumericUpDown nudPort;
        public System.Windows.Forms.CheckBox chkAutoStart;
        public System.Windows.Forms.CheckBox chkShowQR;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.Label lblIP;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.Label lblHint;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox grpQR;
        public System.Windows.Forms.PictureBox picQRCode;
        private System.Windows.Forms.Label lblQRUrl;
    }
}
