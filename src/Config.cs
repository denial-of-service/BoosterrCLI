using System.Collections.Immutable;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Boosterr;

public static class Config
{
    private const string FilePath = "config.yaml";

    public static ImmutableArray<MediaManagerInstance> GetMediaManagerInstances()
    {
        using StreamReader reader = new StreamReader(FilePath);
        IDeserializer deserializer = new DeserializerBuilder()
            .WithTypeConverter(new MediaManagerTypeConverter()) // Add custom type converter
            .Build();
        return [..deserializer.Deserialize<List<MediaManagerInstance>>(reader)];
    }
}

file class MediaManagerTypeConverter : IYamlTypeConverter
{
    public bool Accepts(Type type)
    {
        return type == typeof(MediaManagerType);
    }

    public object ReadYaml(IParser parser, Type type, ObjectDeserializer nestedDeserializer)
    {
        string value = parser.Consume<Scalar>().Value;
        return MediaManagerTypeExtensions.Parse(value);
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer nestedSerializer)
    {
        if (value is MediaManagerType enumValue) emitter.Emit(new Scalar(enumValue.ToString()));
    }
}