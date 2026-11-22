# SFS Mod Manager

An in-game mod manager for Spaceflight Simulator, implemented against the public SFS ModLoader API.

## Features in this build

- In-game `Mods` panel toggle (F7) with a visible launcher button
- Scans the SFS Mods directory, including nested mod folders
- Lists mod DLLs and enabled/disabled state
- Shows declared dependencies from optional `.modinfo` sidecar files and warns about missing dependencies
- Enables/disables individual DLL mods by renaming `.dll` to `.dll.disabled` (restart SFS after changing state)
- Installs ZIP packages queued in `Mods/Imports`
- Refreshes the mod list
- Opens the Imports folder in Finder on macOS or Explorer on Windows
- Reset App resets only this manager's preferences; it does not remove mods, saves, or game files

## Build on macOS (Intel or Apple silicon)

You can build the game mod locally on a Mac. Xcode and an Apple Developer account are not required. You do need Mono's MSBuild tools and the reference assemblies from your own SFS installation.

1. Install Mono for macOS. If you already use Homebrew, `brew install mono` is one option. Otherwise use the official Mono installation instructions for macOS.
2. Download or clone this repository.
3. Open your SFS installation folder and locate `Spaceflight Simulator.app/Contents/Resources/Data/Managed` (the exact bundle layout can vary by distribution). Copy these four files into this repository's `Dependencies` folder:
   - `ModLoader.dll`
   - `UnityEngine.dll`
   - `UnityEngine.CoreModule.dll`
   - `UnityEngine.IMGUIModule.dll`
4. In Terminal, `cd` into the repository folder.
5. Run `chmod +x build-mod.sh` once, then run `./build-mod.sh`.
6. If the build succeeds, the finished mod is `artifacts/SFSModManager.dll`. Copy that DLL into the SFS `Mods` folder and restart the game.

The script checks that all four reference files exist before compiling and stops with a clear list if any are missing. It uses `msbuild` when available and falls back to `xbuild`.

## Build on Windows

Copy the same four reference DLLs from your own SFS `Spaceflight Simulator_Data/Managed` directory into `Dependencies/`, then build `SFSModManager/SFSModManager.csproj` with Visual Studio Build Tools / MSBuild using the Release configuration. The output is `artifacts/SFSModManager.dll`.

## Public API basis

The entry point follows the public SFS ModLoader pattern: derive from `Mod`, override metadata, and implement `Early_Load()` / `Load()`. See `docs/PUBLIC_REFERENCES.md`.

## Important limitations

- This is a local folder manager, not a remote mod marketplace or online mod database.
- Dependency warnings are best-effort checks of `.modinfo` text files; they do not guarantee binary compatibility or resolve arbitrary assembly dependencies.
- The manager cannot safely hot-reload arbitrary code mods. Restart SFS after installing, enabling, or disabling a code mod.
- The overlay is a floating in-game panel, not a replacement for or integration into the game's official main-menu tab.
- The source has not yet been verified in a live SFS game session. Build success alone does not prove runtime compatibility with every SFS version.

## Safety

DLL mods execute code inside the game process. Install only mods you trust. ZIP installation rejects path-traversal entries and avoids overwriting existing files.

## License

MIT for original code in this repository. SFS and Unity reference assemblies remain the property of their respective owners and must not be redistributed here.
