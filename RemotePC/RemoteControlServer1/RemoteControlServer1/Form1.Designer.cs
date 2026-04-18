namespace RemoteControlServer1
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        // Các thành phần giao diện chính
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Panel pnlSystem;
        private System.Windows.Forms.Panel pnlRequest;
        private System.Windows.Forms.Panel pnlStats;
        private System.Windows.Forms.Panel pnlIdle;
        private System.Windows.Forms.Panel pnlConnected; // Panel mới khi đã kết nối

        // Các tiêu đề chung cho từng khung
        private System.Windows.Forms.Label lblSystemTitle;
        private System.Windows.Forms.Label lblIdleTitle;
        private System.Windows.Forms.Label lblConnectedTitle;
        private System.Windows.Forms.Label lblStatsTitle;
        private System.Windows.Forms.Label lblRequestTitle;

        // Label trong nhóm Hệ Thống
        private System.Windows.Forms.Label lblServerStatus;
        private System.Windows.Forms.Label lblClientStatus;
        private System.Windows.Forms.Label lblIP;
        private System.Windows.Forms.Label lblPort;

        // Nhóm Yêu Cầu Kết Nối
        private System.Windows.Forms.Label lblRequestIP;
        private System.Windows.Forms.Button btnAccept;
        private System.Windows.Forms.Button btnDeny;

        // Khung chờ
        private System.Windows.Forms.Label lblIdleStatus;

        // Khung đã kết nối
        private System.Windows.Forms.Label lblConnectedIcon;
        private System.Windows.Forms.Label lblConnectedIP;

        // Thống kê
        private System.Windows.Forms.Label lblUptime;
        private System.Windows.Forms.Label lblPing;

        // Terminal & Control
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Label lblLogHeader;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblTitle = new System.Windows.Forms.Label();
            this.pnlSystem = new System.Windows.Forms.Panel();
            this.pnlRequest = new System.Windows.Forms.Panel();
            this.pnlIdle = new System.Windows.Forms.Panel();
            this.pnlConnected = new System.Windows.Forms.Panel();
            this.pnlStats = new System.Windows.Forms.Panel();

            this.lblSystemTitle = new System.Windows.Forms.Label();
            this.lblIdleTitle = new System.Windows.Forms.Label();
            this.lblConnectedTitle = new System.Windows.Forms.Label();
            this.lblStatsTitle = new System.Windows.Forms.Label();
            this.lblRequestTitle = new System.Windows.Forms.Label();

            this.lblServerStatus = new System.Windows.Forms.Label();
            this.lblClientStatus = new System.Windows.Forms.Label();
            this.lblIP = new System.Windows.Forms.Label();
            this.lblPort = new System.Windows.Forms.Label();
            this.lblRequestIP = new System.Windows.Forms.Label();
            this.btnAccept = new System.Windows.Forms.Button();
            this.btnDeny = new System.Windows.Forms.Button();
            this.lblIdleStatus = new System.Windows.Forms.Label();
            this.lblConnectedIcon = new System.Windows.Forms.Label();
            this.lblConnectedIP = new System.Windows.Forms.Label();
            this.lblUptime = new System.Windows.Forms.Label();
            this.lblPing = new System.Windows.Forms.Label(); // KHỞI TẠO BIẾN lblPing ĐỂ KHÔNG BỊ LỖI
            this.txtLog = new System.Windows.Forms.TextBox();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.lblLogHeader = new System.Windows.Forms.Label();

            this.SuspendLayout();

            // --- Form1 Configuration ---
            this.ClientSize = new System.Drawing.Size(900, 520);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PC Remote";

            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(305, 15);
            this.lblTitle.Text = "REMOTE ACCESS GATEWAY";

            // --- 1. Panel HỆ THỐNG (Left) ---
            this.pnlSystem.Location = new System.Drawing.Point(20, 60);
            this.pnlSystem.Size = new System.Drawing.Size(270, 180);
            this.pnlSystem.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

            this.lblSystemTitle.Text = "THÔNG TIN HỆ THỐNG";
            this.lblSystemTitle.Location = new System.Drawing.Point(10, 10);
            this.lblSystemTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblSystemTitle.AutoSize = true;

            this.lblServerStatus.Location = new System.Drawing.Point(10, 40);
            this.lblServerStatus.Size = new System.Drawing.Size(250, 25);
            this.lblServerStatus.Text = "● Trạng thái: [ STOPPED ]";

            this.lblClientStatus.Location = new System.Drawing.Point(10, 75);
            this.lblClientStatus.Size = new System.Drawing.Size(250, 25);
            this.lblClientStatus.Text = "● Khách: [ 0 ] WAITING";

            this.lblIP.Location = new System.Drawing.Point(10, 110);
            this.lblIP.Size = new System.Drawing.Size(250, 25);
            this.lblIP.Text = "● Địa chỉ: [ 127.0.0.1 ]";

            this.lblPort.Location = new System.Drawing.Point(10, 145);
            this.lblPort.Size = new System.Drawing.Size(250, 25);
            this.lblPort.Text = "● Cổng: [ 8888 ]";

            this.pnlSystem.Controls.AddRange(new System.Windows.Forms.Control[] { lblSystemTitle, lblServerStatus, lblClientStatus, lblIP, lblPort });

            // --- 2a. Panel YÊU CẦU KẾT NỐI (Center - Request) ---
            this.pnlRequest.Location = new System.Drawing.Point(305, 60);
            this.pnlRequest.Size = new System.Drawing.Size(290, 180);
            this.pnlRequest.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlRequest.Visible = false;

            this.lblRequestTitle.Text = "YÊU CẦU KẾT NỐI";
            this.lblRequestTitle.Location = new System.Drawing.Point(10, 10);
            this.lblRequestTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblRequestTitle.AutoSize = true;

            this.lblRequestIP.Text = "CLIENT IP: ...";
            this.lblRequestIP.Location = new System.Drawing.Point(10, 45);
            this.lblRequestIP.AutoSize = true;

            this.btnAccept.Text = "ĐỒNG Ý ";
            this.btnAccept.Location = new System.Drawing.Point(15, 80);
            this.btnAccept.Size = new System.Drawing.Size(260, 40);
            this.btnAccept.Click += new System.EventHandler(this.btnAccept_Click);

            this.btnDeny.Text = "TỪ CHỐI ";
            this.btnDeny.Location = new System.Drawing.Point(15, 125);
            this.btnDeny.Size = new System.Drawing.Size(260, 40);
            this.btnDeny.Click += new System.EventHandler(this.btnDeny_Click);

            this.pnlRequest.Controls.AddRange(new System.Windows.Forms.Control[] { lblRequestTitle, lblRequestIP, btnAccept, btnDeny });

            // --- 2b. Panel CHỜ (Center - Idle) ---
            this.pnlIdle.Location = new System.Drawing.Point(305, 60);
            this.pnlIdle.Size = new System.Drawing.Size(290, 180);
            this.pnlIdle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlIdle.BackColor = System.Drawing.Color.FromArgb(40, 46, 54);

            this.lblIdleTitle.Text = "TRẠNG THÁI KẾT NỐI";
            this.lblIdleTitle.Location = new System.Drawing.Point(10, 10);
            this.lblIdleTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblIdleTitle.AutoSize = true;

            this.lblIdleStatus.Text = "📱\r\nĐang đợi kết nối...";
            this.lblIdleStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblIdleStatus.Location = new System.Drawing.Point(0, 50);
            this.lblIdleStatus.Size = new System.Drawing.Size(290, 100);
            this.lblIdleStatus.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblIdleStatus.ForeColor = System.Drawing.Color.Gray;

            this.pnlIdle.Controls.AddRange(new System.Windows.Forms.Control[] { lblIdleTitle, lblIdleStatus });

            // --- 2c. Panel ĐÃ KẾT NỐI (Center - Connected) ---
            this.pnlConnected.Location = new System.Drawing.Point(305, 60);
            this.pnlConnected.Size = new System.Drawing.Size(290, 180);
            this.pnlConnected.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlConnected.BackColor = System.Drawing.Color.FromArgb(40, 46, 54);

            this.lblConnectedTitle.Text = "TRẠNG THÁI KẾT NỐI";
            this.lblConnectedTitle.Location = new System.Drawing.Point(10, 10);
            this.lblConnectedTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblConnectedTitle.AutoSize = true;

            this.lblConnectedIcon.Text = "✅\r\nThiết bị đã kết nối!";
            this.lblConnectedIcon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblConnectedIcon.Location = new System.Drawing.Point(0, 50);
            this.lblConnectedIcon.Size = new System.Drawing.Size(290, 50);
            this.lblConnectedIcon.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblConnectedIcon.ForeColor = System.Drawing.Color.FromArgb(118, 255, 3); // Xanh neon

            this.lblConnectedIP.Text = "CLIENT IP: ...";
            this.lblConnectedIP.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblConnectedIP.Location = new System.Drawing.Point(0, 110);
            this.lblConnectedIP.Size = new System.Drawing.Size(290, 30);
            this.lblConnectedIP.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblConnectedIP.ForeColor = System.Drawing.Color.White;

            this.pnlConnected.Controls.AddRange(new System.Windows.Forms.Control[] { lblConnectedTitle, lblConnectedIcon, lblConnectedIP });

            // --- 3. Panel THỐNG KÊ (Right) ---
            this.pnlStats.Location = new System.Drawing.Point(610, 60);
            this.pnlStats.Size = new System.Drawing.Size(270, 180);
            this.pnlStats.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

            this.lblStatsTitle.Text = "THỐNG KÊ SERVER";
            this.lblStatsTitle.Location = new System.Drawing.Point(10, 10);
            this.lblStatsTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblStatsTitle.AutoSize = true;

            this.lblUptime.Text = "🕒 Uptime: [00h:00m]";
            this.lblUptime.Location = new System.Drawing.Point(15, 50);
            this.lblUptime.AutoSize = true;

            this.lblPing.Text = "⚡ Ping: [-- ms]";
            this.lblPing.Location = new System.Drawing.Point(15, 85);
            this.lblPing.AutoSize = true;

            this.pnlStats.Controls.AddRange(new System.Windows.Forms.Control[] { lblStatsTitle, lblUptime, lblPing });

            // --- TERMINAL LOGS ---
            this.lblLogHeader.Text = "TERMINAL LOGS";
            this.lblLogHeader.Location = new System.Drawing.Point(20, 260);
            this.lblLogHeader.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);

            this.txtLog.Location = new System.Drawing.Point(20, 285);
            this.txtLog.Multiline = true;
            this.txtLog.ReadOnly = true;
            this.txtLog.Size = new System.Drawing.Size(710, 210);

            // --- BUTTON CONTROLS ---
            this.btnStart.Text = "START";
            this.btnStart.Location = new System.Drawing.Point(750, 285);
            this.btnStart.Size = new System.Drawing.Size(130, 45);
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);

            this.btnStop.Text = "STOP";
            this.btnStop.Location = new System.Drawing.Point(750, 340);
            this.btnStop.Size = new System.Drawing.Size(130, 45);
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);

            this.btnClear.Text = "CLEAR LOG";
            this.btnClear.Location = new System.Drawing.Point(750, 450);
            this.btnClear.Size = new System.Drawing.Size(130, 45);
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);

            // --- Add to Form ---
            this.Controls.AddRange(new System.Windows.Forms.Control[] {
                lblTitle, pnlSystem, pnlConnected, pnlIdle, pnlRequest, pnlStats,
                lblLogHeader, txtLog, btnStart, btnStop, btnClear
            });

            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}