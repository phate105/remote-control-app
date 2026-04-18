using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Linq;

namespace RemoteControlServer1
{
    public partial class Form1 : Form
    {
        SocketServer server;
        private TaskCompletionSource<bool> _authTask;

        // Quản lý thời gian Uptime
        private Timer uptimeTimer;
        private DateTime startTime;

        public Form1()
        {
            InitializeComponent();
            ApplyModernTheme();

            pnlRequest.Visible = false;
            pnlConnected.Visible = false;

            // Khởi tạo trạng thái nút bấm ban đầu
            SetButtonState(btnStart, true);
            SetButtonState(btnStop, false);

            // Cấu hình Timer cho Uptime
            uptimeTimer = new Timer();
            uptimeTimer.Interval = 1000; // Cập nhật mỗi 1 giây
            uptimeTimer.Tick += UptimeTimer_Tick;
        }

        private void ApplyModernTheme()
        {
            this.BackColor = Color.FromArgb(32, 38, 46);
            this.ForeColor = Color.White;

            txtLog.BackColor = Color.Black;
            txtLog.ForeColor = Color.FromArgb(118, 255, 3);
            txtLog.BorderStyle = BorderStyle.None;

            StyleControlBtn(btnStart, Color.FromArgb(74, 79, 89));
            StyleControlBtn(btnStop, Color.FromArgb(74, 79, 89));
            StyleControlBtn(btnClear, Color.FromArgb(51, 51, 51));

            StyleConfirmBtn(btnAccept, Color.FromArgb(118, 255, 3), Color.Black);
            StyleConfirmBtn(btnDeny, Color.FromArgb(255, 82, 82), Color.White);

            // Đặt font chữ chuyên nghiệp cho Uptime và Ping
            lblUptime.Font = new Font("Consolas", 10F, FontStyle.Bold);
            lblPing.Font = new Font("Consolas", 10F, FontStyle.Bold);
        }

        private void SetButtonState(Button btn, bool isEnabled)
        {
            btn.Enabled = isEnabled;
            // Làm mờ bằng cách đổi màu nền tối đi khi disabled
            btn.BackColor = isEnabled ? Color.FromArgb(74, 79, 89) : Color.FromArgb(40, 46, 54);
        }

        private void StyleControlBtn(Button btn, Color bg)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = bg;
            btn.Paint += (s, e) => {
                GraphicsPath path = GetRoundedRect(new Rectangle(0, 0, btn.Width, btn.Height), 8);
                btn.Region = new Region(path);
            };
        }

        private void StyleConfirmBtn(Button btn, Color bg, Color fg)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = bg;
            btn.ForeColor = fg;
            btn.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        }

        private GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void UptimeTimer_Tick(object sender, EventArgs e)
        {
            TimeSpan elapsed = DateTime.Now - startTime;
            string timeStr = string.Format("{0:D2}h:{1:D2}m:{2:D2}s",
                (int)elapsed.TotalHours, elapsed.Minutes, elapsed.Seconds);
            lblUptime.Text = "🕒 Uptime: [" + timeStr + "]";
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (server == null)
            {
                // Truyền thêm "this" để SocketServer có thể gọi hàm UpdatePingUI
                server = new SocketServer(txtLog, UpdateClientStatus, AskClientPermissionUI, this);
                server.StartServer();

                lblServerStatus.Text = "● Trạng thái: [ RUNNING ]";
                lblServerStatus.ForeColor = Color.FromArgb(118, 255, 3);
                lblIP.Text = "● Địa chỉ: [ " + GetLocalIP() + " ]";

                // Chạy đồng hồ Uptime
                startTime = DateTime.Now;
                uptimeTimer.Start();

                // Khóa nút Start, mở nút Stop
                SetButtonState(btnStart, false);
                SetButtonState(btnStop, true);
            }
        }

        // Hàm cập nhật Ping được gọi từ SocketServer
        public void UpdatePingUI(long ms)
        {
            if (this.IsDisposed) return;
            this.Invoke((MethodInvoker)(() => {
                lblPing.Text = $"⚡ Ping: [{ms} ms]";

                // Đổi màu theo độ trễ: Xanh (<100ms), Vàng (<300ms), Đỏ (Chậm)
                if (ms < 100) lblPing.ForeColor = Color.FromArgb(118, 255, 3);
                else if (ms < 300) lblPing.ForeColor = Color.Yellow;
                else lblPing.ForeColor = Color.FromArgb(255, 82, 82);
            }));
        }

        bool AskClientPermissionUI(string ip)
        {
            _authTask = new TaskCompletionSource<bool>();

            this.Invoke((MethodInvoker)(() => {
                pnlIdle.Visible = false;
                lblRequestIP.Text = "CLIENT IP: " + ip;
                pnlRequest.Visible = true;
                pnlRequest.BringToFront();
            }));

            bool result = _authTask.Task.Result;

            this.Invoke((MethodInvoker)(() => {
                pnlRequest.Visible = false;
            }));

            return result;
        }

        private void btnAccept_Click(object sender, EventArgs e) => _authTask?.SetResult(true);
        private void btnDeny_Click(object sender, EventArgs e)
        {
            _authTask?.SetResult(false);
            pnlIdle.Visible = true;
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            server?.StopServer();
            server = null;

            // Dừng đồng hồ Uptime và reset Ping
            uptimeTimer.Stop();
            lblUptime.Text = "🕒 Uptime: [00h:00m:00s]";
            lblPing.Text = "⚡ Ping: [-- ms]";
            lblPing.ForeColor = Color.White;

            lblServerStatus.Text = "● Trạng thái: [ STOPPED ]";
            lblServerStatus.ForeColor = Color.Red;
            lblClientStatus.Text = "● Khách: [ 0 ] WAITING";
            lblClientStatus.ForeColor = Color.Gray;

            pnlRequest.Visible = false;
            pnlConnected.Visible = false;
            pnlIdle.Visible = true;

            // Mở lại nút Start, khóa nút Stop
            SetButtonState(btnStart, true);
            SetButtonState(btnStop, false);
        }

        private void btnClear_Click(object sender, EventArgs e) => txtLog.Clear();

        void UpdateClientStatus(bool connected, string clientIp)
        {
            if (this.IsDisposed) return;
            this.Invoke((MethodInvoker)(() => {
                lblClientStatus.Text = connected ? "● Khách: [ 1 ] CONNECTED" : "● Khách: [ 0 ] WAITING";
                lblClientStatus.ForeColor = connected ? Color.FromArgb(118, 255, 3) : Color.Gray;

                if (connected)
                {
                    pnlIdle.Visible = false;
                    pnlConnected.Visible = true;
                    lblConnectedIP.Text = "CLIENT IP: " + clientIp;
                    pnlConnected.BringToFront();
                }
                else
                {
                    pnlConnected.Visible = false;
                    pnlIdle.Visible = true;
                    lblPing.Text = "⚡ Ping: [-- ms]"; // Reset ping khi khách ngắt kết nối
                    lblPing.ForeColor = Color.White;
                }
            }));
        }

        string GetLocalIP()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            return host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)?.ToString() ?? "127.0.0.1";
        }
    }
}