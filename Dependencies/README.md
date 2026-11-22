# Local game reference assemblies

Before compiling the in-game DLL, copy these four files from your own SFS installation into this folder:

- `ModLoader.dll`
- `UnityEngine.dll`
- `UnityEngine.CoreModule.dll`
- `UnityEngine.IMGUIModule.dll`

On macOS, these are commonly located inside `Spaceflight Simulator.app/Contents/Resources/Data/Managed/`. On Windows, look in `Spaceflight Simulator_Data/Managed/`.

Use the assemblies from the same SFS installation/version you intend to run the mod with. These proprietary game/loader assemblies are intentionally not committed to this repository or included in GitHub Actions artifacts.
