# Changelog

## [2.0.0-preview.3] - 2026-10-12

### Added
- Lists: hover support. `PointerElement` now reports pointer enter/exit; `ListItemView` has an optional `PointerElement` field; `ListItemMediator` exposes `SetHovered(bool)` and `ListItemHoverSignal`; `ListBaseMediator` gets `ListItemHoverEnter` / `ListItemHoverExit` hooks.
- Lists: `SelectionTrigger` enum (`Click` default, `Hover`, `None`) on `ListView` (serialized) and the list mediator, deciding which input changes selection. `Hover` and `None` leave selection to hover or code and let clicks act as confirm.
- Lists: public `ItemClickedSignal` on the list mediator, dispatched on every item click regardless of selection.
- `ExtendedMediator.AddHoverListener` with auto-cleanup.
- Editor: `FramewerkScaffold.Execute(json)` static entry point for headless scaffolding through the Unity CLI (`unity command eval`).

### Changed
- Lists: `SelectionChangedSignal` is dispatched once per click instead of twice.
- Wizard list item template prefab ships with a `PointerElement` wired to the view; generated item mediators get a `SetHovered` stub.
- Editor package no longer depends on `com.gamelovers.mcp-unity`; the project uses Unity's CLI and `com.unity.pipeline` instead.
- Project upgraded to Unity 6000.6; `UIPrefabParentingFixer` uses `GetEntityId()`.

## [2.0.0] - 2025-01-29

### Changed
- Restructured as Unity Package Manager (UPM) package
- Embedded StrangeIoC dependency within the package
- Added assembly definition for proper compilation isolation
