using UnityEngine;

namespace Veyra.Examples
{
    [CreateAssetMenu(menuName = "Veyra/Examples/Succulent Lightning", fileName = "SucculentLightning")]
    public sealed class VeyraLightningEffect : VeyraEffectDefinition
    {
        [Min(0.1f)] public float length = 8f;
        [Min(0.001f)] public float width = 0.08f;
        [Range(0, 64)] public int branches = 7;
        [Min(0f)] public float jaggedness = 1.15f;
        [Range(0f, 1f)] public float flicker = 0.35f;
        [Min(0f)] public float flashHold = 0.06f;
        [Min(0f)] public float fade = 0.5f;
        [Min(0f)] public float pause = 0.5f;

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
                .Envelope(0f, flashHold, fade, pause)
                .Color(new Color(0.72f, 0.18f, 1f, 1f));

            fx.Beam("HotCore")
                .From(start).To(end)
                .Segments(34)
                .Jagged(jaggedness * 0.72f)
                .Width(width * 0.42f)
                .Branches(0)
                .Flicker(flicker)
                .Speed(28f)
                .Envelope(0f, flashHold * 0.8f, fade * 0.72f, pause)
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
                .Envelope(0f, flashHold * 1.2f, fade * 1.15f, pause)
                .Color(new Color(0.38f, 0.05f, 0.85f, 0.7f));

            return fx;
        }
    }
}
