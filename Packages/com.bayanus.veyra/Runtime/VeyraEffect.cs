using System;
using System.Collections.Generic;
using UnityEngine;

namespace Veyra
{
    public sealed class VeyraProgram
    {
        internal readonly List<VeyraEmitterNode> Emitters = new();
        internal readonly List<VeyraFieldNode> Fields = new();
        internal readonly List<VeyraRenderNode> Renders = new();
        internal readonly List<VeyraBeamNode> Beams = new();
        internal string Name { get; }
        public VeyraProgram(string name = "Effect") => Name = string.IsNullOrWhiteSpace(name) ? "Effect" : name;
        public static VeyraProgram Create(string name = "Effect") => new(name);
        public VeyraEmitterNode Emitter(string name = "Emitter") { var n = new VeyraEmitterNode(name); Emitters.Add(n); return n; }
        public VeyraFieldNode Field(VeyraFieldType type, float strength) { var n = new VeyraFieldNode(type, strength); Fields.Add(n); return n; }
        public VeyraRenderNode Render(VeyraRenderType type) { var n = new VeyraRenderNode(type); Renders.Add(n); return n; }
        public VeyraBeamNode Beam(string name = "Beam") { var n = new VeyraBeamNode(name, (uint)(Beams.Count + 1) * 2654435761u); Beams.Add(n); return n; }
        public VeyraIR Compile() => VeyraCompiler.Compile(this);
    }

    public sealed class VeyraEmitterNode
    {
        internal string Name; internal int BurstCount; internal int Capacity = 1024; internal Vector3 Position; internal Vector3 Velocity;
        internal float Lifetime = 1f; internal float LifetimeRandomness; internal float Size = 1f; internal float SizeRandomness; internal Gradient Gradient;
        internal VeyraEmitterNode(string name) { Name = string.IsNullOrWhiteSpace(name) ? "Emitter" : name; Gradient = DefaultGradient(); }
        public VeyraEmitterNode Burst(int count) { BurstCount = Mathf.Max(0, count); return this; }
        public VeyraEmitterNode CapacityCount(int count) { Capacity = Mathf.Clamp(count, 1, 1048576); return this; }
        public VeyraEmitterNode At(Vector3 position) { Position = position; return this; }
        public VeyraEmitterNode Velocity(Vector3 velocity) { Velocity = velocity; return this; }
        public VeyraEmitterNode Lifetime(float seconds) { Lifetime = Mathf.Max(0.001f, seconds); return this; }
        public VeyraEmitterNode LifetimeRandom(float amount) { LifetimeRandomness = Mathf.Clamp01(amount); return this; }
        public VeyraEmitterNode Size(float size) { Size = Mathf.Max(0.001f, size); return this; }
        public VeyraEmitterNode SizeRandom(float amount) { SizeRandomness = Mathf.Clamp01(amount); return this; }
        public VeyraEmitterNode Color(Gradient gradient) { Gradient = gradient ?? Gradient; return this; }
        static Gradient DefaultGradient() { var g = new Gradient(); g.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) }, new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(0, 1) }); return g; }
    }

    public enum VeyraFieldType { Gravity, Radial, Vortex, Turbulence }
    public sealed class VeyraFieldNode
    {
        internal readonly VeyraFieldType Type; internal readonly float Strength; internal Vector3 Position; internal float Radius = 1f;
        internal VeyraFieldNode(VeyraFieldType type, float strength) { Type = type; Strength = strength; }
        public VeyraFieldNode At(Vector3 position) { Position = position; return this; }
        public VeyraFieldNode Within(float radius) { Radius = Mathf.Max(0.001f, radius); return this; }
    }

    public enum VeyraRenderType { Billboard, Trail, Mesh }
    public sealed class VeyraRenderNode
    {
        internal readonly VeyraRenderType Type; internal Material Material;
        internal VeyraRenderNode(VeyraRenderType type) => Type = type;
        public VeyraRenderNode MaterialOverride(Material material) { Material = material; return this; }
    }

    [Serializable]
    public sealed class VeyraIR
    {
        public int version = 4;
        public string name;
        public List<VeyraIREmitter> emitters = new();
        public List<VeyraIRField> fields = new();
        public List<VeyraIRRender> renders = new();
        public List<VeyraIRBeam> beams = new();
    }

    [Serializable] public sealed class VeyraIREmitter { public string name; public int burstCount; public int capacity; public Vector3 position; public Vector3 velocity; public float lifetime; public float lifetimeRandomness; public float size; public float sizeRandomness; public Gradient color; }
    [Serializable] public sealed class VeyraIRField { public VeyraFieldType type; public float strength; public Vector3 position; public float radius; }
    [Serializable] public sealed class VeyraIRRender { public VeyraRenderType type; public Material material; }

    internal static class VeyraCompiler
    {
        public static VeyraIR Compile(VeyraProgram program)
        {
            if (program == null) throw new ArgumentNullException(nameof(program));
            var ir = new VeyraIR { name = program.Name };
            foreach (var s in program.Emitters) ir.emitters.Add(new VeyraIREmitter { name = s.Name, burstCount = s.BurstCount, capacity = s.Capacity, position = s.Position, velocity = s.Velocity, lifetime = s.Lifetime, lifetimeRandomness = s.LifetimeRandomness, size = s.Size, sizeRandomness = s.SizeRandomness, color = s.Gradient });
            foreach (var s in program.Fields) ir.fields.Add(new VeyraIRField { type = s.Type, strength = s.Strength, position = s.Position, radius = s.Radius });
            foreach (var s in program.Renders) ir.renders.Add(new VeyraIRRender { type = s.Type, material = s.Material });
            foreach (var s in program.Beams) ir.beams.Add(new VeyraIRBeam { name = s.Name, start = s.Start, end = s.End, segments = s.Segments, jaggedness = s.Jaggedness, width = s.Width, branchCount = s.BranchCount, branchLength = s.BranchLength, flicker = s.Flicker, speed = s.Speed, color = s.ColorValue, seed = s.Seed, attack = s.Attack, hold = s.Hold, decay = s.Decay, off = s.Off });
            return ir;
        }
    }

    /// <summary>Legacy prototype asset retained for source compatibility. Use VeyraEffectDefinition instead.</summary>
    [Obsolete("VeyraEffect is a legacy prototype asset. Use VeyraEffectDefinition and VeyraProgram.", false)]
    [CreateAssetMenu(menuName = "Veyra/Legacy Effect", fileName = "VeyraEffect")]
    public sealed class VeyraEffect : ScriptableObject
    {
        [Min(1)] public int particleCount = 4096; [Min(0.01f)] public float lifetime = 2f; public Vector3 initialVelocity = new(0, 3, 0);
        [Min(0)] public float radialForce = 4f; [Min(0)] public float turbulence = 1.5f; [Min(0.001f)] public float particleSize = 0.06f;
        public Color startColor = Color.white; public Color endColor = new(1f, 0.2f, 0.02f, 0f);
    }
}
