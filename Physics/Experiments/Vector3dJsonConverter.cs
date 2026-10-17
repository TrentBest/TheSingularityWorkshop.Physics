using System.Text.Json;
using System.Text.Json.Serialization;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Experiments;

/// <summary>
/// Serializes vectors as their three canonical components. Derived values such as magnitude
/// are calculations, not part of the persisted vector schema.
/// </summary>
internal sealed class Vector3dJsonConverter : JsonConverter<Vector3d>
{
    public override Vector3d Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;
        return new Vector3d(
            root.GetProperty("x").GetDouble(),
            root.GetProperty("y").GetDouble(),
            root.GetProperty("z").GetDouble());
    }

    public override void Write(Utf8JsonWriter writer, Vector3d value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("x", value.X);
        writer.WriteNumber("y", value.Y);
        writer.WriteNumber("z", value.Z);
        writer.WriteEndObject();
    }
}
