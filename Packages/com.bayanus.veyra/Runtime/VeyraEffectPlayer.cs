using System.Collections.Generic;
using UnityEngine;

namespace Veyra
{
    [AddComponentMenu("Veyra/Effect Player")]
    public sealed class VeyraEffectPlayer : MonoBehaviour
    {
        [Header("Program")]
        public VeyraEffectDefinition definition;
        public bool playOnEnable = true;
        public bool loop = true;
        [Min(0.001f)] public float scale = 1f;

        [Header("GPU Particle Backend")]
        public ComputeShader particleSimulation;
        public Material particleMaterial;
        public bool enableParticles = true;

        VeyraIR ir;
        readonly List<LineRenderer> lines = new();
        Material runtimeMaterial;
        VeyraParticleBackend particleBackend;
        float age;
        bool playing;

        void OnEnable()
        {
            if (playOnEnable) Play();
        }

        public void Play()
        {
            Stop();
            if (!definition) return;

            VeyraProgram program = definition.Build();
            if (program == null) return;
            ir = program.Compile();
            if (ir == null) return;

            age = 0f;
            playing = true;
            BuildBeamRenderers();
            BuildParticleBackend();
            UpdateBeams();
            particleBackend?.Update(0f, 0f);
        }

        public void Stop()
        {
            playing = false;
            for (int i = 0; i < lines.Count; i++)
                if (lines[i]) Destroy(lines[i].gameObject);
            lines.Clear();
            if (runtimeMaterial) Destroy(runtimeMaterial);
            runtimeMaterial = null;
            particleBackend?.Dispose();
            particleBackend = null;
            ir = null;
            age = 0f;
        }

        public void Restart() => Play();

        void Update()
        {
            if (!playing || ir == null) return;

            float deltaTime = Time.deltaTime;
            age += deltaTime;
            UpdateBeams();
            particleBackend?.Update(deltaTime, Time.time);

            if (!loop && HasFiniteDuration() && age >= GetDuration())
                Stop();
        }

        void BuildParticleBackend()
        {
            if (!enableParticles || ir == null || ir.emitters.Count == 0) return;
            if (!particleSimulation || !particleMaterial)
            {
                Debug.LogWarning("Veyra: effect contains emitters but particleSimulation and particleMaterial are not assigned. Particle execution is disabled.", this);
                return;
            }

            particleBackend = new VeyraParticleBackend(this, particleSimulation, particleMaterial);
            if (!particleBackend.IsValid)
            {
                particleBackend.Dispose();
                particleBackend = null;
                return;
            }
            particleBackend.Build(ir, transform, loop);
        }

        bool HasFiniteDuration()
        {
            for (int i = 0; i < ir.beams.Count; i++)
            {
                var beam = ir.beams[i];
                if (beam.attack > 0f || beam.decay > 0f || beam.off > 0f)
                    return true;
            }

            for (int i = 0; i < ir.emitters.Count; i++)
                if (ir.emitters[i].burstCount > 0)
                    return true;

            return false;
        }

        float GetDuration()
        {
            float duration = 0f;
            for (int i = 0; i < ir.beams.Count; i++)
            {
                var beam = ir.beams[i];
                float cycle = beam.attack + beam.hold + beam.decay + beam.off;
                if (cycle > duration) duration = cycle;
            }

            for (int i = 0; i < ir.emitters.Count; i++)
            {
                var emitter = ir.emitters[i];
                if (emitter.burstCount > 0)
                    duration = Mathf.Max(duration, emitter.lifetime);
            }
            return duration;
        }

        void BuildBeamRenderers()
        {
            if (ir.beams.Count == 0) return;

            Shader shader = Shader.Find("Veyra/UnlitAdditive");
            if (!shader)
            {
                Debug.LogError("Veyra: shader 'Veyra/UnlitAdditive' was not found.", this);
                return;
            }

            runtimeMaterial = new Material(shader);
            int index = 0;
            foreach (var beam in ir.beams)
            {
                AddLine(index++, beam.width, beam.color, 1f);
                AddLine(index++, beam.width * 3.5f, beam.color, 0.22f);
                for (int b = 0; b < beam.branchCount; b++)
                    AddLine(index++, beam.width * 0.55f, beam.color, 0.8f);
            }
        }

        void AddLine(int index, float width, Color color, float alpha)
        {
            var go = new GameObject("VeyraBeam_" + index);
            go.transform.SetParent(transform, false);
            var line = go.AddComponent<LineRenderer>();
            line.material = runtimeMaterial;
            line.alignment = LineAlignment.View;
            line.textureMode = LineTextureMode.Stretch;
            line.useWorldSpace = false;
            line.positionCount = 2;
            line.widthMultiplier = width * scale;
            var c = color;
            c.a *= alpha;
            line.startColor = c;
            line.endColor = c;
            lines.Add(line);
        }

        void UpdateBeams()
        {
            if (lines.Count == 0) return;

            int lineIndex = 0;
            foreach (var beam in ir.beams)
            {
                GetEnvelope(beam, age, out float intensity, out int cycle);
                uint cycleSeed = (uint)cycle * 2246822519u;
                float localTime = GetCycleTime(beam, age);
                var main = VeyraBeamGenerator.Generate(beam, localTime, cycleSeed);

                ApplyLine(lines[lineIndex++], main, beam.width, beam.color, intensity);
                ApplyLine(lines[lineIndex++], main, beam.width * 3.5f, beam.color, intensity * (0.18f + beam.flicker * 0.08f));

                for (int b = 0; b < beam.branchCount; b++)
                {
                    var branch = GenerateBranch(beam, main, b, localTime, cycleSeed);
                    ApplyLine(lines[lineIndex++], branch, beam.width * 0.55f, beam.color, intensity * (0.55f + beam.flicker * 0.15f));
                }
            }
        }

        static float GetCycleTime(VeyraIRBeam beam, float time)
        {
            float cycle = beam.attack + beam.hold + beam.decay + beam.off;
            return cycle > 0f ? Mathf.Repeat(time, cycle) : time;
        }

        static void GetEnvelope(VeyraIRBeam beam, float time, out float intensity, out int cycleIndex)
        {
            float cycle = beam.attack + beam.hold + beam.decay + beam.off;
            if (cycle <= 0f)
            {
                intensity = 1f;
                cycleIndex = 0;
                return;
            }

            float position = Mathf.Repeat(time, cycle);
            cycleIndex = Mathf.FloorToInt(time / cycle);
            float holdEnd = beam.attack + beam.hold;
            float decayEnd = holdEnd + beam.decay;

            if (beam.attack > 0f && position < beam.attack)
                intensity = position / beam.attack;
            else if (position < holdEnd)
                intensity = 1f;
            else if (beam.decay > 0f && position < decayEnd)
                intensity = 1f - (position - holdEnd) / beam.decay;
            else
                intensity = 0f;
        }

        void ApplyLine(LineRenderer line, List<Vector3> points, float width, Color color, float intensity)
        {
            line.positionCount = points.Count;
            for (int i = 0; i < points.Count; i++)
                line.SetPosition(i, points[i] * scale);

            float pulse = intensity <= 0f ? 0f : 1f - Random01((uint)line.GetInstanceID(), age * 14f) * 0.35f;
            line.widthMultiplier = width * scale * pulse;
            var c = color;
            c.a *= intensity * pulse;
            line.startColor = c;
            line.endColor = c;
        }

        static List<Vector3> GenerateBranch(VeyraIRBeam beam, List<Vector3> main, int branchIndex, float time, uint cycleSeed)
        {
            int sample = 1 + Mathf.Abs((int)(beam.seed + cycleSeed + (uint)branchIndex * 7919u)) % Mathf.Max(1, main.Count - 2);
            Vector3 origin = main[sample];
            Vector3 tangent = (main[Mathf.Min(sample + 1, main.Count - 1)] - main[Mathf.Max(0, sample - 1)]).normalized;
            Vector3 side = Vector3.Cross(tangent, Mathf.Abs(Vector3.Dot(tangent, Vector3.up)) > 0.9f ? Vector3.right : Vector3.up).normalized;
            float angle = Random01(beam.seed + cycleSeed + (uint)branchIndex * 31u, time * beam.speed + branchIndex) * Mathf.PI * 2f;
            Vector3 direction = (side * Mathf.Cos(angle) + Vector3.Cross(tangent, side) * Mathf.Sin(angle) + tangent * 0.2f).normalized;
            float remaining = Vector3.Distance(beam.start, beam.end) * beam.branchLength * (0.45f + Random01(beam.seed + cycleSeed + 99u + (uint)branchIndex, 3.1f) * 0.55f);
            Vector3 end = origin + direction * remaining;
            var branch = new List<Vector3>(7);
            for (int i = 0; i <= 6; i++)
            {
                float t = i / 6f;
                Vector3 p = Vector3.Lerp(origin, end, t);
                if (i > 0 && i < 6)
                    p += side * (Random01(beam.seed + cycleSeed + (uint)(i * 101 + branchIndex), time * beam.speed) * 2f - 1f) * remaining * 0.12f;
                branch.Add(p);
            }
            return branch;
        }

        static float Random01(uint seed, float time)
        {
            return Mathf.Repeat(Mathf.Sin(seed * 0.000123f + time * 12.9898f) * 43758.5453f, 1f);
        }

        void OnDisable() => Stop();
        void OnDestroy() => Stop();
    }
}
