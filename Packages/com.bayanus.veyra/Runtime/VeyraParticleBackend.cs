using System;
using System.Collections.Generic;
using UnityEngine;

namespace Veyra
{
    /// <summary>GPU execution backend for emitter/field/render IR nodes.</summary>
    internal sealed class VeyraParticleBackend : IDisposable
    {
        const int MaxFields = 32;
        readonly MonoBehaviour owner;
        readonly ComputeShader simulation;
        readonly Material sourceMaterial;
        readonly List<EmitterInstance> emitters = new();
        readonly Vector4[] fieldData = new Vector4[MaxFields * 2];
        ComputeBuffer fieldBuffer;
        int fieldCount;
        bool loop;
        bool disposed;

        struct EmitterInstance : IDisposable
        {
            public ComputeBuffer particles;
            public GraphicsBuffer args;
            public int kernel;
            public int count;
            public float lifetime;
            public Vector3 position;
            public Vector3 velocity;
            public float size;
            public Material material;
            public Bounds bounds;

            public void Dispose()
            {
                particles?.Release();
                args?.Dispose();
                particles = null;
                args = null;
                if (material) UnityEngine.Object.Destroy(material);
                material = null;
            }
        }

        public bool IsValid => !disposed && simulation && sourceMaterial && SystemInfo.supportsComputeShaders;

        public VeyraParticleBackend(MonoBehaviour owner, ComputeShader simulation, Material sourceMaterial)
        {
            this.owner = owner;
            this.simulation = simulation;
            this.sourceMaterial = sourceMaterial;
            if (IsValid)
            {
                fieldBuffer = new ComputeBuffer(MaxFields, sizeof(float) * 8, ComputeBufferType.Structured);
                fieldBuffer.SetData(fieldData);
            }
        }

        public void Build(VeyraIR ir, Transform root, bool loop)
        {
            ClearEmitters();
            this.loop = loop;
            if (!IsValid || ir == null || ir.emitters.Count == 0) return;

            BuildFields(ir, root);
            Material renderMaterial = ResolveMaterial(ir);

            for (int i = 0; i < ir.emitters.Count; i++)
            {
                var spec = ir.emitters[i];
                int count = Mathf.Clamp(spec.capacity, 1, 1048576);
                var instance = new EmitterInstance
                {
                    particles = new ComputeBuffer(count, sizeof(float) * 8, ComputeBufferType.Structured),
                    args = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 1, GraphicsBuffer.IndirectDrawArgs.size),
                    kernel = simulation.FindKernel("Update"),
                    count = count,
                    lifetime = Mathf.Max(0.001f, spec.lifetime),
                    position = root.TransformPoint(spec.position),
                    velocity = root.TransformDirection(spec.velocity),
                    size = Mathf.Max(0.001f, spec.size),
                    material = new Material(renderMaterial),
                    bounds = CalculateBounds(spec, root)
                };

                var args = new GraphicsBuffer.IndirectDrawArgs[1];
                args[0].vertexCountPerInstance = (uint)count;
                args[0].instanceCount = 1;
                args[0].startVertex = 0;
                args[0].startInstance = 0;
                instance.args.SetData(args);
                Configure(instance, spec, i);
                emitters.Add(instance);
            }
        }

        void Configure(EmitterInstance instance, VeyraIREmitter spec, int emitterIndex)
        {
            simulation.SetBuffer(instance.kernel, "Particles", instance.particles);
            simulation.SetBuffer(instance.kernel, "Fields", fieldBuffer);
            simulation.SetInt("ParticleCount", instance.count);
            simulation.SetInt("FieldCount", fieldCount);
            simulation.SetInt("BurstCount", Mathf.Clamp(spec.burstCount, 0, instance.count));
            simulation.SetInt("LoopEmitter", loop ? 1 : 0);
            simulation.SetFloat("Lifetime", instance.lifetime);
            simulation.SetVector("InitialVelocity", instance.velocity);
            simulation.SetVector("EmitterPosition", instance.position);
            simulation.SetInt("EmitterSeed", emitterIndex * 747796405 + 289133645);

            instance.material.SetBuffer("Particles", instance.particles);
            instance.material.SetFloat("_ParticleSize", instance.size);
            instance.material.SetFloat("_Lifetime", instance.lifetime);
            ApplyGradient(instance.material, spec.color);
        }

        void BuildFields(VeyraIR ir, Transform root)
        {
            fieldCount = Mathf.Min(ir.fields.Count, MaxFields);
            Array.Clear(fieldData, 0, fieldData.Length);
            for (int i = 0; i < fieldCount; i++)
            {
                var field = ir.fields[i];
                Vector3 position = root.TransformPoint(field.position);
                fieldData[i * 2] = new Vector4(position.x, position.y, position.z, field.radius);
                fieldData[i * 2 + 1] = new Vector4((float)field.type, field.strength, 0f, 0f);
            }
            fieldBuffer.SetData(fieldData);
        }

        Material ResolveMaterial(VeyraIR ir)
        {
            for (int i = 0; i < ir.renders.Count; i++)
                if (ir.renders[i].type == VeyraRenderType.Billboard && ir.renders[i].material)
                    return ir.renders[i].material;
            return sourceMaterial;
        }

        static void ApplyGradient(Material material, Gradient gradient)
        {
            if (gradient == null) return;
            material.SetColor("_StartColor", gradient.Evaluate(0f));
            material.SetColor("_EndColor", gradient.Evaluate(1f));
        }

        static Bounds CalculateBounds(VeyraIREmitter spec, Transform root)
        {
            Vector3 center = root.TransformPoint(spec.position);
            float lifetime = Mathf.Max(0.001f, spec.lifetime);
            float distance = spec.velocity.magnitude * lifetime + Mathf.Max(1f, spec.size * 4f);
            return new Bounds(center, Vector3.one * (distance * 2f + 4f));
        }

        public void Update(float deltaTime, float time)
        {
            if (!IsValid) return;
            for (int i = 0; i < emitters.Count; i++)
            {
                var instance = emitters[i];
                simulation.SetFloat("DeltaTime", Mathf.Min(deltaTime, 0.05f));
                simulation.SetFloat("TimeValue", time);
                int groups = Mathf.CeilToInt(instance.count / 256f);
                simulation.Dispatch(instance.kernel, groups, 1, 1);

                var renderParams = new RenderParams(instance.material)
                {
                    worldBounds = instance.bounds,
                    layer = owner.gameObject.layer
                };
                Graphics.RenderPrimitivesIndirect(ref renderParams, MeshTopology.Points, instance.args);
            }
        }

        void ClearEmitters()
        {
            for (int i = 0; i < emitters.Count; i++)
            {
                var instance = emitters[i];
                instance.Dispose();
            }
            emitters.Clear();
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            ClearEmitters();
            fieldBuffer?.Release();
            fieldBuffer = null;
        }
    }
}
