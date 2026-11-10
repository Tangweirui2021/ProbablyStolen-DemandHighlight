# Validation — 2026-11-10

Base: `ee0e791669833239a3c636aeeb20810970ecf3e6`
SDK: .NET 8.0.427, Linux x86-64.

- Twelve pure-data test suites: 364 checks passed.
- Production-code integration with explicitly labelled stand-in game/Unity APIs: 34 checks passed.
- Integrated source compiled with zero warnings/errors against those stand-in APIs.
- Shell syntax and `git diff --check` passed.
- `python3 tools/check_docs.py`: 19 current documents, local links and 15 configuration keys passed.
- Main/sample project and manifest XML files parsed.
- Documentation is maintained in Chinese and English directly in this fork; obsolete generated catalogs and diagrams were removed.
- Preview rendered from actual C# alpha masks and visually inspected.
- A production build without game DLLs intentionally stops with a clear missing-assemblies error.

## Not yet verified

- Build against the player's current `Probably Stolen_Data/Managed` assemblies.
- Real Harmony constructor hooks and real Unity layout/rotation/mirroring/clipping.
- Game version compatibility beyond the upstream manifest's declared `0.46D`.
- Custom mod UIs that do not create/validate native GameItemElement views.

The source archive is not an installable game DLL. No game files or test stand-in binaries are included in the mod output.
