# Marker behavior

[简体中文](../zh-CN/index.md) · [Documentation index](../README.md)

Type Demand & Risk Stars has two independent presentation layers.

## Demand outlines

The original customer acceptance checks decide whether to mark an item. The displayed full outline is a solid line, optionally with a faint fill; its color comes only from the item's selected product type. A stolen food item and an ordinary food item with the same selected type get the same frame.

A container that is not accepted itself but holds a match gets corner brackets in the single configured container color. Storage tabs use that color and corner-hint style too. If the container itself is accepted, it gets its own type-colored full frame.

Customer-demand highlighting retains the original rules for locked/behind-lock or immovable items, whole-container acceptance, barter, variety buyers and flexi buyers. A flexi buyer is marked after choosing what it wants. The manual filter has its own lock-independent matching rules.

Inspections do not add legal-status outlines. Risk is represented by the independent star layer.

## Permanent risk stars

| Item's own status | Default star |
| --- | --- |
| Neither | None |
| Stolen only | Yellow |
| Contraband only | Green |
| Both | One red star |

Stars are registered through native `GameItemElement` creation and validation, independent of shop inventory roots. This includes native item views for customer goods, counters, showcases, containers, machines and other scenes. No customer is required. Hiding demand/filter outlines does not hide or stop updating stars. Native drag/stack previews are tracked separately: both portions get stars when splitting a stack, and a fully picked-up original leaves no star behind.

A container's star describes its own status, not an aggregate of its contents. The star is attached to the outer item-view rectangle rather than its internal rotator, uses an integer-scaled pixel mask, and ignores pointer input/layout. Actual game rotation, mirroring and clipping remain unverified.

## Configuration and compatibility

All marker colors, type priority, rounding, thickness, fill, star pixel scale and corner inset are configurable. See [Configuration](configuration.md).

Disable the original Demand Highlight and overlapping marker mods. The fork uses a separate mod ID but shares the legacy query API contract, so enabling both providers is unsupported. See [Building](building.md) for the real-assembly build/scan checks and remaining gameplay validation.
