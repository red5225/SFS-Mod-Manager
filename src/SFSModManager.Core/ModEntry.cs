using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SFSModManager
{
    public sealed class ModEntry
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string FilePath { get; set; } = "";
        public bool Enabled { get; set; }
        public string Version { get; set; } = "Unknown";
        public string Error { get; set; } = "";
    }

    /// <summary>Filesystem-level catalog. It inspects assembly metadata without loading mod code.</summary>
    public sealed class ModCatalog
    {
        public IReadOnlyList<ModEntry> Scan(string modsDirectory)
        {
            if (string.IsNullOrWhiteSpace(modsDirectory)) throw new ArgumentException("Mods directory is required.", nameof(modsDirectory));
            if (!Directory.Exists(modsDirectory)) Directory.CreateDirectory(modsDirectory);
            var result = new List<ModEntry>();
            foreach (var file in Directory.EnumerateFiles(modsDirectory, "*", SearchOption.TopDirectoryOnly)
                         .Where(p => p.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) || p.EndsWith(".dll.disabled", StringComparison.OrdinalIgnoreCase)))
            {
                var enabled = file.EndsWith(".dll", StringComparison.OrdinalIgnoreCase);
                var entry = new ModEntry { FilePath = file, Enabled = enabled, Id = Path.GetFileName(file), Name = Path.GetFileNameWithoutExtension(file) };
                try
                {
                    var inspectPath = enabled ? file : file.Substring(0, file.Length - ".disabled".Length);
                    var name = AssemblyName.GetAssemblyName(inspectPath);
                    entry.Name = name.Name ?? entry.Name;
                    entry.Version = name.Version?.ToString() ?? "Unknown";
                }
                catch (Exception ex) { entry.Error = "Could not read assembly metadata: " + ex.Message; }
                result.Add(entry);
            }
            return result.OrderBy(m => m.Name, StringComparer.OrdinalIgnoreCase).ToArray();
        }

        public void SetEnabled(ModEntry mod, bool enabled)
        {
            if (mod == null) throw new ArgumentNullException(nameof(mod));
            var current = mod.FilePath;
            var target = enabled
                ? (current.EndsWith(".disabled", StringComparison.OrdinalIgnoreCase) ? current.Substring(0, current.Length - ".disabled".Length) : current)
                : (current.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) ? current + ".disabled" : current);
            if (String.Equals(current, target, StringComparison.Ordinal)) return;
            if (File.Exists(target)) throw new IOException("A file already exists at the target path: " + target);
            File.Move(current, target);
        }
    }
}
