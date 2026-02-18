using System.Globalization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace ChestSnap.Helpers.Yaml;

public sealed class Vector3YamlConverter : IYamlTypeConverter
{
    public bool Accepts(Type type) => type == typeof(Vector3);

    public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        var scalar = parser.Consume<Scalar>();
        var raw = scalar.Value.TrimStart('(').TrimEnd(')');

        var parts = raw.Split([','], StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3)
            throw new YamlException(scalar.Start, scalar.End, $"Expected 3 components for Vector3, got {parts.Length}");

        return new Vector3(
            float.Parse(parts[0], CultureInfo.InvariantCulture),
            float.Parse(parts[1], CultureInfo.InvariantCulture),
            float.Parse(parts[2], CultureInfo.InvariantCulture)
        );
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
    {
        var v = (Vector3)value!;
        emitter.Emit(new Scalar(FormattableString.Invariant($"({v.x}, {v.y}, {v.z})")));
    }
}