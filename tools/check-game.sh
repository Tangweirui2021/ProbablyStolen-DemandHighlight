#!/usr/bin/env bash
# Read-only checks against the player's game assemblies; does not start the game.
set -euo pipefail
cd "$(dirname "$0")/.."
DOTNET="${DOTNET:-dotnet}"
managed="${1:?Usage: tools/check-game.sh /path/to/Managed [built-mod-folder]}"
mod="${2:-out/wrt_type_demand_stars}"
"$DOTNET" build tools/GameCheck/GameCheck.csproj -c Release "-p:GameManagedDir=$managed" --nologo -v quiet
"$DOTNET" tools/GameCheck/bin/Release/net8.0/GameCheck.dll "$managed" "$mod"
