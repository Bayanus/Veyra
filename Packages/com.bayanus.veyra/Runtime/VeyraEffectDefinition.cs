using UnityEngine;

namespace Veyra
{
    /// <summary>Reusable code-authored effect definition. Generated effect code can derive from this type.</summary>
    public abstract class VeyraEffectDefinition : ScriptableObject
    {
        public abstract VeyraProgram Build();
    }
}
