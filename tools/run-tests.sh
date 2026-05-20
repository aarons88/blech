#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
: "${UNITY_BIN:?UNITY_BIN must be set}"
cd "$ROOT"

"$UNITY_BIN" -batchmode -projectPath . \
  -runTests -testPlatform EditMode \
  -testResults TestResults.xml \
  -logFile -
