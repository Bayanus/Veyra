using System;
using UnityEngine;

namespace Veyra
{
    /// <summary>
    /// Legacy prototype component retained only for source compatibility.
    /// Use VeyraEffectPlayer with VeyraEffectDefinition for runtime execution.
    /// </summary>
    [Obsolete("VeyraRuntime is a legacy prototype. Use VeyraEffectPlayer with VeyraEffectDefinition.", false)]
    public sealed class VeyraRuntime : MonoBehaviour
    {
        public VeyraEffect effect;
        public ComputeShader simulation;
        public Material particleMaterial;

        void OnEnable()
        {
            Debug.LogWarning("VeyraRuntime is a legacy prototype and is disabled. Use VeyraEffectPlayer with a VeyraEffectDefinition.", this);
        }
    }
}
