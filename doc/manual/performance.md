# Performance and lifecycle

[简体中文](../zh-CN/performance.md) · [Documentation index](../README.md)

These are code-level scheduling targets, not measured frame-time guarantees on the current game build.

## Demand/filter outlines

The inherited engine records item facts, customer acceptance and container summaries. Validation, tag/type changes, moves and container visibility events invalidate relevant records. Current demand is compared by value every 0.5 seconds, and work is batched with a 0.15-second minimum gap.

The engine walks shop inventories and open containers, using cached summaries for closed containers. It targets a 2.5 ms walk budget and continues incomplete work on later frames. Individual game calls and drawing can exceed that target.

The presentation layer normalizes matches to a solid full frame or contents-only brackets. Colors are selected by product type and independently cached with frame state. Rectangles share nine-slice textures; other shapes use cached masks. Thickness, rounding and fill still affect geometry.

## Global permanent stars

`RiskStars` is a separate registry of native item views, registered by constructor, validation, tag/type change, movement and rotation hooks. It never requires `Shop`, `Engine`, a customer or shop inventory membership. Its tick executes before the demand-toggle and no-query early returns.

The registry uses weak item/UI references, an urgent event queue and a round-robin safety refresh. Each tick targets approximately 1.5 ms, with bounded queue/background work; a normal view is eligible for another status check after 0.25 seconds. Large queues can delay work. Two shared point-filtered masks draw the star body and edge; colors and size do not allocate a separate sprite for every item.

The game helpers are read for stolen and contraband independently, preserving the combined red-star state. A container does not aggregate its contents' risk.

## Cleanup

Leaving the store releases demand-engine state and its outlines/filter window. Single-scene transitions release star UI but retain weak registrations so views already created in the new scene can redraw. Additive scenes do not clear existing star UI. Orphaned badges are removed when a view model is collected, including when a pooled anchor remains alive. Disabling the mod removes patches, sprites, badges and registry state.

## Diagnostics and checks

The fork's log prefix is `[TypeDemandStars]`. Demand refreshes above 8 ms are logged with walk/draw timings; this is a diagnostic threshold rather than a budget guarantee. Invalid color input produces a warning and keeps a valid fallback.

Run `tests/run.sh` for the pure-data and stand-in integration checks. [VALIDATION.md](../../VALIDATION.md) separates passing checks from real-game tests that are still pending. See [Configuration](configuration.md) for the marker settings and [Query API](query-api.md) for whole-shop API walks, which have a different scope and scheduling model.
