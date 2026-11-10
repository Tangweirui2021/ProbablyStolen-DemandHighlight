#!/usr/bin/env bash
# SDK-only tests: no game binaries are required. Integration uses explicitly labelled stand-in APIs.
set -euo pipefail
cd "$(dirname "$0")/.."
DOTNET="${DOTNET:-dotnet}"
failed=0
for test in tests/*Test.cs; do
  name=$(basename "$test" .cs)
  printf '\n== %s\n' "$name"
  if "$DOTNET" build tests/Headless.csproj -c Release "-p:TestName=$name" --nologo -v quiet; then
    "$DOTNET" "tests/bin/Release/net8.0/$name.dll" || failed=1
  else failed=1; fi
done
printf '\n== Integration (stand-in game/UI APIs; not a real-game compatibility check)\n'
if "$DOTNET" build tests/Integration.csproj -c Release --nologo -v quiet; then
  "$DOTNET" tests/bin/Release/net8.0/Integration.dll || failed=1
else failed=1; fi
exit "$failed"
