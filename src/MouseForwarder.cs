using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ShadersWallpaper
{
    /// <summary>
    /// Los íconos del escritorio quedan encima del fondo y se quedan con el mouse.
    /// Igual que Lively: un gancho de mouse que, cuando el cursor está sobre el escritorio
    /// (no sobre otra ventana), le reenvía el movimiento y los clics a tu HTML.
    /// Así funcionan el botón invisible, el hover y el menú.
    /// </summary>
    internal class MouseForwarder : IDisposable
    {
        private const int WH_MOUSE_LL = 14;
        private const int WM_MOUSEMOVE = 0x0200, WM_LBUTTONDOWN = 0x0201, WM_LBUTTONUP = 0x0202, WM_MOUSEWHEEL = 0x020A;
        private const int MK_LBUTTON = 0x0001;

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT { public Native.POINT pt; public uint mouseData, flags, time; public IntPtr extra; }
        private delegate IntPtr HookProc(int code, IntPtr wParam, IntPtr lParam);
        public delegate bool EnumProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int id, HookProc proc, IntPtr hMod, uint threadId);
        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hook, int code, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hook);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr GetModuleHandle(string name);
        [DllImport("user32.dll")]
        private static extern IntPtr WindowFromPoint(Native.POINT pt);
        [DllImport("user32.dll")]
        private static extern IntPtr GetAncestor(IntPtr hwnd, uint flags);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumChildWindows(IntPtr parent, EnumProc proc, IntPtr lParam);

        private readonly HookProc _proc;   // referencia viva para que el GC no la borre
        private IntPtr _hook;
        private readonly Func<HostForm> _host;
        private IntPtr _target;
        private bool _leftDown, _wasOver;

        public bool Enabled { get; set; } = true;

        public MouseForwarder(Func<HostForm> host)
        {
            _host = host;
            _proc = Callback;
            using var module = Process.GetCurrentProcess().MainModule;
            _hook = SetWindowsHookEx(WH_MOUSE_LL, _proc, GetModuleHandle(module.ModuleName), 0);
            Log.Write(_hook == IntPtr.Zero ? "No se pudo instalar el gancho del mouse" : "Gancho del mouse activo");
        }

        private IntPtr Callback(int code, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                if (code >= 0 && Enabled) Handle(wParam.ToInt32(), Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam));
            }
            catch { }
            return CallNextHookEx(_hook, code, wParam, lParam);
        }

        private void Handle(int msg, MSLLHOOKSTRUCT info)
        {
            if (msg == WM_LBUTTONDOWN) _leftDown = true;
            else if (msg == WM_LBUTTONUP) _leftDown = false;

            var host = _host();
            if (host == null || !host.Ready) return;
            var p = info.pt;
            var r = host.ScreenRect;
            bool over = p.X >= r.Left && p.X < r.Right && p.Y >= r.Top && p.Y < r.Bottom && IsDesktopAt(p);

            if (!over)
            {
                if (_wasOver)
                {
                    // El cursor se fue a otra ventana: quita el hover del HTML.
                    Post(WM_MOUSEMOVE, IntPtr.Zero, MakeLParam(-100, -100), host);
                    _wasOver = false;
                }
                return;
            }
            _wasOver = true;

            if (_target == IntPtr.Zero || !Native.IsWindow(_target))
            {
                _target = host.FindInputWindow();
                Log.Write($"Ventana de entrada de Chromium: 0x{_target.ToInt64():X}");
                if (_target == IntPtr.Zero) return;
            }

            var c = p;
            Native.ScreenToClient(_target, ref c);
            IntPtr lp = MakeLParam(c.X, c.Y);
            IntPtr mk = new IntPtr(_leftDown ? MK_LBUTTON : 0);

            switch (msg)
            {
                case WM_MOUSEMOVE:
                case WM_LBUTTONDOWN:
                case WM_LBUTTONUP:
                    Post(msg, mk, lp, host);
                    break;
                case WM_MOUSEWHEEL:
                    // La rueda usa coordenadas de pantalla.
                    int delta = (short)(info.mouseData >> 16);
                    Post(msg, new IntPtr((delta << 16) | mk.ToInt32()), MakeLParam(p.X, p.Y), host);
                    break;
            }
        }

        private void Post(int msg, IntPtr wParam, IntPtr lParam, HostForm host)
        {
            if (_target == IntPtr.Zero) return;
            Native.PostMessage(_target, (uint)msg, wParam, lParam);
        }

        private static IntPtr MakeLParam(int x, int y) => new IntPtr((y << 16) | (x & 0xFFFF));

        /// <summary>¿Lo que está bajo el cursor es el escritorio (íconos o fondo) y no otra ventana?</summary>
        private static bool IsDesktopAt(Native.POINT p)
        {
            IntPtr w = WindowFromPoint(p);
            if (w == IntPtr.Zero) return false;
            string root = Native.ClassOf(GetAncestor(w, 2 /* GA_ROOT */));
            return root == "Progman" || root == "WorkerW";
        }

        public void Dispose()
        {
            if (_hook != IntPtr.Zero) { UnhookWindowsHookEx(_hook); _hook = IntPtr.Zero; }
        }
    }
}
