using UnityEngine;

namespace Veyra.Examples
{
    [CreateAssetMenu(menuName = "Veyra/Examples/Succulent Lightning", fileName = "SucculentLightning")]
    public sealed class VeyraLightningEffect : VeyraEffectDefinition
    {
        public float length = 8f;
        public float width = 0.08f;
        public int branches = 7;
        public float jaggedness = 1.15f;
        public float flicker = 0.35f;

        public override VeyraProgram Build()
        {
            var fx = VeyraProgram.Create("Succulent Lightning");
            var start = Vector3.zero;
            var end = Vector3.right * length;

            fx.Beam("Core")
                .From(start).To(end)
                .Segments(30)
                .Jagged(jaggedness)
                .Width(width)
                .Branches(branches)
                .BranchLength(0.42f)
                .Flicker(flicker)
                .Speed(22f)
                .Color(new Color(0.72f, 0.18f, 1f, 1f));

            fx.Beam("HotCore")
                .From(start).To(end)
                .Segments(34)
                .Jagged(jaggedness * 0.72f)
                .Width(width * 0.42f)
                .Branches(0)
                .Flicker(flicker)
                .Speed(28f)
                .Color(new Color(0.9f, 0.8f, 1f, 1f));

            fx.Beam("Halo")
                .From(start).To(end)
                .Segments(24)
                .Jagged(jaggedness * 1.25f)
                .Width(width * 2.8f)
                .Branches(Mathf.Max(2, branches / 2))
                .BranchLength(0.3f)
                .Flicker(flicker * 1.25f)
                .Speed(16f)
                .Color(new Color(0.38f, 0.05f, 0.85f, 0.7f));

            return fx;
        }
    }
}
