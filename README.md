# Veyra

**Programmable runtime for procedural visual effects in Unity.**

Veyra is a code-first VFX system. An effect is described as a program through the Veyra API and compiled into a renderer/runtime representation. The same API is intended to be usable by a human, generated code, or future editor tooling.

## Core model

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

AI is **not** a special subsystem of Veyra. It is simply another client capable of producing Veyra programs.

## API direction

```csharp
var effect = VeyraProgram.Create();

var particles = effect.Emitter("fire")
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

Beam effects can also express temporal behavior without runtime knowledge of a particular effect type:

```csharp
var strike = effect.Beam("strike")
    .From(Vector3.zero)
    .To(Vector3.right * 8f)
    .Segments(32)
    .Jagged(1.1f)
    .Width(0.08f)
    .Envelope(
        attack: 0f,
        hold: 0.06f,
        decay: 0.5f,
        off: 0.5f);
```

`Envelope` is a generic visibility envelope: attack → hold → decay → off, repeated when the effect player is looping. The runtime does not contain lightning-specific behavior.

The API is deliberately renderer-agnostic. The long-term goal is to describe effects in terms of emitters, particles, fields, transformations, animation and rendering rather than exposing GPU implementation details directly.

## IR direction

The intermediate representation is the contract between the public programmable API and the execution backend:

```text
Veyra API
   ↓
Effect IR
   ├── emitters
   ├── particle state
   ├── fields / forces
   ├── curves / gradients
   ├── transforms
   └── render operations
   ↓
backend/compiler
   ├── GPU simulation
   ├── procedural geometry
   └── GPU rendering
```

This keeps the public API independent from a particular simulation implementation and leaves room for future CPU/GPU backends, editor tooling and serialization.

## Current vertical slice

The repository currently contains two execution paths: the original GPU particle prototype and the newer programmable API/runtime. The programmable beam path is the current vertical slice and demonstrates:

- code-authored effect definitions
- API → IR compilation
- deterministic procedural beam geometry
- branching and flicker
- generic temporal envelopes
- Unity Package Manager-compatible package layout
- an example horizontal magical lightning strike built entirely from the public effect definition API

The original particle runtime remains as a lower-level prototype while the programmable runtime is being expanded toward a unified execution backend.

## Status

Experimental PoC — API, IR and execution architecture are expected to change. The current goal is to turn the programmable API/IR into a coherent runtime rather than reproduce Unity's Particle System as a wrapper.
