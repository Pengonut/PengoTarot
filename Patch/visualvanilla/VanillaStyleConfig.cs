#nullable enable

using System;
using System.IO;
using System.Text.Json;
using Godot;

namespace PengoTarot.Patch.VisualVanilla
{
    /// <summary>
    /// Global vanilla-style toggle by card family (not per card).
    /// TarotVanilla applies to ALL Tarot cards; PlanetVanilla to ALL Planet cards.
    /// </summary>
    public static class VanillaStyleConfig
    {
        private static readonly string FolderPath = Path.Combine(OS.GetUserDataDir(), "mod_configs", "PengoTarot");
        private static readonly string FilePath = Path.Combine(FolderPath, "VanillaStyleSettings.json");

        private class Data { public bool TarotVanilla { get; set; } public bool PlanetVanilla { get; set; } }
        private static Data _data = new();
        private static bool _loaded;

        public static bool TarotVanilla
        {
            get { Load(); return _data.TarotVanilla; }
            set { Load(); _data.TarotVanilla = value; Save(); }
        }
        public static bool PlanetVanilla
        {
            get { Load(); return _data.PlanetVanilla; }
            set { Load(); _data.PlanetVanilla = value; Save(); }
        }

        private static void Load()
        {
            if (_loaded) return;
            _loaded = true;
            try
            {
                if (!File.Exists(FilePath)) return;
                var json = File.ReadAllText(FilePath);
                var d = JsonSerializer.Deserialize<Data>(json);
                if (d != null) _data = d;
            }
            catch (Exception e) { GD.PrintErr($"[PengoTarot] VanillaStyle load: {e.Message}"); }
        }

        private static void Save()
        {
            try
            {
                if (!Directory.Exists(FolderPath)) Directory.CreateDirectory(FolderPath);
                File.WriteAllText(FilePath, JsonSerializer.Serialize(_data));
            }
            catch (Exception e) { GD.PrintErr($"[PengoTarot] VanillaStyle save: {e.Message}"); }
        }
    }
}
