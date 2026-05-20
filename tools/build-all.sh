#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
: "${UNITY_BIN:?UNITY_BIN must be set (e.g. export UNITY_BIN=/Applications/Unity/Hub/Editor/6000.4.6f1/Unity.app/Contents/MacOS/Unity)}"
cd "$ROOT"

echo "[Blech] Generating all assets..."
"$UNITY_BIN" -batchmode -quit -projectPath . \
  -executeMethod Blech.Editor.BlechEditorMenu.BuildAll \
  -logFile -

echo "[Blech] Building Mac standalone..."
"$UNITY_BIN" -batchmode -quit -projectPath . \
  -executeMethod Blech.Editor.StandaloneBuild.BuildMac \
  -logFile -

echo "[Blech] Done. Builds/Blech_Mac/Blech.app should exist."
