#!/usr/bin/env bash
# Cross-platform .NET SDK build, using the player's own game assemblies.
set -euo pipefail
cd "$(dirname "$0")"
DOTNET="${DOTNET:-dotnet}"
GAME="${GAME:-}"
install=0
for arg in "$@"; do
  if [[ "$arg" == --install ]]; then install=1
  else printf 'Unknown argument: %s\n' "$arg" >&2; exit 2; fi
done
if [[ -z "$GAME" ]]; then
  printf '%s\n' 'Set GAME to the game install directory, e.g.:' 'GAME="/path/to/Probably Stolen" ./build.sh --install' >&2
  exit 2
fi
"$DOTNET" build TypeDemandStars.csproj -c Release "-p:GamePath=$GAME" --nologo
if ((install)); then
  mkdir -p "$GAME/Mods/wrt_type_demand_stars"
  cp -R out/wrt_type_demand_stars/. "$GAME/Mods/wrt_type_demand_stars/"
  printf '%s\n' "Installed to $GAME/Mods/wrt_type_demand_stars" 'Disable the original Demand Highlight and other duplicate highlighting mods; enable this fork and restart.'
fi
