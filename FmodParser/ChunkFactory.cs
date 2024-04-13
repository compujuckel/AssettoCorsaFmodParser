using System.Reflection;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;
using FmodParser.Riff;

namespace FmodParser;

public class ChunkFactory
{
    private readonly Dictionary<uint, Type> _typeMap = new();

    public ChunkFactory()
    {
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
        {
            var dataChunkAttr = type.GetCustomAttribute<DataChunkAttribute>();
            if (dataChunkAttr == null) continue;

            var identifierInt = MemoryMarshal.Read<uint>(dataChunkAttr.Identifier);

            _typeMap.Add(identifierInt, type);
        }
    }

    public RiffChunkBase GetDataChunk(Memory<byte> identifier, int length, Memory<byte> data)
    {
        var identifierInt = MemoryMarshal.Read<uint>(identifier.Span);

        if (_typeMap.TryGetValue(identifierInt, out var type))
        {
            using var stream = data.AsStream();
            using var reader = new BinaryReader(stream);
            var chunk = (RiffChunkBase)Activator.CreateInstance(type, reader)!;
            chunk.Identifier = identifier;
            chunk.Length = length;

            return chunk;
        }

        return new DataChunk
        {
            Identifier = identifier,
            Length = length,
            Data = data
        };
    }
}