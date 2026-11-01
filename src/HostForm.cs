using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace ShadersWallpaper
{
    /// <summary>
    /// Ventana pegada detrás de los íconos que muestra TU index.html (el mismo de Lively) en WebView2.
    /// </summary>
    internal class HostForm : Form
    {
        private readonly Settings _settings;
        private readonly WebView2 _web;
        private IntPtr _parent;
        public Rectangle ScreenRect { get; private set; }
        public bool Ready { get; private set; }

        public HostForm(Settings settings)
        {
            _settings = settings;
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            BackColor = Color.Black;
            Text = "ShadersWallpaper.Fondo";
            ScreenRect = Screen.PrimaryScreen.Bounds;   // igual que Lively: la pantalla principal completa
            Bounds = new Rectangle(-32000, -32000, ScreenRect.Width, ScreenRect.Height); // se ubica al pegarla

            _web = new WebView2 { Dock = DockStyle.Fill, DefaultBackgroundColor = Color.Black };
            Controls.Add(_web);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= (int)(Native.WS_EX_TOOLWINDOW | Native.WS_EX_NOACTIVATE);
                return cp;
            }
        }

        protected override bool ShowWithoutActivation => true;

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);
            DesktopHost.Mode = _settings.AttachMode ?? "auto";
            _parent = DesktopHost.Attach(Handle, ScreenRect);
            await InitWebView();
        }

        public void Refit()
        {
            ScreenRect = Screen.PrimaryScreen.Bounds;
            if (_parent != IntPtr.Zero) DesktopHost.FitTo(Handle, _parent, ScreenRect);
        }

        private async Task InitWebView()
        {
            try
            {
                string folder = _settings.FolderPath;
                string page = Path.Combine(folder, _settings.Page);
                Log.Write("Carpeta del fondo: " + folder + (File.Exists(page) ? "" : "  (¡NO existe " + _settings.Page + "!)"));

                string userData = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ShadersWallpaper", "WebView2");

                // Igual que un reproductor de fondos: que el video y el audio corran sin pedir clic,
                // y que Chromium no "duerma" la página por estar tapada por los íconos.
                var options = new CoreWebView2EnvironmentOptions(
                    "--autoplay-policy=no-user-gesture-required " +
                    "--disable-features=CalculateNativeWinOcclusion,msSmartScreenProtection " +
                    "--disable-backgrounding-occluded-windows " +
                    "--disable-renderer-backgrounding " +
                    "--disable-background-timer-throttling");

                var env = await CoreWebView2Environment.CreateAsync(null, userData, options);
                await _web.EnsureCoreWebView2Async(env);

                var s = _web.CoreWebView2.Settings;
                s.AreDefaultContextMenusEnabled = false;
                s.AreDevToolsEnabled = false;
                s.IsStatusBarEnabled = false;
                s.IsZoomControlEnabled = false;
                s.AreBrowserAcceleratorKeysEnabled = false;
                s.IsSwipeNavigationEnabled = false;

                _web.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    "wallpaper.local", folder, CoreWebView2HostResourceAccessKind.Allow);
                _web.CoreWebView2.NavigationCompleted += (o, a) =>
                    Log.Write("Página cargada: " + (a.IsSuccess ? "OK" : "ERROR " + a.WebErrorStatus));
                _web.CoreWebView2.Navigate("https://wallpaper.local/" + Uri.EscapeDataString(_settings.Page));
                Ready = true;
                Log.Write("WebView2 versión " + env.BrowserVersionString);
            }
            catch (Exception ex)
            {
                Log.Write("Error iniciando WebView2: " + ex);
            }
        }

        public async void SetPaused(bool paused)
        {
            if (!Ready || _web.CoreWebView2 == null) return;
            try
            {
                if (paused)
                {
                    await _web.CoreWebView2.ExecuteScriptAsync("document.querySelectorAll('video').forEach(v => v.pause());");
                    _web.CoreWebView2.MemoryUsageTargetLevel = CoreWebView2MemoryUsageTargetLevel.Low;
                }
                else
                {
                    _web.CoreWebView2.MemoryUsageTargetLevel = CoreWebView2MemoryUsageTargetLevel.Normal;
                    await _web.CoreWebView2.ExecuteScriptAsync(
                        "try { currentElement.play(); } catch (e) { const v = document.querySelector('video'); if (v) v.play(); }");
                }
            }
            catch { }
        }

        public void Reload()
        {
            try { _web.CoreWebView2?.Reload(); } catch { }
        }

        /// <summary>Ventana interna de Chromium que recibe el mouse (dentro de WebView2).</summary>
        public IntPtr FindInputWindow()
        {
            IntPtr found = IntPtr.Zero;
            MouseForwarder.EnumChildWindows(Handle, (h, l) =>
            {
                if (Native.ClassOf(h) == "Chrome_RenderWidgetHostHWND") { found = h; return false; }
                return true;
            }, IntPtr.Zero);
            return found;
        }
    }
}
