# Veyra

**Programmable runtime for procedural visual effects in Unity.**

Veyra is a code-first VFX runtime. An effect is described as a program through the Veyra API, compiled into an intermediate representation (IR), and executed by runtime backends.

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
    .CapacityCount(4096)
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

`Burst(0)` means use the full emitter capacity as an active stream. A positive burst count limits the active particle set to that many particles.

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

The supported runtime path now demonstrates:

- code-authored effect definitions;
- API → IR compilation for emitters, fields, render nodes and beams;
- GPU particle simulation driven by programmable emitter/field IR;
- Unity 6 indirect primitive rendering for billboard particles;
- deterministic procedural beam geometry;
- branching and temporal variation;
- generic temporal envelopes;
- explicit effect lifecycle (`Play`, `Stop`, `Restart`);
- Unity Package Manager-compatible package structure;
- a complete horizontal magical lightning example authored through the public API.

### Known limitations

The release candidate is intentionally narrower than the long-term architecture:

- beam execution is currently CPU-driven through Unity `LineRenderer`;
- only `Billboard` render nodes execute in the GPU particle backend; `Trail` and `Mesh` remain IR-only;
- particle gradients currently reduce to start/end colors in the GPU backend;
- automated Unity editor/runtime validation is not included yet.

The old direct `VeyraRuntime` GPU prototype is quarantined and marked legacy; it is no longer an alternate execution path.

## Package

```text
Packages/com.bayanus.veyra/
├── Runtime/
│   ├── VeyraEffect.cs
│   ├── VeyraEffectDefinition.cs
│   ├── VeyraEffectPlayer.cs
│   ├── VeyraParticleBackend.cs
│   ├── VeyraLightning.cs
│   └── VeyraRuntime.cs        # legacy compatibility stub
├── Compute/
│   └── VeyraParticles.compute
├── Shaders/
│   ├── VeyraParticles.shader
│   └── VeyraUnlitAdditive.shader
├── Examples/
│   └── VeyraLightningEffect.cs
├── Tests/
│   └── Runtime/
├── package.json
└── CHANGELOG.md
```

## Status

**0.2.0 Release Candidate.**

The programmable API, IR, CPU beam backend and GPU particle backend form one execution architecture. The remaining release work is validation and hardening in the target Unity 6 editor and on the graphics APIs Veyra intends to support, followed by broader render backend coverage.
