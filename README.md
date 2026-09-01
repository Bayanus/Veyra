# Veyra

**Programmable runtime for procedural visual effects in Unity.**

Veyra is a code-first VFX system: an effect is authored as a program, compiled to backend-neutral IR, and executed by a runtime. Humans, generated code and future editor tools use the same API.

AI is **not** a subsystem. It is simply another authoring client.

## The target workflow

The important test for Veyra is not “can it make particles?” but:

> “Make me an awesome juicy lightning effect.”

A generated Veyra definition should be able to describe that effect in ordinary C# and the Unity user should only need to put the resulting `VeyraEffectPlayer` in a scene and assign the definition.

```text
Human request
     ↓
Human / AI writes Veyra definition
     ↓
Veyra API
     ↓
Veyra IR
     ↓
Veyra Runtime
     ↓
Unity scene
```

## Vertical slice — 0.2

The current vertical slice proves the complete authoring-to-rendering path for procedural beams/lightning:

- code-authored `VeyraEffectDefinition`
- `VeyraProgram` → `VeyraIR` compilation
- procedural jagged beam generation
- deterministic branching
- animated flicker/noise
- layered core + hot core + halo rendering
- additive beam shader
- reusable `VeyraEffectPlayer` component
- example “Succulent Lightning” effect

Example authoring API:

```csharp
public override VeyraProgram Build()
{
    var fx = VeyraProgram.Create("Lightning");

    fx.Beam("Main")
        .From(Vector3.zero)
        .To(Vector3.forward * 8f)
        .Segments(30)
        .Jagged(1.2f)
        .Width(0.08f)
        .Branches(8)
        .BranchLength(0.4f)
        .Flicker(0.35f)
        .Speed(22f)
        .Color(new Color(0.6f, 0.85f, 1f));

    return fx;
}
```

The example deliberately builds a lightning effect from several beams rather than hard-coding a “lightning renderer”. This is the intended direction: reusable primitives first, effect presets second.

## Architecture

```text
                    Veyra API
                       │
                       ▼
                  Veyra Effect
                       │
                       ▼
                    Veyra IR
                       │
                ┌──────┴──────┐
                ▼             ▼
          GPU particle    Procedural geometry
           backend             backend
                │             │
                └──────┬──────┘
                       ▼
                  Veyra Runtime
                       │
                       ▼
                     Unity
```

The IR is the contract between authoring and execution. It must not depend on editor state or on a particular rendering implementation.

## Current API primitives

### Simulation

- emitters
- burst spawning
- initial velocity
- lifetime and randomness
- size and randomness
- gradients
- gravity
- radial fields
- vortex fields
- turbulence

### Geometry

- procedural beams
- configurable segment count
- jagged displacement
- deterministic seeds
- branching
- branch length
- animated flicker

### Rendering

- billboard
- trail
- mesh (API primitive; backend support is still being expanded)
- additive beam rendering

## Example: use an effect in Unity

1. Create a class deriving from `VeyraEffectDefinition`.
2. Implement `Build()` with the Veyra API.
3. Create the definition asset from Unity's asset menu.
4. Add `VeyraEffectPlayer` to a GameObject.
5. Assign the definition.

Generated effect code can therefore live directly in a project/package and be reused like any other Unity asset.

## Roadmap after the vertical slice

The vertical slice is intentionally small. The next expansion should make the same API capable of expressing:

1. particle simulation driven by the IR instead of the legacy `VeyraEffect` asset;
2. curves and gradients as first-class IR values;
3. transforms, attractors and collision queries;
4. mesh/ribbon/trail procedural geometry;
5. spawn/update/render stages and composable operations;
6. GPU compilation/execution of the same IR;
7. serialization so generated effects can be stored and versioned independently of C#;
8. a small library of primitives that makes generated effect authoring fast and predictable.

The end goal is not a clone of Unity Particle System. It is a programmable VFX language/runtime where an effect can be generated, reviewed, versioned and executed without hand-building a large graph in the editor.

## Status

**0.2 vertical slice — experimental.** The lightning path is functional; the broader programmable runtime is still under active development.
