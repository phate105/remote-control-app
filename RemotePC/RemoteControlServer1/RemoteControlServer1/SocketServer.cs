using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Text.Json;
using Fleck;
using System.Threading.Tasks;

namespace RemoteControlServer1
{
    class SocketServer
    {
        WebSocketServer server;
        List<IWebSocketConnection> allClients = new List<IWebSocketConnection>();
        bool isRunning = false;

        TextBox logBox;
        Action<bool, string> updateClientStatus;
        Func<string, bool> requestPermission;
        Form1 formMain;

        private System.Timers.Timer pingTimer;
        private Stopwatch pingStopwatch = new Stopwatch();

        // [IMPORT LÕI WINDOWS] - Để điều khiển Media (Âm lượng, Play/Pause)
        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);
        const int VK_VOLUME_MUTE = 0xAD;
        const int VK_VOLUME_DOWN = 0xAE;
        const int VK_VOLUME_UP = 0xAF;
        const int VK_MEDIA_PLAY_PAUSE = 0xB3;

        // [IMPORT LÕI WINDOWS] - Để khóa màn hình
        [DllImport("user32.dll")]
        public static extern bool LockWorkStation();

        public SocketServer(TextBox log, Action<bool, string> clientCallback = null, Func<string, bool> permissionCallback = null, Form1 main = null)
        {
            logBox = log;
            updateClientStatus = clientCallback;
            requestPermission = permissionCallback;
            formMain = main;
        }

        public void StartServer()
        {
            if (isRunning) return;
            isRunning = true;

            server = new WebSocketServer("ws://0.0.0.0:8888");

            server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    string clientIP = socket.ConnectionInfo.ClientIpAddress;

                    // [FIX 3] ĐỘC QUYỀN 1 THIẾT BỊ: Chặn ngay lập tức nếu đã có người kết nối
                    if (allClients.Count >= 1)
                    {
                        WriteLog($"Từ chối kết nối chui từ: {clientIP} (Hệ thống đang bận)");
                        socket.Close();
                        return;
                    }

                    WriteLog("Yêu cầu kết nối từ: " + clientIP);

                    bool allow = false;
                    if (requestPermission != null)
                        allow = requestPermission(clientIP);

                    if (allow)
                    {
                        allClients.Add(socket);
                        updateClientStatus?.Invoke(true, clientIP);

                        // [FIX 2] Xóa trắng đồng hồ Ping để tính lại từ đầu cho phiên kết nối mới
                        pingStopwatch.Reset();

                        var acceptRes = new BaseResponse { status = "auth", message = "ACCEPTED" };
                        socket.Send(JsonSerializer.Serialize(acceptRes));
                        WriteLog("Đã đồng ý cho: " + clientIP);
                    }
                    else
                    {
                        var denyRes = new BaseResponse { status = "auth", message = "DENIED" };
                        socket.Send(JsonSerializer.Serialize(denyRes));
                        WriteLog("Đã từ chối: " + clientIP);
                        Task.Delay(500).ContinueWith(_ => socket.Close());
                    }
                };

                socket.OnClose = () =>
                {
                    allClients.Remove(socket);
                    WriteLog("Một kết nối đã ngắt.");
                    updateClientStatus?.Invoke(allClients.Count > 0, "");
                };

                socket.OnPong = (byte[] b) =>
                {
                    pingStopwatch.Stop();
                    formMain?.UpdatePingUI(pingStopwatch.ElapsedMilliseconds);
                };

                socket.OnMessage = message =>
                {
                    // Tắt log của lệnh screenshot đi kẻo spam đầy khung Log (vì nó gọi 2s/lần)
                    if (!message.Contains("screenshot"))
                        WriteLog("Lệnh nhận được: " + message);

                    try
                    {
                        var request = JsonSerializer.Deserialize<BaseRequest>(message);

                        // Truyền nguyên cục Request vào để bóc tách data
                        string result = ExecuteCommand(request);

                        var response = new BaseResponse
                        {
                            status = "success",
                            message = result
                        };
                        socket.Send(JsonSerializer.Serialize(response));
                    }
                    catch (Exception ex)
                    {
                        WriteLog("Lỗi xử lý JSON: " + ex.Message);
                    }
                };
            });

            pingTimer = new System.Timers.Timer(2000);
            pingTimer.Elapsed += (sender, e) =>
            {
                try
                {
                    var clients = allClients.ToList();
                    if (clients.Count > 0 && clients[0].IsAvailable)
                    {
                        pingStopwatch.Restart();
                        clients[0].SendPing(new byte[] { 1 });
                    }
                }
                catch { }
            };
            pingTimer.Start();

            WriteLog("Server WebSocket đang chạy port 8888...");
            WriteLog("IP Local: " + GetLocalIP());
        }

        public void StopServer()
        {
            isRunning = false;
            if (pingTimer != null)
            {
                pingTimer.Stop();
                pingTimer.Dispose();
            }
            foreach (var client in allClients.ToList()) client.Close();
            allClients.Clear();

            // [FIX 1] Giải phóng hoàn toàn Port 8888 để chặn kết nối ngầm khi chưa Start
            if (server != null)
            {
                server.Dispose();
            }

            WriteLog("Server đã dừng.");
        }

        // Đã đổi tham số truyền vào thành BaseRequest để đọc được biến "data"
        string ExecuteCommand(BaseRequest request)
        {
            string cmd = request.command?.Trim().ToLower();
            if (string.IsNullOrEmpty(cmd)) return "Lệnh trống";

            switch (cmd)
            {
                // ---- CÁC LỆNH HỆ THỐNG CŨ ----
                case "shutdown":
                    Process.Start("shutdown", "/s /t 0");
                    return "Máy tính đang thực hiện tắt nguồn...";

                case "restart":
                    Process.Start("shutdown", "/r /t 0");
                    return "Máy tính đang thực hiện khởi động lại...";

                case "lock":
                    LockWorkStation();
                    return "Máy tính đã được khóa màn hình.";

                case "sleep": // TÍNH NĂNG MỚI
                    Application.SetSuspendState(PowerState.Suspend, true, true);
                    return "Máy tính đang vào chế độ Sleep.";

                case "getinfo":
                    return GetSystemInfo();

                case "getprocess":
                    return GetProcessList();

                // ---- TÍNH NĂNG MỚI: TẮT TIẾN TRÌNH ----
                case "killprocess":
                    if (request.data.ValueKind != JsonValueKind.Undefined && request.data.TryGetProperty("name", out JsonElement nameElem))
                    {
                        string pName = nameElem.GetString();
                        try
                        {
                            var procs = Process.GetProcessesByName(pName);
                            if (procs.Length == 0) return $"Không tìm thấy tiến trình đang chạy: {pName}";
                            foreach (var p in procs) p.Kill();
                            return $"Đã tắt thành công tiến trình: {pName}";
                        }
                        catch { return $"Lỗi khi tắt tiến trình {pName}"; }
                    }
                    return "Thiếu tên tiến trình";

                // ---- TÍNH NĂNG MỚI: MEDIA CONTROL ----
                case "volup":
                    keybd_event(VK_VOLUME_UP, 0, 0, 0);
                    return "Đã TĂNG âm lượng";

                case "voldown":
                    keybd_event(VK_VOLUME_DOWN, 0, 0, 0);
                    return "Đã GIẢM âm lượng";

                case "volmute":
                    keybd_event(VK_VOLUME_MUTE, 0, 0, 0);
                    return "Đã Bật/Tắt MUTE";

                case "playpause":
                    keybd_event(VK_MEDIA_PLAY_PAUSE, 0, 0, 0);
                    return "Đã Play/Pause Media";

                // ---- TÍNH NĂNG MỚI: CHỤP MÀN HÌNH ----
                case "screenshot":
                    return GetScreenshotBase64();

                default:
                    return "Không tìm thấy lệnh: " + cmd;
            }
        }

        string GetScreenshotBase64()
        {
            try
            {
                var bounds = Screen.PrimaryScreen.Bounds;
                using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
                {
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
                    }
                    // Dùng MemoryStream để ép ảnh thành JPEG giúp truyền qua mạng cực nhanh
                    using (MemoryStream ms = new MemoryStream())
                    {
                        bitmap.Save(ms, ImageFormat.Jpeg);
                        byte[] byteImage = ms.ToArray();
                        return Convert.ToBase64String(byteImage);
                    }
                }
            }
            catch { return "screenshot_error"; }
        }

        string GetSystemInfo()
        {
            try
            {
                DriveInfo cDrive = new DriveInfo("C");
                double totalSpace = Math.Round(cDrive.TotalSize / (1024.0 * 1024 * 1024), 2);
                double freeSpace = Math.Round(cDrive.AvailableFreeSpace / (1024.0 * 1024 * 1024), 2);
                var screen = Screen.PrimaryScreen.Bounds;
                string arch = Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit";

                return
                    "--- THÔNG TIN HỆ THỐNG ---\n" +
                    $"💻 Tên máy: {Environment.MachineName}\n" +
                    $"👤 Tài khoản: {Environment.UserName}\n" +
                    $"💿 OS: {Environment.OSVersion} ({arch})\n" +
                    $"⚙️ .NET Version: {Environment.Version}\n\n" +
                    "--- THÔNG SỐ PHẦN CỨNG ---\n" +
                    $"🖥️ Màn hình: {screen.Width}x{screen.Height} px\n" +
                    $"🧠 CPU: {Environment.ProcessorCount} cores\n" +
                    $"💾 Ổ C: {freeSpace} GB trống / {totalSpace} GB tổng\n\n" +
                    $"⏰ Cập nhật lúc: {DateTime.Now:HH:mm:ss}";
            }
            catch { return "Lỗi khi truy xuất thông tin hệ thống."; }
        }

        string GetProcessList()
        {
            try
            {
                var processes = Process.GetProcesses()
                                // BÍ QUYẾT Ở ĐÂY: Chỉ lấy những tiến trình có Tiêu đề cửa sổ
                                .Where(p => !string.IsNullOrEmpty(p.MainWindowTitle))
                                .Select(p => p.ProcessName)
                                .Distinct()
                                .OrderBy(n => n);
                // Mình đã bỏ .Take(20) đi vì app có cửa sổ thường không nhiều, hiển thị hết sẽ đúng thực tế hơn.

                // Đổi chữ "Top 20" thành "Danh sách" để khớp chuẩn với bộ lọc bên React Native của bạn
                return "Danh sách tiến trình:\n" + string.Join("\n", processes);
            }
            catch { return "Không thể lấy danh sách tiến trình."; }
        }

        void WriteLog(string text)
        {
            if (logBox.IsDisposed) return;
            logBox.Invoke((MethodInvoker)(() => {
                logBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {text}\r\n");
            }));
        }

        string GetLocalIP()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            return host.AddressList.FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?.ToString() ?? "127.0.0.1";
        }
    }

    // ĐÃ ĐỔI "object data" THÀNH "JsonElement data"
    public class BaseRequest
    {
        public string command { get; set; }
        public JsonElement data { get; set; }
    }
    public class BaseResponse
    {
        public string status { get; set; }
        public string message { get; set; }
    }
}