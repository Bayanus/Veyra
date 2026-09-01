# Veyra

**Code-first, GPU-driven VFX runtime for Unity.**

Veyra is a PoC for a programmable visual-effects system where an effect is represented as data/program instructions instead of a hand-authored Particle System graph.

## PoC

The current PoC demonstrates the core execution path:

```text
VeyraEffect (data)
      ↓
C# VeyraRuntime
      ↓
Compute Shader
      ↓
GraphicsBuffer
      ↓
Procedural Render Shader
      ↓
GPU
```

It contains:

- GPU particle simulation through a Compute Shader
- `Graphics.DrawProceduralIndirect` rendering
- a small serializable `VeyraEffect` definition
- configurable lifetime, velocity, radial force, turbulence, size and color
- Unity Package Manager-compatible package layout

## Direction

The PoC deliberately does **not** include an LLM yet. The intended architecture is:

```text
Natural language / VFX DSL
          ↓
       AI compiler
          ↓
      Veyra program
          ↓
     Veyra Runtime
          ↓
 Compute + Render Shaders
```

The AI should generate a constrained Veyra representation rather than arbitrary HLSL. This keeps generation deterministic, validateable and safe to execute.

## Status

Experimental PoC — API and architecture are expected to change.
