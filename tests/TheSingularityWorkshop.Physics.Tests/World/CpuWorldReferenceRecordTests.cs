using System.Text.Json;
using NUnit.Framework;
using TheSingularityWorkshop.Physics.Dynamics;
using TheSingularityWorkshop.Physics.Spatial;
using TheSingularityWorkshop.Physics.World;

namespace TheSingularityWorkshop.Physics.Tests.World;

[TestFixture]
public sealed class CpuWorldReferenceRecordTests
{
    [Test]
    public void TwoSphereReferenceRecord_MatchesExecutableStepperOutput()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Records", "cpu-world-two-sphere-reference.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var root = document.RootElement;
        var model = root.GetProperty("model");
        var bodies = root.GetProperty("inputs").EnumerateArray().Select(input =>
        {
            var id = input.GetProperty("body_id").GetString()!;
            var acceleration = Vector(input, "constant_acceleration_m_per_s2");
            var state = new RigidBodyState(
                id,
                new Pose3d(Vector(input, "position_m"), Quaterniond.Identity),
                Vector(input, "linear_velocity_m_per_s"),
                Vector(input, "angular_velocity_rad_per_s"),
                input.GetProperty("step_index").GetInt32(),
                input.GetProperty("elapsed_time_s").GetDouble());
            var definition = new CpuWorldBodyDefinition(
                id,
                input.GetProperty("mass_kg").GetDouble(),
                input.GetProperty("sphere_radius_m").GetDouble(),
                acceleration);
            return new CpuWorldBody(definition, state);
        }).ToArray();

        var configuration = new CpuWorldConfiguration(
            model.GetProperty("model_id").GetString()!,
            model.GetProperty("timestep_seconds").GetDouble(),
            model.GetProperty("restitution").GetDouble(),
            model.GetProperty("contact_tolerance_metres").GetDouble(),
            model.GetProperty("penetration_slop_metres").GetDouble(),
            model.GetProperty("correction_fraction").GetDouble(),
            model.GetProperty("maximum_correction_metres").GetDouble());

        var result = DeterministicCpuWorldStepper.Step(configuration, bodies);
        var expectedOutputs = root.GetProperty("outputs").EnumerateArray().ToArray();
        var expectedContacts = root.GetProperty("contacts").EnumerateArray().ToArray();
        var expectedDiagnostics = root.GetProperty("diagnostics");
        var expectedOrdering = root.GetProperty("ordering");

        Assert.Multiple(() =>
        {
            Assert.That(result.ModelId, Is.EqualTo(model.GetProperty("model_id").GetString()));
            Assert.That(result.StepIndex, Is.EqualTo(expectedOutputs[0].GetProperty("step_index").GetInt32()));
            Assert.That(result.ElapsedTimeSeconds,
                Is.EqualTo(expectedOutputs[0].GetProperty("elapsed_time_s").GetDouble()).Within(1e-12));
            Assert.That(result.Bodies, Has.Count.EqualTo(expectedOutputs.Length));
            Assert.That(result.Bodies.Select(body => body.BodyId), Is.EqualTo(
                expectedOrdering.GetProperty("body_ids").EnumerateArray().Select(id => id.GetString())));
            Assert.That(result.Contacts, Has.Count.EqualTo(expectedContacts.Length));
            Assert.That(result.Diagnostics.BroadphaseCandidateCount,
                Is.EqualTo(expectedDiagnostics.GetProperty("broadphase_candidate_count").GetInt32()));
            Assert.That(result.Diagnostics.EndpointContactCount,
                Is.EqualTo(expectedDiagnostics.GetProperty("endpoint_contact_count").GetInt32()));
            Assert.That(result.Diagnostics.ResolvedContactCount,
                Is.EqualTo(expectedDiagnostics.GetProperty("resolved_contact_count").GetInt32()));
            Assert.That(result.Diagnostics.TotalPositionCorrectionMetres,
                Is.EqualTo(expectedDiagnostics.GetProperty("total_position_correction_m").GetDouble()).Within(1e-12));
            Assert.That(result.Diagnostics.KineticEnergyBeforeJoules,
                Is.EqualTo(expectedDiagnostics.GetProperty("kinetic_energy_before_j").GetDouble()).Within(1e-12));
            Assert.That(result.Diagnostics.KineticEnergyAfterJoules,
                Is.EqualTo(expectedDiagnostics.GetProperty("kinetic_energy_after_j").GetDouble()).Within(1e-12));
            Assert.That(result.Diagnostics.KineticEnergyChangeJoules,
                Is.EqualTo(expectedDiagnostics.GetProperty("kinetic_energy_change_j").GetDouble()).Within(1e-12));
            Assert.That(result.Limitations, Has.Count.EqualTo(root.GetProperty("limitations").GetArrayLength()));
        });

        for (var i = 0; i < expectedOutputs.Length; i++)
        {
            var expected = expectedOutputs[i];
            var actual = result.Bodies[i];
            Assert.Multiple(() =>
            {
                Assert.That(actual.BodyId, Is.EqualTo(expected.GetProperty("body_id").GetString()));
                Assert.That(actual.StepIndex, Is.EqualTo(expected.GetProperty("step_index").GetInt32()));
                Assert.That(actual.ElapsedTimeSeconds,
                    Is.EqualTo(expected.GetProperty("elapsed_time_s").GetDouble()).Within(1e-12));
                AssertVector(actual.Pose.Position, expected, "position_m");
                AssertVector(actual.LinearVelocity, expected, "linear_velocity_m_per_s");
                AssertVector(actual.AngularVelocity, expected, "angular_velocity_rad_per_s");
                AssertQuaternion(actual.Pose.Orientation, expected, "orientation_wxyz");
                Assert.That(expectedOrdering.GetProperty("proxy_ids").GetProperty(actual.BodyId).GetInt64(), Is.EqualTo(i + 1));
            });
        }

        for (var i = 0; i < expectedContacts.Length; i++)
        {
            var expected = expectedContacts[i];
            var actual = result.Contacts[i];
            Assert.Multiple(() =>
            {
                Assert.That(actual.ContactId, Is.EqualTo(expected.GetProperty("contact_id").GetString()));
                Assert.That(actual.BodyAId, Is.EqualTo(expected.GetProperty("body_a_id").GetString()));
                Assert.That(actual.BodyBId, Is.EqualTo(expected.GetProperty("body_b_id").GetString()));
                Assert.That(actual.SignedSeparation,
                    Is.EqualTo(expected.GetProperty("signed_separation_m").GetDouble()).Within(1e-12));
                Assert.That(actual.NormalImpulseNewtonSeconds,
                    Is.EqualTo(expected.GetProperty("normal_impulse_ns").GetDouble()).Within(1e-12));
                Assert.That(actual.NormalForceNewtons, Is.Null);
                AssertVector(actual.Point, expected, "point_m");
                AssertVector(actual.NormalFromAToB, expected, "normal_a_to_b");
                AssertVector(actual.RelativeVelocity, expected, "relative_velocity_m_per_s");
            });
        }

        var expectedMomentum = Vector(expectedDiagnostics, "momentum_residual_kg_m_per_s");
        Assert.That(result.Diagnostics.MomentumResidualKilogramMetresPerSecond.X,
            Is.EqualTo(expectedMomentum.X).Within(1e-12));
        Assert.That(result.Diagnostics.MomentumResidualKilogramMetresPerSecond.Y,
            Is.EqualTo(expectedMomentum.Y).Within(1e-12));
        Assert.That(result.Diagnostics.MomentumResidualKilogramMetresPerSecond.Z,
            Is.EqualTo(expectedMomentum.Z).Within(1e-12));
    }

    private static Vector3d Vector(JsonElement parent, string property)
    {
        var values = parent.GetProperty(property).EnumerateArray().Select(value => value.GetDouble()).ToArray();
        return new Vector3d(values[0], values[1], values[2]);
    }

    private static void AssertQuaternion(Quaterniond actual, JsonElement expected, string property)
    {
        var values = expected.GetProperty(property).EnumerateArray().Select(value => value.GetDouble()).ToArray();
        Assert.That(actual.W, Is.EqualTo(values[0]).Within(1e-12), property + " W");
        Assert.That(actual.X, Is.EqualTo(values[1]).Within(1e-12), property + " X");
        Assert.That(actual.Y, Is.EqualTo(values[2]).Within(1e-12), property + " Y");
        Assert.That(actual.Z, Is.EqualTo(values[3]).Within(1e-12), property + " Z");
    }

    private static void AssertVector(Vector3d actual, JsonElement expected, string property)
    {
        var vector = Vector(expected, property);
        Assert.That(actual.X, Is.EqualTo(vector.X).Within(1e-12), property + " X");
        Assert.That(actual.Y, Is.EqualTo(vector.Y).Within(1e-12), property + " Y");
        Assert.That(actual.Z, Is.EqualTo(vector.Z).Within(1e-12), property + " Z");
    }
}
