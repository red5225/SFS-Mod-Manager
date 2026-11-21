#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")" && pwd)"
cd "$ROOT"
mkdir -p artifacts
if command -v msbuild >/dev/null 2>&1; then
  msbuild SFSModManager/SFSModManager.csproj /p:Configuration=Release /verbosity:minimal
elif command -v xbuild >/dev/null 2>&1; then
  xbuild SFSModManager/SFSModManager.csproj /p:Configuration=Release /verbosity:minimal
else
  echo "Install Mono MSBuild (or Visual Studio Build Tools) first." >&2
  exit 2
fi
echo "Built: $ROOT/artifacts/SFSModManager.dll"
