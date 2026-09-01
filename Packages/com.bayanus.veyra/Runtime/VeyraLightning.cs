using System;
using System.Collections.Generic;
using UnityEngine;

namespace Veyra
{
    [Serializable]
    public sealed class VeyraBeamNode
    {
        internal string Name;
        internal Vector3 Start;
        internal Vector3 End = Vector3.forward * 5f;
        internal int Segments = 24;
        internal float Jaggedness = 0.75f;
        internal float Width = 0.08f;
        internal int BranchCount;
        internal float BranchLength = 0.35f;
        internal float Flicker = 0.15f;
        internal float Speed = 18f;
        internal Color ColorValue = Color.white;
        internal uint Seed;
        internal float Attack;
        internal float Hold = 1f;
        internal float Decay;
        internal float Off = 1f;

        internal VeyraBeamNode(string name, uint seed)
        {
            Name = string.IsNullOrWhiteSpace(name) ? "Beam" : name;
            Seed = seed;
        }

        public VeyraBeamNode From(Vector3 start) { Start = start; return this; }
        public VeyraBeamNode To(Vector3 end) { End = end; return this; }
        public VeyraBeamNode Segments(int count) { Segments = Mathf.Clamp(count, 2, 256); return this; }
        public VeyraBeamNode Jagged(float amount) { Jaggedness = Mathf.Max(0f, amount); return this; }
        public VeyraBeamNode Width(float width) { Width = Mathf.Max(0.001f, width); return this; }
        public VeyraBeamNode Branches(int count) { BranchCount = Mathf.Clamp(count, 0, 64); return this; }
        public VeyraBeamNode BranchLength(float amount) { BranchLength = Mathf.Clamp01(amount); return this; }
        public VeyraBeamNode Flicker(float amount) { Flicker = Mathf.Clamp01(amount); return this; }
        public VeyraBeamNode Speed(float speed) { Speed = Mathf.Max(0f, speed); return this; }
        public VeyraBeamNode Color(Color color) { ColorValue = color; return this; }

        /// <summary>Controls a repeating visibility envelope. Zero values are instantaneous.</summary>
        public VeyraBeamNode Envelope(float attack, float hold, float decay, float off)
        {
            Attack = Mathf.Max(0f, attack);
            Hold = Mathf.Max(0f, hold);
            Decay = Mathf.Max(0f, decay);
            Off = Mathf.Max(0f, off);
            return this;
        }
    }

    [Serializable]
    public sealed class VeyraIRBeam
    {
        public string name;
        public Vector3 start;
        public Vector3 end;
        public int segments;
        public float jaggedness;
        public float width;
        public int branchCount;
        public float branchLength;
        public float flicker;
        public float speed;
        public Color color;
        public uint seed;
        public float attack;
        public float hold = 1f;
        public float decay;
        public float off = 1f;
    }

    internal static class VeyraBeamGenerator
    {
        public static List<Vector3> Generate(VeyraIRBeam beam, float time)
        {
            var points = new List<Vector3>(beam.segments + 1);
            Vector3 axis = beam.end - beam.start;
            float length = axis.magnitude;
            if (length < 0.0001f)
            {
                points.Add(beam.start);
                points.Add(beam.end);
                return points;
            }

            Vector3 forward = axis / length;
            Vector3 side = Vector3.Cross(forward, Mathf.Abs(Vector3.Dot(forward, Vector3.up)) > 0.92f ? Vector3.right : Vector3.up).normalized;
            Vector3 side2 = Vector3.Cross(forward, side).normalized;
            float flickerPhase = time * beam.speed;

            for (int i = 0; i <= beam.segments; i++)
            {
                float t = i / (float)beam.segments;
                Vector3 p = Vector3.Lerp(beam.start, beam.end, t);
                if (i != 0 && i != beam.segments)
                {
                    float envelope = Mathf.Sin(t * Mathf.PI);
                    float n1 = Noise(beam.seed, i, flickerPhase, 0f) * 2f - 1f;
                    float n2 = Noise(beam.seed, i, flickerPhase, 31f) * 2f - 1f;
                    float amount = beam.jaggedness * length * 0.12f * envelope;
                    p += (side * n1 + side2 * n2) * amount;
                }
                points.Add(p);
            }
            return points;
        }

        static float Noise(uint seed, int index, float time, float channel)
        {
            float x = seed * 0.000123f + index * 12.9898f + channel * 78.233f + time * 0.173f;
            return Mathf.Repeat(Mathf.Sin(x) * 43758.5453f, 1f);
        }
    }
}
