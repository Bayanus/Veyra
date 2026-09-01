using NUnit.Framework;
using UnityEngine;

namespace Veyra.Tests
{
    public sealed class VeyraProgramTests
    {
        [Test]
        public void BeamCompilesIntoIR()
        {
            var program = VeyraProgram.Create("Test");
            program.Beam("Strike")
                .From(Vector3.zero)
                .To(Vector3.right * 8f)
                .Segments(32)
                .Jagged(1.1f)
                .Width(0.08f)
                .Branches(4)
                .Envelope(0f, 0.06f, 0.5f, 0.5f);

            var ir = program.Compile();

            Assert.That(ir, Is.Not.Null);
            Assert.That(ir.version, Is.EqualTo(4));
            Assert.That(ir.name, Is.EqualTo("Test"));
            Assert.That(ir.beams, Has.Count.EqualTo(1));
            Assert.That(ir.beams[0].name, Is.EqualTo("Strike"));
            Assert.That(ir.beams[0].segments, Is.EqualTo(32));
            Assert.That(ir.beams[0].branchCount, Is.EqualTo(4));
            Assert.That(ir.beams[0].attack, Is.EqualTo(0f));
            Assert.That(ir.beams[0].hold, Is.EqualTo(0.06f));
            Assert.That(ir.beams[0].decay, Is.EqualTo(0.5f));
            Assert.That(ir.beams[0].off, Is.EqualTo(0.5f));
        }

        [Test]
        public void BeamParametersAreClamped()
        {
            var program = VeyraProgram.Create();
            program.Beam()
                .Segments(999)
                .Width(-1f)
                .Branches(999)
                .Jagged(-1f)
                .Flicker(2f)
                .Speed(-5f)
                .Envelope(-1f, -2f, -3f, -4f);

            var beam = program.Compile().beams[0];

            Assert.That(beam.segments, Is.EqualTo(256));
            Assert.That(beam.width, Is.EqualTo(0.001f));
            Assert.That(beam.branchCount, Is.EqualTo(64));
            Assert.That(beam.jaggedness, Is.EqualTo(0f));
            Assert.That(beam.flicker, Is.EqualTo(1f));
            Assert.That(beam.speed, Is.EqualTo(0f));
            Assert.That(beam.attack, Is.EqualTo(0f));
            Assert.That(beam.hold, Is.EqualTo(0f));
            Assert.That(beam.decay, Is.EqualTo(0f));
            Assert.That(beam.off, Is.EqualTo(0f));
        }

        [Test]
        public void EmitterAndFieldsCompileIntoIR()
        {
            var gradient = new Gradient();
            var program = VeyraProgram.Create("Particles");
            program.Emitter("Smoke")
                .CapacityCount(4096)
                .Burst(32)
                .At(new Vector3(1f, 2f, 3f))
                .Velocity(Vector3.up * 4f)
                .Lifetime(3f)
                .LifetimeRandom(0.25f)
                .Size(0.2f)
                .SizeRandom(0.5f)
                .Color(gradient);
            program.Field(VeyraFieldType.Gravity, 9.81f).At(Vector3.zero).Within(20f);
            program.Field(VeyraFieldType.Vortex, 2f).At(Vector3.zero).Within(5f);
            program.Render(VeyraRenderType.Billboard);

            var ir = program.Compile();

            Assert.That(ir.emitters, Has.Count.EqualTo(1));
            Assert.That(ir.emitters[0].capacity, Is.EqualTo(4096));
            Assert.That(ir.emitters[0].burstCount, Is.EqualTo(32));
            Assert.That(ir.emitters[0].lifetime, Is.EqualTo(3f));
            Assert.That(ir.fields, Has.Count.EqualTo(2));
            Assert.That(ir.fields[0].type, Is.EqualTo(VeyraFieldType.Gravity));
            Assert.That(ir.fields[1].type, Is.EqualTo(VeyraFieldType.Vortex));
            Assert.That(ir.renders, Has.Count.EqualTo(1));
        }

        [Test]
        public void EmitterCapacityIsClamped()
        {
            var program = VeyraProgram.Create();
            program.Emitter().CapacityCount(0);
            Assert.That(program.Compile().emitters[0].capacity, Is.EqualTo(1));
        }

        [Test]
        public void MultipleBeamsGetStableDistinctSeeds()
        {
            var program = VeyraProgram.Create();
            program.Beam("A");
            program.Beam("B");

            var ir = program.Compile();

            Assert.That(ir.beams[0].seed, Is.Not.EqualTo(ir.beams[1].seed));
        }
    }
}
