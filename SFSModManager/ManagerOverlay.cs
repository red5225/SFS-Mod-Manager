using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SFSModManager
{
    internal sealed class ModRow
    {
        public string Path;
        public string Name;
        public string Version;
        public bool Enabled;
        public string Dependencies;
        public string Missing;
    }

    public sealed class ManagerOverlay : MonoBehaviour
    {
        private readonly List<ModRow> _mods = new List<ModRow>();
        private Vector2 _scroll;
        private bool _open;
        private string _status = "Ready";
        private string _modsFolder;
        private string _settingsPath;
        private const long MaxArchiveBytes = 200L * 1024L * 1024L;

        private void Awake()
        {
            _modsFolder = ResolveModsFolder();
            _settingsPath = Path.Combine(_modsFolder, ".sfsmodmanager.settings");
            Refresh();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F7)) _open = !_open;
        }

        private void OnGUI()
        {
            if (GUI.Button(new Rect(12, Screen.height - 48, 118, 34), "Mods  [F7]")) _open = !_open;
            if (!_open) return;

            float width = Mathf.Min(650f, Screen.width - 32f);
            float height = Mathf.Min(560f, Screen.height - 72f);
            Rect window = new Rect((Screen.width - width) / 2f, (Screen.height - height) / 2f, width, height);
            GUI.Box(window, "SFS Mod Manager");
            GUILayout.BeginArea(new Rect(window.x + 12, window.y + 30, window.width - 24, window.height - 42));
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh", GUILayout.Width(90))) Refresh();
            if (GUILayout.Button("Open Imports Folder", GUILayout.Width(150))) OpenImports();
            if (GUILayout.Button("Install queued ZIPs", GUILayout.Width(145))) InstallQueued();
            if (GUILayout.Button("Reset App", GUILayout.Width(95))) ResetApp();
            GUILayout.EndHorizontal();
            GUILayout.Label("Mods folder: " + _modsFolder);
            GUILayout.Label(_status);
            GUILayout.Space(6);
            _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.ExpandHeight(true));
            foreach (var mod in _mods.ToArray())
            {
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.BeginHorizontal();
                GUILayout.Label(mod.Name + "  " + mod.Version, GUILayout.ExpandWidth(true));
                if (GUILayout.Button(mod.Enabled ? "Disable" : "Enable", GUILayout.Width(82))) Toggle(mod);
                GUILayout.EndHorizontal();
                if (!String.IsNullOrEmpty(mod.Dependencies)) GUILayout.Label("Dependencies: " + mod.Dependencies);
                if (!String.IsNullOrEmpty(mod.Missing)) GUILayout.Label("Missing: " + mod.Missing);
                GUILayout.EndVertical();
            }
            GUILayout.EndScrollView();
            if (GUILayout.Button("Close", GUILayout.Height(30))) _open = false;
            GUILayout.EndArea();
        }

        private string ResolveModsFolder()
        {
            var candidates = new List<string>();
            string cwd = Environment.CurrentDirectory;
            candidates.Add(Path.Combine(cwd, "Mods"));
            candidates.Add(Path.Combine(cwd, "MODS"));
            string data = Application.dataPath;
            if (!String.IsNullOrEmpty(data))
            {
                string parent = Directory.GetParent(data)?.FullName;
                if (!String.IsNullOrEmpty(parent))
                {
                    candidates.Add(Path.Combine(parent, "Mods"));
                    candidates.Add(Path.Combine(parent, "MODS"));
                    candidates.Add(Path.Combine(parent, "Contents", "Mods"));
                    candidates.Add(Path.Combine(parent, "Contents", "MODS"));
                }
            }
            var existing = candidates.FirstOrDefault(Directory.Exists);
            string chosen = existing ?? candidates[0];
            Directory.CreateDirectory(chosen);
            Directory.CreateDirectory(Path.Combine(chosen, "Imports"));
            return chosen;
        }

        private void Refresh()
        {
            try
            {
                _mods.Clear();
                foreach (string file in Directory.EnumerateFiles(_modsFolder, "*", SearchOption.AllDirectories))
                {
                    string lower = file.ToLowerInvariant();
                    if (!(lower.EndsWith(".dll") || lower.EndsWith(".dll.disabled"))) continue;
                    if (Path.GetFileName(file).Equals("SFSModManager.dll", StringComparison.OrdinalIgnoreCase)) continue;
                    bool enabled = lower.EndsWith(".dll");
                    string assemblyPath = enabled ? file : file.Substring(0, file.Length - ".disabled".Length);
                    var row = new ModRow { Path = file, Name = Path.GetFileName(assemblyPath), Version = "Unknown", Enabled = enabled };
                    if (enabled)
                    {
                        try { row.Version = AssemblyName.GetAssemblyName(file).Version?.ToString() ?? "Unknown"; }
                        catch { row.Version = "Unreadable"; }
                    }
                    string info = Path.ChangeExtension(assemblyPath, ".modinfo");
                    if (File.Exists(info))
                    {
                        foreach (string line in File.ReadAllLines(info))
                        {
                            if (line.StartsWith("dependencies=", StringComparison.OrdinalIgnoreCase)) row.Dependencies = line.Substring("dependencies=".Length).Trim();
                        }
                    }
                    _mods.Add(row);
                }
                var installed = new HashSet<string>(_mods.Where(m => m.Enabled).Select(m => Path.GetFileName(m.Path)), StringComparer.OrdinalIgnoreCase);
                foreach (var row in _mods.Where(m => !String.IsNullOrEmpty(m.Dependencies)))
                {
                    var missing = row.Dependencies.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).Where(x => !installed.Contains(x)).ToArray();
                    row.Missing = String.Join(", ", missing);
                }
                _mods.Sort((a, b) => String.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
                _status = "Found " + _mods.Count + " mod DLL(s).";
            }
            catch (Exception ex) { _status = "Refresh failed: " + ex.Message; }
        }

        private void Toggle(ModRow mod)
        {
            try
            {
                string target = mod.Enabled ? mod.Path + ".disabled" : mod.Path.Substring(0, mod.Path.Length - ".disabled".Length);
                if (File.Exists(target)) throw new IOException("Target already exists: " + target);
                File.Move(mod.Path, target);
                Refresh();
                _status = "Changed " + mod.Name + ". Restart SFS to apply.";
            }
            catch (Exception ex) { _status = "Could not change mod: " + ex.Message; }
        }

        private void InstallQueued()
        {
            string imports = Path.Combine(_modsFolder, "Imports");
            int count = 0;
            try
            {
                foreach (string zip in Directory.GetFiles(imports, "*.zip", SearchOption.TopDirectoryOnly))
                {
                    string stage = Path.Combine(imports, ".stage_" + Guid.NewGuid().ToString("N"));
                    Directory.CreateDirectory(stage);
                    try
                    {
                        ExtractSafe(zip, stage);
                        MergeWithoutOverwrite(stage, _modsFolder);
                        string done = Path.Combine(imports, "Installed");
                        Directory.CreateDirectory(done);
                        string destination = Path.Combine(done, Path.GetFileName(zip));
                        if (File.Exists(destination)) destination = Path.Combine(done, Guid.NewGuid().ToString("N") + "_" + Path.GetFileName(zip));
                        File.Move(zip, destination);
                        count++;
                    }
                    finally { if (Directory.Exists(stage)) Directory.Delete(stage, true); }
                }
                Refresh();
                _status = "Installed " + count + " ZIP package(s). Restart SFS to load code mods.";
            }
            catch (Exception ex) { _status = "Install stopped: " + ex.Message; }
        }

        private static void ExtractSafe(string zipPath, string destination)
        {
            long total = 0;
            using (var archive = ZipFile.OpenRead(zipPath))
            {
                foreach (var entry in archive.Entries)
                {
                    string normalized = entry.FullName.Replace('\\', '/');
                    if (normalized.StartsWith("/", StringComparison.Ordinal) || normalized.Split('/').Any(p => p == "..")) throw new InvalidDataException("Unsafe ZIP entry path: " + entry.FullName);
                    if (String.IsNullOrEmpty(normalized)) continue;
                    string target = Path.GetFullPath(Path.Combine(destination, normalized.Replace('/', Path.DirectorySeparatorChar)));
                    string root = Path.GetFullPath(destination) + Path.DirectorySeparatorChar;
                    if (!target.StartsWith(root, StringComparison.Ordinal)) throw new InvalidDataException("ZIP entry escapes destination.");
                    if (String.IsNullOrEmpty(entry.Name)) { Directory.CreateDirectory(target); continue; }
                    total += entry.Length;
                    if (total > MaxArchiveBytes) throw new InvalidDataException("ZIP exceeds the 200 MB extraction limit.");
                    Directory.CreateDirectory(Path.GetDirectoryName(target));
                    using (var input = entry.Open())
                    using (var output = new FileStream(target, FileMode.CreateNew, FileAccess.Write, FileShare.None)) input.CopyTo(output);
                }
            }
        }

        private static void MergeWithoutOverwrite(string source, string destination)
        {
            foreach (string dir in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
            {
                string relative = dir.Substring(source.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                Directory.CreateDirectory(Path.Combine(destination, relative));
            }
            foreach (string file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
            {
                string relative = file.Substring(source.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                string target = Path.Combine(destination, relative);
                Directory.CreateDirectory(Path.GetDirectoryName(target));
                if (File.Exists(target)) throw new IOException("Install would overwrite existing file: " + relative);
                File.Move(file, target);
            }
        }

        private void OpenImports()
        {
            try
            {
                string path = Path.Combine(_modsFolder, "Imports");
                Directory.CreateDirectory(path);
                if (Application.platform == RuntimePlatform.OSXPlayer) Process.Start("open", "\"" + path + "\"");
                else if (Application.platform == RuntimePlatform.WindowsPlayer) Process.Start("explorer.exe", "\"" + path + "\"");
                else _status = "Imports folder: " + path;
            }
            catch (Exception ex) { _status = "Could not open folder: " + ex.Message; }
        }

        private void ResetApp()
        {
            try
            {
                if (File.Exists(_settingsPath)) File.Delete(_settingsPath);
                _status = "Manager preferences reset. Installed mods and saves were not touched.";
                Refresh();
            }
            catch (Exception ex) { _status = "Reset failed: " + ex.Message; }
        }
    }
}
