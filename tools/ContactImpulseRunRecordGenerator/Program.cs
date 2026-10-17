using System.Text.Json;
using TheSingularityWorkshop.Physics.Collision;
using TheSingularityWorkshop.Physics.Dynamics;
using TheSingularityWorkshop.Physics.Spatial;

if (args.Length is < 1 or > 2 || (args.Length == 2 && args[1] != "--check"))
{
    Console.Error.WriteLine("Usage: ContactImpulseRunRecordGenerator <output.json>");
    return 2;
}

var bodyA = Body("A", new Vector3d(1d, 0d, 0d));
var bodyB = Body("B", new Vector3d(-1d, 0d, 0d));
const double massA = 1d;
const double massB = 1d;
const double restitution = 0d;
var contact = new ContactObservation(
    "A-B", "A", "B",
    Vector3d.Zero,
    new Vector3d(1d, 0d, 0d),
    new Vector3d(-2d, 0d, 0d),
    -0.1d,
    true,
    3,
    0.3d);

var result = ContactImpulseResolver.Resolve(contact, bodyA, massA, bodyB, massB, restitution);
var record = new
{
    schemaVersion = 1,
    modelId = "collision.single-contact-normal-impulse",
    modelVersion = 1,
    units = new
    {
        mass = "kg",
        velocity = "m/s",
        separation = "m",
        impulse = "N·s",
        time = "s"
    },
    definition = new
    {
        contactNormalFromAToB = Vec(contact.NormalFromAToB),
        signedSeparationMetres = contact.SignedSeparation,
        active = contact.IsActive,
        stepIndex = contact.StepIndex,
        elapsedTimeSeconds = contact.ElapsedTime,
        massAKilograms = massA,
        massBKilograms = massB,
        restitution,
        bodyAInitialVelocityMetresPerSecond = Vec(bodyA.LinearVelocity),
        bodyBInitialVelocityMetresPerSecond = Vec(bodyB.LinearVelocity)
    },
    result = new
    {
        normalImpulseNewtonSeconds = result.Contact.NormalImpulseNewtonSeconds,
        bodyAFinalVelocityMetresPerSecond = Vec(result.BodyA.LinearVelocity),
        bodyBFinalVelocityMetresPerSecond = Vec(result.BodyB.LinearVelocity),
        achievedRelativeVelocityMetresPerSecond = Vec(result.Contact.RelativeVelocity),
        physicalTimeAdvancedSeconds = 0d,
        posePreserved = result.BodyA.Pose == bodyA.Pose && result.BodyB.Pose == bodyB.Pose
    }
};

var json = JsonSerializer.Serialize(record, new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine;
var path = Path.GetFullPath(args[0]);
Directory.CreateDirectory(Path.GetDirectoryName(path)!);
if (File.Exists(path) && args.Length == 2 && args[1] == "--check")
{
    if (File.ReadAllText(path) != json)
    {
        Console.Error.WriteLine("Contact impulse run record differs from the current resolver output.");
        return 1;
    }
    Console.WriteLine("Contact impulse run record matches current resolver output.");
    return 0;
}
File.WriteAllText(path, json);
Console.WriteLine($"Wrote contact impulse run record: {path}");
return 0;

static RigidBodyState Body(string id, Vector3d velocity) =>
    new(id, new Pose3d(Vector3d.Zero, Quaterniond.Identity), velocity, Vector3d.Zero, 2, 0.2d);

static object Vec(Vector3d value) => new { x = value.X, y = value.Y, z = value.Z };
