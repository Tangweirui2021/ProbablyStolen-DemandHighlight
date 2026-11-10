#!/usr/bin/env python3
"""Package a checked mod build without redistributing any game assemblies or test binaries."""
import argparse
import hashlib
import json
import re
import subprocess
from pathlib import Path
from zipfile import ZIP_DEFLATED, ZipFile

root = Path(__file__).resolve().parents[1]
parser = argparse.ArgumentParser()
parser.add_argument("--managed", required=True, type=Path)
parser.add_argument("--check-log", required=True, type=Path)
parser.add_argument("--output", required=True, type=Path)
parser.add_argument("--date", required=True, help="Release date in YYYY-MM-DD format")
parser.add_argument("--source-commit", help="Published source commit, or local HEAD by default")
args = parser.parse_args()
out = root / "out/wrt_type_demand_stars"
log = args.check_log.read_text()
assert "refused: False; error: none" in log and "Capabilities: none" in log, "Game scanner did not pass"
assert not re.search(r"^(HARD|FIND) ", log, re.M), "Unexpected scanner findings"
assert list(sorted(p.name for p in out.glob("*.dll"))) == ["TypeDemandStars.dll"], "Unexpected DLL in output"

def digest(data):
    return hashlib.sha256(data).hexdigest()

names = ["TypeDemandStars.dll", "manifest.xml", "preview.png", "LICENSE", "NOTICE"]
files = {name: (out / name).read_bytes() for name in names}
files.update({"Localization/" + p.name: p.read_bytes() for p in sorted((out / "Localization").glob("*.csv"))})
files["INSTALL.md"] = (root / "doc/installation.md").read_bytes()
refs = ["Assembly-CSharp.dll", "UnityEngine.CoreModule.dll", "UnityEngine.UI.dll", "0Harmony.dll"]
reference_hashes = {name: digest((args.managed / name).read_bytes()) for name in refs}
commit = args.source_commit or subprocess.check_output(["git", "rev-parse", "HEAD"], cwd=root, text=True).strip()
tree = subprocess.check_output(["git", "rev-parse", "HEAD^{tree}"], cwd=root, text=True).strip()
info = {
    "mod_id": "wrt_type_demand_stars", "release_date": args.date,
    "source_repository": "https://github.com/Tangweirui2021/ProbablyStolen-DemandHighlight",
    "source_commit": commit, "source_tree": tree,
    "game_reference_label": "VERSION 100-Rev13",
    "game_reference_sha256": reference_hashes,
    "mod_dll_sha256": digest(files["TypeDemandStars.dll"]),
    "api_targets_checked": int(re.search(r"API targets verified: (\d+)", log)[1]),
    "methods_scanned": int(re.search(r"Methods scanned: (\d+)", log)[1]),
    "scanner_capabilities": [], "scanner_refused": False,
    "gameplay_tested": False,
}
files["BUILD-INFO.json"] = (json.dumps(info, indent=2) + "\n").encode()
files["SHA256SUMS"] = "".join(digest(data) + "  " + name + "\n" for name, data in sorted(files.items())).encode()
args.output.parent.mkdir(parents=True, exist_ok=True)
with ZipFile(args.output, "w", compression=ZIP_DEFLATED, compresslevel=9) as archive:
    for name, data in sorted(files.items()):
        archive.writestr("wrt_type_demand_stars/" + name, data)
with ZipFile(args.output) as archive:
    assert archive.testzip() is None
print(json.dumps({"archive": str(args.output), "files": len(files), "bytes": args.output.stat().st_size,
                  "sha256": digest(args.output.read_bytes()), "dll_sha256": info["mod_dll_sha256"]}))
