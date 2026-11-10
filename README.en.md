# Type Demand & Risk Stars

Customer-demand outlines and permanent risk stars for *Probably Stolen*.

[简体中文](README.md) · [Manual](doc/manual/index.md) · [Configuration](doc/manual/configuration.md) · [Building](doc/manual/building.md)

This is [Tangweirui2021's fork](https://github.com/Tangweirui2021/ProbablyStolen-DemandHighlight) of [Demand Highlight by KCN-judu](https://github.com/KCN-judu/ProbablyStolen-DemandHighlight), with a separate mod ID, `wrt_type_demand_stars`. Attribution and the Apache-2.0 license are in [NOTICE](NOTICE) and [LICENSE](LICENSE).

> Source is available; a real-game build and gameplay validation against the player's current assemblies are still pending. No game files or installable release DLL are included. The manifest retains the upstream `0.46D` declaration; it does not establish compatibility with the current full release.

![Schematic rendered from the code's actual masks, not a game screenshot](preview.png)

## Behavior

| Marker | When shown | Default color |
| --- | --- | --- |
| Full demand outline | The customer accepts the item, or the manual filter matches it | Configurable per product type |
| Container corner brackets | The container itself does not match, but something inside does | One configurable pale blue |
| Top-right pixel star | The item itself is stolen only | Configurable yellow |
| Top-right pixel star | The item itself is contraband only | Configurable green |
| Top-right pixel star | The item itself has both statuses | Configurable red |

Legal status does not change the demand outline's color, pattern or fill intensity. Customer acceptance, variety and barter rules are retained. A container that is itself wanted receives its own type-colored full outline.

Stars run independently of the current customer, demand hotkey and shop inventory roots. Native item views created or validated on counters, customer goods, showcases, containers, machines and other scenes are registered. An ordinary container does not inherit its contents' risk star.

Outline thickness, rounding, faint fill, the Item Filter, statistics and key bindings remain available. Type priority, every marker color, star pixel scale and corner inset are configurable. Pixel stars use point-filtered masks and ignore pointer input.

## Settings

Enable the mod, then use **main menu → Mods → this mod's gear button**. Colors are hexadecimal text fields (`#RRGGBB` or `#RRGGBBAA`), applied with Apply. The native panel's availability from the shop pause menu has not been confirmed.

The game stores settings in `<Application.persistentDataPath>/ModConfigs/wrt_type_demand_stars.json`. Exit the game before editing it manually and retain the generated JSON structure. No separate YAML file is required.

A multi-type item uses the first type in `typePriority`; unlisted types have a stable fallback order. `CONTRABAND`, `STOLEN` and `RESTRICTED_*` never choose a frame color. See [all settings](doc/manual/configuration.md).

Both hotkeys are initially unbound. The demand/filter-outline hotkey does not disable stars; the permanent star layer has its own setting.

## Build and install

Use .NET SDK 8 or newer and the current game's `Probably Stolen_Data/Managed` assemblies.

```bash
GAME='/path/to/Probably Stolen' ./build.sh --install
```

```powershell
./build.ps1 -GamePath 'D:\SteamLibrary\steamapps\common\Probably Stolen' -Install
```

Output: `out/wrt_type_demand_stars/`. Copy that whole folder into the game's `Mods/` if installing manually. Disable the original Demand Highlight and overlapping marker mods, enable this fork and restart. See [Building](doc/manual/building.md).

## Validation and limitations

```bash
./tests/run.sh
python3 tools/check_docs.py
```

Twelve pure-data suites and a stand-in integration suite exercise the fork. See [VALIDATION.md](VALIDATION.md) for results and unverified items. These tests do not replace a build against real game assemblies or real Harmony/Unity validation.

Entirely custom item interfaces that bypass native `GameItemElement` views need an adapter. Rotation, mirroring, irregular shapes and clipping still need real-game verification.

The inherited query API keeps its 1.x contract and names. Its whole-shop query scope differs from the stars' global item-view registry. [Manual](doc/manual/index.md) · [Changes](CHANGELOG.md) · [Report a problem](https://github.com/Tangweirui2021/ProbablyStolen-DemandHighlight/issues)
