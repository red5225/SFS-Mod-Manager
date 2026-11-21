# SFS Mod Manager

A custom mod-management core for Spaceflight Simulator. This repository is being built for the game's in-game Mods menu and a safe reset action.

## Current status

The first commit contains the standalone mod-catalog core plus a GitHub Actions build. It is **not yet a drop-in in-game mod DLL**: the exact SFS `Managed` assemblies and mod-loader API are required to compile the game-facing entry point and UI against your installation. I am deliberately not guessing those API signatures or bundling copyrighted game DLLs.

## Planned in-game features

- Scan the game's Mods directory and list detected DLL mods
- Install a mod from a ZIP archive
- Enable/disable individual mods
- Show missing dependency information where it can be determined safely
- Refresh the catalog
- Reset manager preferences without deleting installed mods or game files

## Build

GitHub Actions builds the dependency-free core library and uploads a ZIP artifact. To produce the final in-game DLL, add the required game/loader references locally under `Dependencies/` (do not commit proprietary game files), then the game adapter can be compiled against the exact API.

## Safety

Only install mods from sources you trust. DLL mods execute code inside the game process. Reset does not remove mods, saves, or game files.

## License

MIT for original source in this repository. Third-party game assemblies and mod files are not included.
