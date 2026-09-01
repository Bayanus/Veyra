using System;
using System.Collections.Generic;
using UnityEngine;

namespace Veyra
{
    /// <summary>
    /// Authoring API for a Veyra effect. This is the common programmable surface
    /// intended for humans, generated code and future editor tooling.
    /// </summary>
    public sealed class VeyraProgram
    {
        internal readonly List<VeyraEmitterNode> Emitters = new();
        internal readonly List<VeyraFieldNode> Fields = new();
        internal readonly List<VeyraRenderNode> Renders = new();

        public static VeyraProgram Create() => new();

        public VeyraEmitterNode Emitter(string name = "Emitter")
        {
            var node = new VeyraEmitterNode(name);
            Emitters.Add(node);
            return node;
        }

        public VeyraFieldNode Field(VeyraFieldType type, float strength)
        {
            var node = new VeyraFieldNode(type, strength);
            Fields.Add(node);
            return node;
        }

        public VeyraRenderNode Render(VeyraRenderType type)
        {
            var node = new VeyraRenderNode(type);
            Renders.Add(node);
            return node;
        }

        public VeyraIR Compile() => VeyraCompiler.Compile(this);
    }

    public sealed class VeyraEmitterNode
    {
        internal string Name;
        internal int BurstCount;
        internal Vector3 Position;
        internal Vector3 Velocity;
        internal float Lifetime = 1f;
        internal float LifetimeRandomness;
        internal float Size = 1f;
        internal float SizeRandomness;
        internal Gradient Gradient;

        internal VeyraEmitterNode(string name)
        {
            Name = string.IsNullOrWhiteSpace(name) ? "Emitter" : name;
            Gradient = new Gradient();
            Gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
        }

        public VeyraEmitterNode Burst(int count)
        {
            BurstCount = Mathf.Max(0, count);
            return this;
        }

        public VeyraEmitterNode At(Vector3 position)
        {
            Position = position;
            return this;
        }

        public VeyraEmitterNode Velocity(Vector3 velocity)
        {
            Velocity = velocity;
            return this;
        }

        public VeyraEmitterNode Lifetime(float seconds)
        {
            Lifetime = Mathf.Max(0.001f, seconds);
            return this;
        }

        public VeyraEmitterNode LifetimeRandom(float amount)
        {
            LifetimeRandomness = Mathf.Clamp01(amount);
            return this;
        }

        public VeyraEmitterNode Size(float size)
        {
            Size = Mathf.Max(0.001f, size);
            return this;
        }

        public VeyraEmitterNode SizeRandom(float amount)
        {
            SizeRandomness = Mathf.Clamp01(amount);
            return this;
        }

        public VeyraEmitterNode Color(Gradient gradient)
        {
            Gradient = gradient ?? Gradient;
            return this;
        }
    }

    public enum VeyraFieldType
    {
        Gravity,
        Radial,
        Vortex,
        Turbulence
    }

    public sealed class VeyraFieldNode
    {
        internal readonly VeyraFieldType Type;
        internal readonly float Strength;
        internal Vector3 Position;
        internal float Radius = 1f;

        internal VeyraFieldNode(VeyraFieldType type, float strength)
        {
            Type = type;
            Strength = strength;
        }

        public VeyraFieldNode At(Vector3 position)
        {
            Position = position;
            return this;
        }

        public VeyraFieldNode Within(float radius)
        {
            Radius = Mathf.Max(0.001f, radius);
            return this;
        }
    }

    public enum VeyraRenderType
    {
        Billboard,
        Trail,
        Mesh
    }

    public sealed class VeyraRenderNode
    {
        internal readonly VeyraRenderType Type;
        internal Material Material;

        internal VeyraRenderNode(VeyraRenderType type) => Type = type;

        public VeyraRenderNode MaterialOverride(Material material)
        {
            Material = material;
            return this;
        }
    }

    /// <summary>
    /// Backend-neutral intermediate representation of an effect program.
    /// It contains values, not Unity editor state or rendering implementation details.
    /// </summary>
    [Serializable]
    public sealed class VeyraIR
    {
        public int version = 1;
        public List<VeyraIREmitter> emitters = new();
        public List<VeyraIRField> fields = new();
        public List<VeyraIRRender> renders = new();
    }

    [Serializable]
    public sealed class VeyraIREmitter
    {
        public string name;
        public int burstCount;
        public Vector3 position;
        public Vector3 velocity;
        public float lifetime;
        public float lifetimeRandomness;
        public float size;
        public float sizeRandomness;
        public Gradient color;
    }

    [Serializable]
    public sealed class VeyraIRField
    {
        public VeyraFieldType type;
        public float strength;
        public Vector3 position;
        public float radius;
    }

    [Serializable]
    public sealed class VeyraIRRender
    {
        public VeyraRenderType type;
        public Material material;
    }

    internal static class VeyraCompiler
    {
        public static VeyraIR Compile(VeyraProgram program)
        {
            if (program == null) throw new ArgumentNullException(nameof(program));

            var ir = new VeyraIR();

            foreach (var source in program.Emitters)
            {
                ir.emitters.Add(new VeyraIREmitter
                {
                    name = source.Name,
                    burstCount = source.BurstCount,
                    position = source.Position,
                    velocity = source.Velocity,
                    lifetime = source.Lifetime,
                    lifetimeRandomness = source.LifetimeRandomness,
                    size = source.Size,
                    sizeRandomness = source.SizeRandomness,
                    color = source.Gradient
                });
            }

            foreach (var source in program.Fields)
            {
                ir.fields.Add(new VeyraIRField
                {
                    type = source.Type,
                    strength = source.Strength,
                    position = source.Position,
                    radius = source.Radius
                });
            }

            foreach (var source in program.Renders)
            {
                ir.renders.Add(new VeyraIRRender
                {
                    type = source.Type,
                    material = source.Material
                });
            }

            return ir;
        }
    }

    /// <summary>
    /// Legacy/simple asset used by the first GPU execution PoC.
    /// It remains as the bridge until the runtime consumes VeyraIR directly.
    /// </summary>
    [CreateAssetMenu(menuName = "Veyra/Effect", fileName = "VeyraEffect")]
    public sealed class VeyraEffect : ScriptableObject
    {
        [Min(1)] public int particleCount = 4096;
        [Min(0.01f)] public float lifetime = 2f;
        public Vector3 initialVelocity = new Vector3(0, 3, 0);
        [Min(0)] public float radialForce = 4f;
        [Min(0)] public float turbulence = 1.5f;
        [Min(0.001f)] public float particleSize = 0.06f;
        public Color startColor = Color.white;
        public Color endColor = new Color(1f, 0.2f, 0.02f, 0f);
    }
}
