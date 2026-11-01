using System;
using System.Drawing;
using System.Text;

namespace ShadersWallpaper
{
    /// <summary>
    /// Pega una ventana detrás de los íconos del escritorio (el mismo truco que usa Lively).
    /// Soporta el escritorio clásico y el "elevado" de Windows 11 24H2+.
    /// </summary>
    internal static class DesktopHost
    {
        /// <summary>"auto", "worker" o "progman" (para probar si un modo no se ve).</summary>
        public static string Mode = "auto";

        public static IntPtr Attach(IntPtr hwnd, Rectangle screenRect)
        {
            IntPtr progman = Native.FindWindow("Progman", null);
            Log.Write($"Progman=0x{progman.ToInt64():X}");
            if (progman == IntPtr.Zero) return IntPtr.Zero;

            // Pide a Explorer que cree la capa WorkerW detrás de los íconos.
            Native.SendMessageTimeout(progman, 0x052C, new IntPtr(0xD), new IntPtr(0x1), Native.SMTO_NORMAL, 1000, out _);
            Native.SendMessageTimeout(progman, 0x052C, IntPtr.Zero, IntPtr.Zero, Native.SMTO_NORMAL, 1000, out _);

            // Ventana hija, sin bordes.
            long style = Native.GetWindowLongPtr(hwnd, Native.GWL_STYLE).ToInt64();
            style &= ~(Native.WS_POPUP | Native.WS_CAPTION | Native.WS_THICKFRAME);
            style |= Native.WS_CHILD;
            Native.SetWindowLongPtr(hwnd, Native.GWL_STYLE, new IntPtr(style));
            Native.AddExStyle(hwnd, Native.WS_EX_TOOLWINDOW | Native.WS_EX_NOACTIVATE, Native.WS_EX_APPWINDOW);

            IntPtr raisedWorker = Native.FindWindowEx(progman, IntPtr.Zero, "WorkerW", null);
            IntPtr defView = Native.FindWindowEx(progman, IntPtr.Zero, "SHELLDLL_DefView", null);
            IntPtr parent;

            if (raisedWorker != IntPtr.Zero)
            {
                // Windows 11 24H2+: Progman contiene los íconos (DefView) y el WorkerW.
                Log.Write($"Escritorio elevado (24H2+). WorkerW=0x{raisedWorker.ToInt64():X} DefView=0x{defView.ToInt64():X} modo={Mode}");
                if (Mode == "progman")
                {
                    Native.AddExStyle(hwnd, Native.WS_EX_LAYERED);
                    Native.SetLayeredWindowAttributes(hwnd, 0, 255, Native.LWA_ALPHA);
                    Native.SetParent(hwnd, progman);
                    Native.SetWindowPos(hwnd, defView, 0, 0, 0, 0, Native.SWP_NOMOVE | Native.SWP_NOSIZE | Native.SWP_NOACTIVATE);
                    parent = progman;
                }
                else
                {
                    Native.SetParent(hwnd, raisedWorker);
                    parent = raisedWorker;
                }
            }
            else
            {
                // Escritorio clásico: el WorkerW que sigue a la ventana que contiene SHELLDLL_DefView.
                IntPtr worker = IntPtr.Zero;
                Native.EnumWindows((top, _) =>
                {
                    IntPtr shell = Native.FindWindowEx(top, IntPtr.Zero, "SHELLDLL_DefView", null);
                    if (shell != IntPtr.Zero)
                        worker = Native.FindWindowEx(IntPtr.Zero, top, "WorkerW", null);
                    return true;
                }, IntPtr.Zero);
                Log.Write($"Escritorio clásico. WorkerW=0x{worker.ToInt64():X}");
                if (worker == IntPtr.Zero) worker = progman;
                Native.SetParent(hwnd, worker);
                parent = worker;
            }

            FitTo(hwnd, parent, screenRect);
            Native.GetWindowRect(hwnd, out var r);
            Log.Write($"Pegado a 0x{parent.ToInt64():X}. Rect final: {r.Left},{r.Top} - {r.Right},{r.Bottom}");
            return parent;
        }

        /// <summary>Coloca la ventana sobre un rectángulo de pantalla (píxeles físicos).</summary>
        public static void FitTo(IntPtr hwnd, IntPtr parent, Rectangle rect)
        {
            var pt = new Native.POINT { X = rect.X, Y = rect.Y };
            Native.ScreenToClient(parent, ref pt);
            Native.SetWindowPos(hwnd, IntPtr.Zero, pt.X, pt.Y, rect.Width, rect.Height,
                Native.SWP_NOZORDER | Native.SWP_NOACTIVATE | Native.SWP_FRAMECHANGED | Native.SWP_SHOWWINDOW);
        }

        /// <summary>Vuelve a pintar el fondo normal de Windows (al salir).</summary>
        public static void RestoreWindowsWallpaper()
        {
            try
            {
                var sb = new StringBuilder(520);
                Native.SystemParametersInfo(Native.SPI_GETDESKWALLPAPER, (uint)sb.Capacity, sb, 0);
                Native.SystemParametersInfo(Native.SPI_SETDESKWALLPAPER, 0, sb.ToString(), Native.SPIF_UPDATEINIFILE);
            }
            catch { }
        }
    }
}
