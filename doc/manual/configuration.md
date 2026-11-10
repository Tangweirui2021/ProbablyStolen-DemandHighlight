# Configuration

[简体中文](../zh-CN/configuration.md) · [Documentation index](../README.md)

Enable the mod and open its gear button in **main menu → Mods**. The game generates the panel and persists the values. Color fields accept text, not a graphical color picker. Apply invokes the setting callbacks. An entry in the shop's pause menu has not been confirmed.

## Marker settings

| Key | Default | Range or meaning |
| --- | --- | --- |
| `lineThickness` | `2` | Outline width, 1–6 UI pixels |
| `cornerRadius` | `2` | Outer corner radius, 0–8; 0 is square |
| `fillTint` | `true` | Faint fill inside direct-match frames |
| `defaultOutlineColor` | `#5CEB6B` | Items without an eligible product type |
| `containerOutlineColor` | `#80D8FF` | One color for contents-only brackets and storage tabs |
| `typeColor_<TYPE_ID>` | Per type | Native product types are registered; new matched product types are added when encountered |
| `typePriority` | See `MarkerSettings.DefaultPriority` | Comma-separated type IDs, highest priority first |
| `riskStars` | `true` | Permanent star-layer switch, independent of the demand hotkey |
| `stolenStarColor` | `#FFD84A` | Stolen only |
| `contrabandStarColor` | `#66ED80` | Contraband only |
| `bothStarColor` | `#FF5656` | Both statuses on one item |
| `starOutlineColor` | `#211C30` | Pixel-star edge |
| `starPixelScale` | `2` | Size levels 1–4: approximately 1/6, 1/3, 1/2 and 2/3 of one cell, rounded to UI pixels |
| `starInset` | `2` | Inset from the rotated/mirrored footprint's top-right corner, 0–16 UI pixels |
| `filterNumbers` | `true` | Item Filter statistics |

Both **Toggle demand highlight** and **Open item filter** are unbound by default and belong in the game's key bindings. They are separate from the persisted marker-color entries. The old conditional `highlightClientGoods` option is replaced by the global permanent star layer.

Star dimensions follow the game's single-cell size, independently of item/container dimensions. At the default 16 UI pixels per cell, level 2 is 5×5 UI pixels including the edge. Stars stay at the top right. The legacy `starPixelScale` key and saved values are retained; updating automatically makes existing configurations compact.

The color-priority input has the same fixed width as other text settings. Long values scroll inside the field; format instructions are in its tooltip.

## Type colors

A multi-type item uses the first matching ID in `typePriority`, case-insensitively. Unlisted types fall back to ordinal case-insensitive ID ordering. There is one selected type per frame; the current customer and legal status do not choose the color.

`CONTRABAND`, `STOLEN` and `RESTRICTED_*` are excluded from frame-color selection. These classifications remain available for matching/filtering. No eligible product type uses `defaultOutlineColor`.

Default palettes group related products: food is peach, tools pale yellow, water/beverages blue, medical goods mint, weapons salmon, materials beige and machines violet. Each registered type remains separately editable. Different types may initially share a color.

## Hex values and persistence

Use `#RRGGBB` or `#RRGGBBAA` (a leading `#` is optional). Alpha affects the relevant frame/fill or star layer. Star fill and edge colors are separate settings.

An invalid color logs a warning and uses the last valid color in the current process. An invalid startup value uses that entry's default. The invalid text is not silently rewritten in the JSON; correct it and Apply again.

Settings are saved by the game at:

`<Application.persistentDataPath>/ModConfigs/wrt_type_demand_stars.json`

Exit the game before manual editing and keep its generated key/value-list structure. Missing entries use defaults. The file is outside the mod's install folder, so replacing the mod does not replace saved settings. No external YAML parser or file capability is required.
