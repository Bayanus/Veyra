# Changelog

All notable changes to the Veyra package are documented here.

## [0.2.0] - 2026-09-02

### Added

- Code-authored `VeyraEffectDefinition` and `VeyraProgram` workflow.
- API → IR compilation for emitters, fields, render nodes and beams.
- Procedural branching beam generation with deterministic seeds.
- Generic beam visibility envelopes with attack, hold, decay and off phases.
- Per-cycle beam seed variation for repeated procedural strikes.
- `Play`, `Stop` and `Restart` lifecycle controls on `VeyraEffectPlayer`.
- Horizontal magical lightning vertical-slice example.

### Improved

- Beam rendering now uses local-space `LineRenderer` geometry consistently.
- Runtime reports a clear error when the Veyra additive shader cannot be found.
- Package metadata targets Unity 6 (`6000.0`) and package version `0.2.0`.

### Compatibility

- The original GPU particle prototype remains in the package as a legacy prototype path.
- The programmable beam runtime is the supported vertical slice for this release candidate.

### Limitations

- The current beam renderer is CPU-driven and uses Unity `LineRenderer`.
- Emitter, field and render IR nodes are compiled but are not yet executed by `VeyraEffectPlayer`.
- The legacy GPU particle prototype is not yet driven by the programmable IR.
- Automated Unity editor/runtime validation is not included in this repository state.
