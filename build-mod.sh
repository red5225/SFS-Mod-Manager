#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")" && pwd)"
cd "$ROOT"

PROJECT="SFSModManager/SFSModManager.csproj"
OUT="$ROOT/artifacts"
REQUIRED=(
  "Dependencies/ModLoader.dll"
  "Dependencies/UnityEngine.dll"
  "Dependencies/UnityEngine.CoreModule.dll"
  "Dependencies/UnityEngine.IMGUIModule.dll"
)

missing=()
for file in "${REQUIRED[@]}"; do
  [[ -f "$file" ]] || missing+=("$file")
done
if (( ${#missing[@]} )); then
  echo "Missing SFS reference assemblies:" >&2
  printf '  - %s\n' "${missing[@]}" >&2
  echo "Copy these files from your own SFS installation's Spaceflight Simulator_Data/Managed folder into Dependencies/." >&2
  exit 1
fi

mkdir -p "$OUT"
if command -v msbuild >/dev/null 2>&1; then
  BUILDER=(msbuild)
elif command -v xbuild >/dev/null 2>&1; then
  BUILDER=(xbuild)
else
  echo "No MSBuild found. Install Mono (which includes MSBuild or xbuild), then run this script again." >&2
  exit 1
fi

"${BUILDER[@]}" "$PROJECT" /t:Rebuild /p:Configuration=Release /verbosity:minimal

DLL="$OUT/SFSModManager.dll"
if [[ ! -f "$DLL" ]]; then
  echo "Build command finished, but $DLL was not created." >&2
  exit 1
fi

echo "\nBuild successful: $DLL"
echo "Copy this DLL into the SFS Mods folder. Restart SFS before testing it."
