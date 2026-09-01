using System.Collections.Generic;
using UnityEngine;

namespace Veyra
{
    [AddComponentMenu("Veyra/Effect Player")]
    public sealed class VeyraEffectPlayer : MonoBehaviour
    {
        public VeyraEffectDefinition definition;
        public bool playOnEnable = true;
        public bool loop = true;
        public float scale = 1f;

        VeyraIR ir;
        readonly List<LineRenderer> lines = new();
        Material runtimeMaterial;
        float age;

        void OnEnable()
        {
            if (playOnEnable) Play();
        }

        public void Play()
        {
            Stop();
            if (!definition) return;
            ir = definition.Build()?.Compile();
            if (ir == null) return;
            age = 0f;
            BuildBeamRenderers();
        }

        public void Stop()
        {
            for (int i = 0; i < lines.Count; i++)
                if (lines[i]) Destroy(lines[i].gameObject);
            lines.Clear();
            if (runtimeMaterial) Destroy(runtimeMaterial);
            runtimeMaterial = null;
            ir = null;
        }

        void Update()
        {
            if (ir == null) return;
            age += Time.deltaTime;
            UpdateBeams();
            if (!loop && age > 60f) Stop();
        }

        void BuildBeamRenderers()
        {
            runtimeMaterial = new Material(Shader.Find("Veyra/UnlitAdditive"));
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
            line.positionCount = 2;
            line.widthMultiplier = width * scale;
            var c = color; c.a *= alpha;
            line.startColor = c; line.endColor = c;
            lines.Add(line);
        }

        void UpdateBeams()
        {
            int lineIndex = 0;
            foreach (var beam in ir.beams)
            {
                var main = VeyraBeamGenerator.Generate(beam, age);
                ApplyLine(lines[lineIndex++], main, beam.width, beam.color, 1f);
                ApplyLine(lines[lineIndex++], main, beam.width * 3.5f, beam.color, 0.18f + beam.flicker * 0.08f);

                for (int b = 0; b < beam.branchCount; b++)
                {
                    var branch = GenerateBranch(beam, main, b, age);
                    ApplyLine(lines[lineIndex++], branch, beam.width * 0.55f, beam.color, 0.55f + beam.flicker * 0.15f);
                }
            }
        }

        void ApplyLine(LineRenderer line, List<Vector3> points, float width, Color color, float alpha)
        {
            line.positionCount = points.Count;
            for (int i = 0; i < points.Count; i++)
                line.SetPosition(i, transform.InverseTransformPoint(transform.TransformPoint(points[i] * scale)));
            float pulse = 1f - Random01((uint)line.GetInstanceID(), age * 14f) * 0.35f;
            line.widthMultiplier = width * scale * pulse;
            var c = color; c.a *= alpha * pulse;
            line.startColor = c; line.endColor = c;
        }

        static List<Vector3> GenerateBranch(VeyraIRBeam beam, List<Vector3> main, int branchIndex, float time)
        {
            int sample = 1 + Mathf.Abs((int)(beam.seed + (uint)branchIndex * 7919u)) % Mathf.Max(1, main.Count - 2);
            Vector3 origin = main[sample];
            Vector3 tangent = (main[Mathf.Min(sample + 1, main.Count - 1)] - main[Mathf.Max(0, sample - 1)]).normalized;
            Vector3 side = Vector3.Cross(tangent, Mathf.Abs(Vector3.Dot(tangent, Vector3.up)) > 0.9f ? Vector3.right : Vector3.up).normalized;
            float angle = Random01(beam.seed + (uint)branchIndex * 31u, time * beam.speed + branchIndex) * Mathf.PI * 2f;
            Vector3 direction = (side * Mathf.Cos(angle) + Vector3.Cross(tangent, side) * Mathf.Sin(angle) + tangent * 0.2f).normalized;
            float remaining = Vector3.Distance(beam.start, beam.end) * beam.branchLength * (0.45f + Random01(beam.seed + 99u + (uint)branchIndex, 3.1f) * 0.55f);
            Vector3 end = origin + direction * remaining;
            var branch = new List<Vector3>(7);
            for (int i = 0; i <= 6; i++)
            {
                float t = i / 6f;
                Vector3 p = Vector3.Lerp(origin, end, t);
                if (i > 0 && i < 6)
                    p += side * (Random01(beam.seed + (uint)(i * 101 + branchIndex), time * beam.speed) * 2f - 1f) * remaining * 0.12f;
                branch.Add(p);
            }
            return branch;
        }

        static float Random01(uint seed, float time)
        {
            return Mathf.Repeat(Mathf.Sin(seed * 0.000123f + time * 12.9898f) * 43758.5453f, 1f);
        }

        void OnDisable() => Stop();
    }
}
