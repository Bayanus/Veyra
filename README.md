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

## Current PoC

The repository currently contains the first GPU execution prototype as well as the beginning of the programmable API. The original particle runtime demonstrates:

- GPU particle simulation through a Compute Shader
- `Graphics.DrawProceduralIndirect` rendering
- Unity Package Manager-compatible package layout
- configurable particle lifetime, velocity, force, turbulence, size and color

The programmable API/IR is intentionally being designed before expanding the feature set. The goal is to avoid rebuilding Unity's Particle System as a thin wrapper and instead establish a general representation for procedural VFX.

## Status

Experimental PoC — API, IR and execution architecture are expected to change.
