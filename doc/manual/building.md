# Building and installation

[简体中文](../zh-CN/building.md) · [Documentation index](../README.md)

## Requirements and current status

- .NET SDK 8 or newer. Pure-data/integration tests target .NET 8; game mods target `netstandard2.1`.
- A copy of the current game and its `Probably Stolen_Data/Managed` folder.
- Linux/macOS shell, Git Bash, or PowerShell for the wrapper scripts.

The installable test package is supplied separately from this source repository. A real-assembly build and the game's own scanner passed using references with internal label `VERSION 100-Rev13`. No game files are redistributed. The upstream `0.46D` Unity application-version declaration is temporarily retained; that field cannot be inferred solely from the game DLL's internal revision label. Gameplay validation remains pending.

## Main mod

From the repository root:

```bash
GAME='/path/to/Probably Stolen' ./build.sh
GAME='/path/to/Probably Stolen' ./build.sh --install
```

On Windows:

```powershell
./build.ps1 -GamePath 'D:\SteamLibrary\steamapps\common\Probably Stolen'
./build.ps1 -GamePath 'D:\SteamLibrary\steamapps\common\Probably Stolen' -Install
```

Direct SDK invocation, including the case where only the Managed folder is available:

```bash
dotnet build TypeDemandStars.csproj -c Release -p:GamePath='/path/to/Probably Stolen'
dotnet build TypeDemandStars.csproj -c Release -p:GameManagedDir='/path/to/Managed'
```

The project fails early when `Assembly-CSharp.dll` is missing. It references the player's assemblies without copying them to output. No Visual Studio Build Tools `CSC` variable is needed.

Output: `out/wrt_type_demand_stars/`, containing `TypeDemandStars.dll`, its manifest, localization and notices. Do not install binaries from `tests/bin/`.

## Activate and update

Copy the entire output folder into `<game>/Mods/` if not using `--install`/`-Install`. Disable the original Demand Highlight and overlapping marker mods, enable **Type Demand & Risk Stars** in the game's Mods menu, then restart.

The source fork has no separate Workshop subscription yet; the original author's subscription installs the original mod. Updates replace the fork's install folder; saved settings remain in the game's external `ModConfigs` folder.

## Tests

```bash
./tests/run.sh
python3 tools/check_docs.py
```

The first command runs 12 pure-data suites and production-code integration against explicitly labelled stand-in APIs; it needs no game files. It does not validate actual game assembly compatibility, Harmony patching, Unity rotation or clipping. See [VALIDATION.md](../../VALIDATION.md).

## Query sample for mod authors

The optional development mod exercises the inherited query API and logs results in `Player.log`. It is not part of the player's mod output:

```bash
GAME='/path/to/Probably Stolen' ./tools/QuerySample/build.sh --install
```

It uses the same .NET SDK and game-assembly requirements. Enable the fork and the Query Sample to exercise the API; see [Query API](query-api.md).

## Real-assembly API/scanner check

After building the mod, run:

```bash
./tools/check-game.sh '/path/to/Managed'
```

This checks the exact Harmony target signatures and runs the supplied game build's `ModScanner.Scan` outside the game. It reads game assemblies but does not copy them into the mod or run the game's Unity engine. It does not exercise actual Harmony patch execution or render the UI. The checked build reported no extra capabilities or hard flags.
