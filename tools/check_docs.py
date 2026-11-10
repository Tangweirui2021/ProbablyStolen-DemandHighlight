#!/usr/bin/env python3
"""Check local documentation links, configuration coverage and superseded presentation claims."""
from pathlib import Path
import re
import sys
from urllib.parse import unquote

ROOT = Path(__file__).resolve().parents[1]
errors = []
required = ["index", "configuration", "item-filter", "filter-language", "query-api", "performance", "building"]
for language in ("manual", "zh-CN"):
    for name in required:
        path = ROOT / "doc" / language / (name + ".md")
        if not path.is_file():
            errors.append(f"Missing manual: {path.relative_to(ROOT)}")

documents = [ROOT / name for name in ("README.md", "README.en.md", "VALIDATION.md", "NOTICE")]
documents.extend((ROOT / "doc").rglob("*.md"))
stale = [
    "Type matches are drawn in cyan",
    "During an inspection, only stolen goods and contraband are marked",
    "green = wanted, amber = stolen, red = contraband",
    "KCN-judu/ProbablyStolen-Mods/en/",
]
for document in documents:
    if not document.is_file():
        errors.append(f"Missing document: {document.relative_to(ROOT)}")
        continue
    text = document.read_text(encoding="utf-8")
    for phrase in stale:
        if phrase in text:
            errors.append(f"Superseded claim in {document.relative_to(ROOT)}: {phrase}")
    for target in re.findall(r"!?\[[^\]]*\]\(([^)]+)\)", text):
        target = target.strip().strip("<>")
        if re.match(r"[a-zA-Z][a-zA-Z0-9+.-]*:", target) or target.startswith("#"):
            continue
        relative = unquote(target.split("#", 1)[0])
        if relative and not (document.parent / relative).exists():
            errors.append(f"Broken local link in {document.relative_to(ROOT)}: {target}")

settings_source = (ROOT / "src/MarkerSettings.cs").read_text()
settings_source += (ROOT / "src/DemandMod.cs").read_text()
keys = set(re.findall(r'(?:config\.Define|ColorOption)\("([^"\n]+)"', settings_source))
keys.discard("typeColor_")
keys.add("typeColor_<TYPE_ID>")
for language in ("manual", "zh-CN"):
    text = (ROOT / "doc" / language / "configuration.md").read_text()
    for key in keys:
        if f"`{key}`" not in text:
            errors.append(f"Missing setting in {language}/configuration.md: {key}")

if (ROOT / "doc/locale").exists():
    errors.append("Superseded gettext catalogs are still present")
if errors:
    print("\n".join(errors), file=sys.stderr)
    sys.exit(1)
print(f"Documentation passed: {len(documents)} files, {len(keys)} configuration keys, local links and current behavior.")
