# Validation — 2026-11-10

Upstream base: `ee0e791669833239a3c636aeeb20810970ecf3e6`
SDK: .NET 8.0.427, Linux x86-64.
Game reference internal label: `VERSION 100-Rev13`.
Assembly-CSharp SHA-256: `725cb999d019dff1ff31bed353c7f9bb8d81a7605a15de1f80f5a3a6cf7b1d05`

## Passed

- Production mod compiled against the supplied real game assemblies: zero warnings/errors.
- Optional query sample compiled against those same assemblies: zero warnings/errors.
- Real-assembly preflight confirmed 34 API targets, including constructors, destruction and native drag-visibility hooks.
- The supplied game's own `ModScanner.Scan` scanned 541 methods: no refusal, no error, no capabilities and no hard flags.
- Thirteen pure-data test suites: 379 checks passed.
- Production-code integration against explicitly labelled stand-in game/Unity APIs: 117 checks passed.
- Trash/dock regressions cover closed/open fixed bins, removal of wanted contents, duplicate-free stock traversal, category/status selection, manual-query precedence, clearing, advanced-window synchronization, hidden inventory, no-customer use and scene cleanup.
- Rotation regressions use actual affine matrix math in the stand-ins: four directions with/without mirroring, non-square rectangles, asymmetric pivots, shifted nodes, scaled canvases, independent drag geometry and one-cell squares. Geometry refreshes are checked without advancing the fallback polling clock.
- Size/layout regressions cover one-cell items, large containers, grid-size changes, drag previews, top-right placement, the long priority field and isolation from other mods/settings.
- Drag regressions cover whole-stack pickup, partial stacks, drag opacity, combined status, settings, drop and destruction cleanup.
- Documentation links, 15 configuration keys, project/manifest XML, shell syntax and `git diff --check` passed.
- The installable package contains only this mod's DLL, localization, manifest, notices, preview and installation/build information. No uploaded game assemblies, test stand-ins or development tools are distributed.

## Player feedback and remaining gameplay checks

Player feedback confirms that the wrt.3 feature set is broadly working and identified a remaining rotated-star position bug. wrt.5 corrects badge placement using transformed footprint bounds. Actual in-game confirmation of this fix and the wrt.4 checkbox dock remains pending.

- Native dock layout and pointer interaction in the running game, especially narrow resolutions and resized inventories.
- Visual layout, scaling, clipping, rotation and mirroring in the running game.
- Integration with third-party item UIs that bypass native GameItemElement views.

The internal game revision label differs from Unity's Application.version field used by the mod menu's version display. The manifest temporarily retains the upstream `0.46D` declaration; a compile and scanner pass is not a claim that every gameplay path was exercised.

Reproduce the API/scanner check after building with:

```bash
./tools/check-game.sh /path/to/Managed
```
