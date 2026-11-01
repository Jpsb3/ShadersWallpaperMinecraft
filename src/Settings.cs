using System;
using System.IO;
using System.Text.Json;

namespace ShadersWallpaper
{
    /// <summary>Ajustes opcionales (settings.json junto al .exe). Si no existe, usa estos valores.</summary>
    public class Settings
    {
        public string WallpaperFolder { get; set; } = "wallpaper";
        public string Page { get; set; } = "index.html";
        public string AttachMode { get; set; } = "auto";
        public bool PauseOnFullscreen { get; set; } = true;
        public bool PauseOnMaximized { get; set; } = true;
        public bool PauseOnBattery { get; set; } = true;

        public static string BaseDir => AppContext.BaseDirectory;

        public string FolderPath => Path.IsPathRooted(WallpaperFolder)
            ? WallpaperFolder
            : Path.GetFullPath(Path.Combine(BaseDir, WallpaperFolder));

        public static Settings Load()
        {
            string path = Path.Combine(BaseDir, "settings.json");
            if (!File.Exists(path)) return new Settings();
            try
            {
                return JsonSerializer.Deserialize<Settings>(File.ReadAllText(path), new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                }) ?? new Settings();
            }
            catch (Exception ex)
            {
                Log.Write("settings.json inválido: " + ex.Message);
                return new Settings();
            }
        }
    }

    internal static class Log
    {
        private static readonly string PathFile = System.IO.Path.Combine(AppContext.BaseDirectory, "log.txt");
        private static bool _started;

        public static void Write(string msg)
        {
            try
            {
                if (!_started) { File.WriteAllText(PathFile, ""); _started = true; }
                File.AppendAllText(PathFile, $"[{DateTime.Now:HH:mm:ss}] {msg}\r\n");
            }
            catch { }
        }
    }
}
