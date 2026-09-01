using System;
using UnityEngine;

namespace Veyra
{
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
