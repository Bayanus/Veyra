# Veyra

**Programmable runtime for procedural visual effects in Unity.**

Veyra is a code-first VFX runtime. An effect is described as a program through the Veyra API, compiled into an intermediate representation (IR), and executed by a runtime backend.

AI is **not** a special subsystem of Veyra. It is simply another client capable of producing Veyra programs.

## Architecture

```text
Human / AI / Editor
        ↓
    Veyra API
        ↓
    Effect IR
        ↓
   Veyra Compiler
        ↓
   Veyra Runtime
     ↙        ↘
 Simulation   Rendering
    GPU          GPU
```

The public API is intended to remain renderer-agnostic. GPU implementation details belong to execution backends, not effect authors.

## Minimal API

```csharp
var effect = VeyraProgram.Create("Fire");

effect.Emitter("fire")
    .Burst(1000)
    .At(Vector3.zero)
    .Velocity(Vector3.up * 3f)
    .Lifetime(1.5f)
    .LifetimeRandom(0.25f)
    .Size(0.2f);

effect.Field(VeyraFieldType.Gravity, 2f);
effect.Field(VeyraFieldType.Turbulence, 1.5f);
effect.Render(VeyraRenderType.Billboard);

var program = effect.Compile();
```

## Procedural beams

Beams are part of the same programmable representation:

```csharp
var strike = effect.Beam("strike")
    .From(Vector3.zero)
    .To(Vector3.right * 8f)
    .Segments(32)
    .Jagged(1.1f)
    .Width(0.08f)
    .Branches(6)
    .Envelope(
        attack: 0f,
        hold: 0.06f,
        decay: 0.5f,
        off: 0.5f);
```

`Envelope` is a generic temporal visibility primitive: attack → hold → decay → off. It contains no knowledge of lightning or any other specific effect.

Repeated envelope cycles receive deterministic per-cycle seed variation, allowing procedural effects to change shape between strikes without introducing global random state.

## Current release candidate

Version **0.2.0** targets **Unity 6 (`6000.0`)**.

The supported vertical slice demonstrates:

- code-authored effect definitions;
- API → IR compilation;
- deterministic procedural beam geometry;
- branching and temporal variation;
- generic temporal envelopes;
- explicit effect lifecycle (`Play`, `Stop`, `Restart`);
- Unity Package Manager-compatible package structure;
- a complete horizontal magical lightning example authored through the public API.

### Known limitations

The release candidate is intentionally narrower than the long-term architecture:

- beam execution is currently CPU-driven through Unity `LineRenderer`;
- emitter, field and render nodes compile into IR but are not yet executed by `VeyraEffectPlayer`;
- the original GPU particle implementation remains as a legacy prototype and is not yet driven by the programmable IR;
- automated Unity editor/runtime validation is not included yet.

These are explicit next-stage items rather than hidden implementation gaps.

## Package

```text
Packages/com.bayanus.veyra/
├── Runtime/
│   ├── VeyraEffect.cs
│   ├── VeyraEffectDefinition.cs
│   ├── VeyraEffectPlayer.cs
│   ├── VeyraLightning.cs
│   └── VeyraRuntime.cs        # legacy GPU prototype
├── Compute/
│   └── VeyraParticles.compute # legacy GPU prototype
├── Shaders/
│   ├── VeyraParticles.shader
│   └── VeyraUnlitAdditive.shader
├── Examples/
│   └── VeyraLightningEffect.cs
├── package.json
└── CHANGELOG.md
```

## Status

**0.2.0 Release Candidate.**

The API and IR are still experimental, but the current vertical slice is intended to be a coherent, testable foundation for the next runtime milestone rather than a collection of disconnected demos.
