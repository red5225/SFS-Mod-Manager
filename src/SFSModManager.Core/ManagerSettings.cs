using System;
using System.IO;

namespace SFSModManager
{
    /// <summary>Manager-only preferences. Reset intentionally leaves mods, saves, and game files untouched.</summary>
    public sealed class ManagerSettings
    {
        public bool ShowDisabledMods { get; set; } = true;
        public bool ConfirmBeforeInstall { get; set; } = true;
        public static ManagerSettings Defaults() => new ManagerSettings();

        public static void Reset(string settingsPath)
        {
            if (String.IsNullOrWhiteSpace(settingsPath)) throw new ArgumentException("Settings path is required.", nameof(settingsPath));
            var directory = Path.GetDirectoryName(settingsPath);
            if (!String.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            File.WriteAllText(settingsPath, "{\n  \"showDisabledMods\": true,\n  \"confirmBeforeInstall\": true\n}\n");
        }
    }
}
