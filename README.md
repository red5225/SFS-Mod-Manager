# SFS Mod Manager

An in-game mod manager for Spaceflight Simulator, implemented against the public SFS ModLoader API.

## Features in this build

- In-game `Mods` panel toggle (F7) with a visible launcher button
- Scan the SFS Mods directory, including nested mod folders
- Show mod DLL names, versions, declared dependencies, and missing dependency warnings
- Enable/disable individual DLL mods by renaming them to `.dll.disabled`
- Install ZIP packages queued in `Mods/Imports`
- Open the Imports folder in Finder on macOS
- Refresh the mod list
- Reset manager preferences without deleting installed mods, saves, or game files

## Public API basis

The entry point follows the public SFS ModLoader pattern: derive from `Mod`, override metadata, and implement `Early_Load()` / `Load()`. The project targets .NET Framework 4.8 and references the game's ModLoader and Unity assemblies locally. See `docs/PUBLIC_REFERENCES.md`.

## Build requirements

Game and loader reference assemblies are not redistributed. Put the following local DLLs in `Dependencies/` from your own SFS installation:

- `ModLoader.dll`
- `UnityEngine.dll`
- `UnityEngine.CoreModule.dll`
- `UnityEngine.IMGUIModule.dll`

The project does not need `Assembly-CSharp.dll` or Harmony because the manager uses Unity's immediate-mode GUI and the public `Mod` entrypoint. If your loader build requires additional reference DLLs, add them to `Dependencies/` and the project references.

On a Mac with Mono MSBuild installed, run `./build-mod.sh`. The output is `artifacts/SFSModManager.dll`. Copy that DLL into the SFS `Mods` folder and restart the game.

## ZIP installation

Put mod ZIPs in `Mods/Imports`, open the in-game manager, and select **Install queued ZIPs**. Archives are extracted to a staging folder first; path traversal entries are rejected. Existing files are not overwritten. Restart SFS after installing or changing the enabled state of a code mod.

## Reset behavior

**Reset App** resets only this manager's preferences. It deliberately does not delete installed mods, saves, game files, or import archives.

## Important limitations

- This is a local folder manager, not a remote mod marketplace/API client.
- A mod can declare dependencies in a sibling `.modinfo` text file using `dependencies=ModA.dll,ModB.dll`. Dependency warnings are best-effort and do not guarantee binary compatibility.
- The manager cannot hot-reload arbitrary code mods safely. Restart the game after install/enable/disable.
- GitHub Actions builds the dependency-free catalog core and packages the source. The actual in-game DLL must be built with the local SFS reference assemblies because those proprietary game/loader DLLs cannot be redistributed in this repository.

## Safety

DLL mods execute code inside the game process. Install only mods you trust. The installer rejects ZIP path traversal and does not overwrite existing files.

## License

MIT for original code in this repository. SFS and Unity reference assemblies remain the property of their respective owners.
