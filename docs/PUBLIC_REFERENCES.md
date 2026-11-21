# Public SFS modding references

The in-game adapter is based on publicly documented SFS modding conventions and public example projects:

- `105-Code/MalfunctionsMod`: public example of `ModLoader.Mod`, metadata overrides, `Early_Load()`, `Load()`, and Harmony/Unity references. The manager uses the loader entrypoint pattern but does not copy that project's implementation.
- `kojamori/SFS-Code-Mod-Template`: documents .NET Framework 4.8, `Mod` metadata, local SFS installation detection, and local game-reference assembly requirements.
- `Neptune-Sky/SFSModInstaller`: documents the in-game installer workflow and the convention of adding a mod DLL to the Mods folder.
- `105-Code/SFS-Modloader` wiki: documents the legacy Mac loader and Mods directory conventions. Paths differ between loader generations, so the manager checks common Mac and desktop paths.

Public source URLs:

- https://github.com/105-Code/MalfunctionsMod
- https://github.com/kojamori/SFS-Code-Mod-Template
- https://github.com/Neptune-Sky/SFSModInstaller
- https://github.com/105-Code/SFS-Modloader/wiki/Installation-guide

The manager is an independent implementation. No proprietary SFS assemblies are copied into this repository.
