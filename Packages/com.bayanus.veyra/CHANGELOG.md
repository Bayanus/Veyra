# Changelog

All notable changes to the Veyra package are documented here.

## [0.2.0] - 2026-09-02

### Added

- Code-authored `VeyraEffectDefinition` and `VeyraProgram` workflow.
- API → IR compilation for emitters, fields, render nodes and beams.
- Programmable GPU particle execution for emitter/field/billboard IR nodes.
- Bounded GPU field evaluation for gravity, radial, vortex and turbulence fields.
- Emitter capacity and burst semantics in the programmable IR.
- Unity 6 indirect primitive rendering path for GPU particles.
- Procedural branching beam generation with deterministic seeds.
- Generic beam visibility envelopes with attack, hold, decay and off phases.
- Per-cycle beam seed variation for repeated procedural strikes.
- `Play`, `Stop` and `Restart` lifecycle controls on `VeyraEffectPlayer`.
- Horizontal magical lightning vertical-slice example.

### Improved

- Beam rendering now uses local-space `LineRenderer` geometry consistently.
- Particle shader lifetime and material property bindings are explicit and correct.
- Runtime reports a clear error when the Veyra additive shader cannot be found.
- Package metadata targets Unity 6 (`6000.0`) and package version `0.2.0`.

### Compatibility

- `VeyraRuntime` and the old `VeyraEffect` particle asset remain in the package for source compatibility with the prototype, but are legacy and are not the programmable execution path.
- `VeyraEffectPlayer` is the supported entry point for programmable effects.

### Limitations

- The current beam renderer is CPU-driven and uses Unity `LineRenderer`.
- Only Billboard render nodes are currently executed by the GPU particle backend; Trail and Mesh render nodes remain IR-only.
- Particle color gradients currently reduce to start/end colors in the GPU backend.
- Automated Unity editor/runtime validation is not included in this repository state; the package must still be validated in the target Unity 6 editor and on supported graphics APIs.
