using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace ShadersWallpaper
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            using var mutex = new Mutex(true, "ShadersWallpaper_SingleInstance", out bool isFirst);
            if (!isFirst) return;

            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += (s, e) => Log.Write("Error: " + e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (s, e) => Log.Write("Error fatal: " + e.ExceptionObject);

            try { Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal; } catch { }

            Application.Run(new WallpaperApp());
        }
    }

    /// <summary>Bandeja del sistema, pausa automática y reinicio de Explorer.</summary>
    internal class WallpaperApp : ApplicationContext
    {
        private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string RunName = "ShadersWallpaper";

        private readonly Settings _settings = Settings.Load();
        private HostForm _host;
        private readonly MouseForwarder _mouse;
        private readonly NotifyIcon _tray;
        private readonly MessageWindow _msg;
        private readonly System.Windows.Forms.Timer _pauseTimer;
        private ToolStripMenuItem _miPause;
        private bool _userPaused, _locked, _isPaused;

        public WallpaperApp()
        {
            Log.Write("Shaders Wallpaper 2.0 iniciando. Windows " + Environment.OSVersion.Version);

            if (Process.GetProcessesByName("Lively").Any())
                Log.Write("Aviso: Lively sigue abierto.");

            CreateHost();
            _mouse = new MouseForwarder(() => _host);
            _tray = CreateTray();
            _msg = new MessageWindow(OnExplorerRestarted);

            _pauseTimer = new System.Windows.Forms.Timer { Interval = 1500 };
            _pauseTimer.Tick += (s, e) => UpdatePause();
            _pauseTimer.Start();

            SystemEvents.SessionSwitch += OnSessionSwitch;
            SystemEvents.DisplaySettingsChanged += (s, e) => _host?.Refit();
        }

        private void CreateHost()
        {
            _host = new HostForm(_settings);
            _host.Show();
        }

        private void OnExplorerRestarted()
        {
            Log.Write("Explorer se reinició: vuelvo a pegar el fondo.");
            try { _host?.Close(); _host?.Dispose(); } catch { }
            CreateHost();
            _isPaused = false;
        }

        // ---------------------------------------------------------------- pausa automática

        private void UpdatePause()
        {
            bool pause = _userPaused || _locked
                         || (_settings.PauseOnBattery && SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Offline)
                         || IsDesktopCovered();
            if (pause == _isPaused) return;
            _isPaused = pause;
            _host?.SetPaused(pause);
        }

        private bool IsDesktopCovered()
        {
            IntPtr fg = Native.GetForegroundWindow();
            if (fg == IntPtr.Zero || !Native.IsWindowVisible(fg)) return false;
            Native.GetWindowThreadProcessId(fg, out uint pid);
            if (pid == (uint)Environment.ProcessId) return false;

            string cls = Native.ClassOf(fg);
            if (cls == "Progman" || cls == "WorkerW" || cls == "Shell_TrayWnd" || cls == "Shell_SecondaryTrayWnd"
                || cls == "Windows.UI.Core.CoreWindow" || cls == "XamlExplorerHostIslandWindow"
                || cls == "TopLevelWindowForOverflowXamlIsland")
                return false;

            // Solo cuenta si la ventana está en la pantalla del fondo.
            var mi = new Native.MONITORINFO { cbSize = System.Runtime.InteropServices.Marshal.SizeOf<Native.MONITORINFO>() };
            if (!Native.GetMonitorInfo(Native.MonitorFromWindow(fg, Native.MONITOR_DEFAULTTONEAREST), ref mi)) return false;
            var m = mi.rcMonitor;
            var screen = Screen.PrimaryScreen.Bounds;
            if (m.Left != screen.Left || m.Top != screen.Top) return false;

            if (_settings.PauseOnMaximized && Native.IsZoomed(fg)) return true;
            if (_settings.PauseOnFullscreen && Native.GetWindowRect(fg, out var r))
                return r.Left <= m.Left && r.Top <= m.Top && r.Right >= m.Right && r.Bottom >= m.Bottom;
            return false;
        }

        private void OnSessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (e.Reason == SessionSwitchReason.SessionLock) _locked = true;
            else if (e.Reason == SessionSwitchReason.SessionUnlock) _locked = false;
            UpdatePause();
        }

        // ---------------------------------------------------------------- bandeja

        private NotifyIcon CreateTray()
        {
            var menu = new ContextMenuStrip();
            _miPause = new ToolStripMenuItem("Pausar fondo") { CheckOnClick = true };
            _miPause.Click += (s, e) => { _userPaused = _miPause.Checked; UpdatePause(); };
            var reload = new ToolStripMenuItem("Recargar fondo");
            reload.Click += (s, e) => _host?.Reload();
            var startup = new ToolStripMenuItem("Iniciar con Windows") { CheckOnClick = true, Checked = IsStartupEnabled() };
            startup.Click += (s, e) => SetStartup(startup.Checked);
            var folder = new ToolStripMenuItem("Abrir carpeta del fondo");
            folder.Click += (s, e) => Process.Start(new ProcessStartInfo("explorer.exe", "\"" + _settings.FolderPath + "\"") { UseShellExecute = true });
            var exit = new ToolStripMenuItem("Salir");
            exit.Click += (s, e) => ExitThread();

            menu.Items.AddRange(new ToolStripItem[]
            {
                _miPause, reload, startup, new ToolStripSeparator(), folder, exit
            });

            System.Drawing.Icon icon;
            try { icon = new System.Drawing.Icon(Path.Combine(Settings.BaseDir, "icon.ico")); }
            catch { icon = System.Drawing.SystemIcons.Application; }

            return new NotifyIcon { Icon = icon, Text = "Shaders Wallpaper", ContextMenuStrip = menu, Visible = true };
        }

        private static bool IsStartupEnabled()
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKey);
            return key?.GetValue(RunName) != null;
        }

        private static void SetStartup(bool enabled)
        {
            using var key = Registry.CurrentUser.CreateSubKey(RunKey);
            if (enabled) key.SetValue(RunName, "\"" + Environment.ProcessPath + "\"");
            else key.DeleteValue(RunName, false);
        }

        // ---------------------------------------------------------------- salida

        protected override void ExitThreadCore()
        {
            SystemEvents.SessionSwitch -= OnSessionSwitch;
            _pauseTimer.Stop();
            _mouse.Dispose();
            _tray.Visible = false;
            _tray.Dispose();
            _msg.DestroyHandle();
            try { _host?.Close(); } catch { }
            DesktopHost.RestoreWindowsWallpaper();
            base.ExitThreadCore();
        }
    }

    /// <summary>Ventana invisible de nivel superior para enterarse de que Explorer se reinició.</summary>
    internal class MessageWindow : NativeWindow
    {
        private readonly uint _taskbarCreated = Native.RegisterWindowMessage("TaskbarCreated");
        private readonly Action _onRestart;

        public MessageWindow(Action onRestart)
        {
            _onRestart = onRestart;
            CreateHandle(new CreateParams { Caption = "ShadersWallpaperMsg", ExStyle = (int)Native.WS_EX_TOOLWINDOW });
        }

        protected override void WndProc(ref Message m)
        {
            if (_taskbarCreated != 0 && (uint)m.Msg == _taskbarCreated) _onRestart();
            base.WndProc(ref m);
        }
    }
}
