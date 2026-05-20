#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
THIRDPARTY_SRC="${1:-$HOME/Downloads/blech_thirdparty}"

if [ -z "${UNITY_BIN:-}" ]; then
  echo "Set UNITY_BIN to the path of your Unity 6 binary, e.g.:"
  echo "  export UNITY_BIN=/Applications/Unity/Hub/Editor/6000.4.6f1/Unity.app/Contents/MacOS/Unity"
  exit 1
fi

cd "$ROOT"
mkdir -p \
  Assets/Blech/_ThirdParty \
  Assets/Blech/Art/Characters \
  Assets/Blech/Art/Environment \
  Assets/Blech/Art/Shaders \
  Assets/Blech/Art/Materials \
  Assets/Blech/Art/Particles \
  Assets/Blech/Audio/SFX \
  Assets/Blech/Prefabs/Player \
  Assets/Blech/Prefabs/Hazards \
  Assets/Blech/Prefabs/Level \
  Assets/Blech/Prefabs/UI \
  Assets/Blech/Prefabs/FX \
  Assets/Blech/Scenes \
  Assets/Blech/Scripts/Player \
  Assets/Blech/Scripts/World \
  Assets/Blech/Scripts/UI \
  Assets/Blech/Scripts/Audio \
  Assets/Blech/ScriptableObjects/Characters \
  Assets/Blech/Editor \
  Assets/Blech/Tests/Edit \
  Builds

if [ ! -f ProjectSettings/ProjectVersion.txt ]; then
  echo "Generating Unity project via -createProject (may take a minute)..."
  "$UNITY_BIN" -batchmode -quit -createProject "$ROOT" -logFile - || true
fi

THIRDPARTY_DST="$ROOT/Assets/Blech/_ThirdParty"
if [ -d "$THIRDPARTY_SRC" ]; then
  for zip in "$THIRDPARTY_SRC"/*.zip; do
    [ -e "$zip" ] || continue
    name="$(basename "$zip" .zip)"
    if [ -d "$THIRDPARTY_DST/$name" ]; then
      echo "skip $name (already extracted)"
      continue
    fi
    echo "unzip $name"
    mkdir -p "$THIRDPARTY_DST/$name"
    ditto -x -k --sequesterRsrc "$zip" "$THIRDPARTY_DST/$name"
  done
else
  echo "(no third-party zips found at $THIRDPARTY_SRC — skipping import; generators will fall back to primitives)"
fi

echo "Bootstrap complete."
echo "Next: open the project in Unity (or run unity -batchmode -executeMethod Blech.Editor.BlechEditorMenu.BuildAll)"
