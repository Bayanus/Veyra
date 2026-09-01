using UnityEngine;

namespace Veyra
{
    public sealed class VeyraRuntime : MonoBehaviour
    {
        public VeyraEffect effect;
        public ComputeShader simulation;
        public Material particleMaterial;

        ComputeBuffer particles;
        ComputeBuffer args;
        int updateKernel;
        int count;
        readonly uint[] drawArgs = new uint[5];

        void OnEnable()
        {
            if (!effect || !simulation || !particleMaterial) return;

            count = Mathf.Max(1, effect.particleCount);
            particles = new ComputeBuffer(count, sizeof(float) * 8, ComputeBufferType.Structured);
            args = new ComputeBuffer(1, sizeof(uint) * 5, ComputeBufferType.IndirectArguments);

            updateKernel = simulation.FindKernel("Update");
            simulation.SetBuffer(updateKernel, "Particles", particles);
            simulation.SetInt("ParticleCount", count);
            simulation.SetFloat("Lifetime", effect.lifetime);
            simulation.SetVector("InitialVelocity", effect.initialVelocity);
            simulation.SetFloat("RadialForce", effect.radialForce);
            simulation.SetFloat("Turbulence", effect.turbulence);

            drawArgs[0] = 1;
            drawArgs[1] = (uint)count;
            drawArgs[2] = 0;
            drawArgs[3] = 0;
            drawArgs[4] = 0;
            args.SetData(drawArgs);

            particleMaterial.SetBuffer("Particles", particles);
            particleMaterial.SetFloat("_ParticleSize", effect.particleSize);
            particleMaterial.SetColor("_StartColor", effect.startColor);
            particleMaterial.SetColor("_EndColor", effect.endColor);
        }

        void Update()
        {
            if (!particles) return;

            simulation.SetFloat("DeltaTime", Time.deltaTime);
            simulation.SetFloat("TimeValue", Time.time);
            simulation.SetVector("EmitterPosition", transform.position);

            int groups = Mathf.CeilToInt(count / 256f);
            simulation.Dispatch(updateKernel, groups, 1, 1);

            Graphics.DrawProceduralIndirect(
                particleMaterial,
                new Bounds(transform.position, Vector3.one * 1000f),
                MeshTopology.Points,
                args);
        }

        void OnDisable()
        {
            particles?.Release();
            args?.Release();
            particles = null;
            args = null;
        }
    }
}
