# Validation — 2026-11-10

Upstream base: `ee0e791669833239a3c636aeeb20810970ecf3e6`
SDK: .NET 8.0.427, Linux x86-64.
Game reference internal label: `VERSION 100-Rev13`.
Assembly-CSharp SHA-256: `725cb999d019dff1ff31bed353c7f9bb8d81a7605a15de1f80f5a3a6cf7b1d05`

## Passed

- Production mod compiled against the supplied real game assemblies: zero warnings/errors.
- Optional query sample compiled against those same assemblies: zero warnings/errors.
- Real-assembly preflight confirmed 20 API targets, including constructors, destruction and native drag-visibility hooks.
- The supplied game's own `ModScanner.Scan` scanned 514 methods: no refusal, no error, no capabilities and no hard flags.
- Twelve pure-data test suites: 364 checks passed.
- Production-code integration against explicitly labelled stand-in game/Unity APIs: 45 checks passed.
- Drag regressions cover whole-stack pickup, partial stacks, drag opacity, combined status, settings, drop and destruction cleanup.
- Documentation links, 15 configuration keys, project/manifest XML, shell syntax and `git diff --check` passed.
- The installable package contains only this mod's DLL, localization, manifest, notices, preview and installation/build information. No uploaded game assemblies, test stand-ins or development tools are distributed.

## Still requires actual gameplay

- Execution of real Harmony patches within the Unity game.
- Visual layout, scaling, clipping, rotation and mirroring in the running game.
- Integration with third-party item UIs that bypass native GameItemElement views.

The internal game revision label differs from Unity's Application.version field used by the mod menu's version display. The manifest temporarily retains the upstream `0.46D` declaration; a compile and scanner pass is not a claim that every gameplay path was exercised.

Reproduce the API/scanner check after building with:

```bash
./tools/check-game.sh /path/to/Managed
```
