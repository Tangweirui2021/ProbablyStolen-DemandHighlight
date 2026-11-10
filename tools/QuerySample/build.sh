#!/usr/bin/env bash
# Optional query API sample, built with the same .NET SDK as the fork.
set -euo pipefail
cd "$(dirname "$0")/../.."
DOTNET="${DOTNET:-dotnet}"
GAME="${GAME:-}"
install=0
for arg in "$@"; do
  if [[ "$arg" == --install ]]; then install=1
  else printf 'Unknown argument: %s\n' "$arg" >&2; exit 2; fi
done
if [[ -z "$GAME" ]]; then
  printf '%s\n' 'Set GAME to the current game install directory.' >&2
  exit 2
fi
"$DOTNET" build tools/QuerySample/QuerySample.csproj -c Release "-p:GamePath=$GAME" --nologo
if ((install)); then
  mkdir -p "$GAME/Mods/query_sample"
  cp -R out/query_sample/. "$GAME/Mods/query_sample/"
  printf '%s\n' "Installed optional development sample to $GAME/Mods/query_sample"
fi
