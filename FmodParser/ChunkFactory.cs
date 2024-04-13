using System.Reflection;
using System.Runtime.InteropServices;
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

    public DataChunk GetDataChunk(Memory<byte> identifier, int length, Memory<byte> data)
    {
        var identifierInt = MemoryMarshal.Read<uint>(identifier.Span);

        if (_typeMap.TryGetValue(identifierInt, out var type))
        {
            var chunk = (DataChunk)Activator.CreateInstance(type, data)!;
            chunk.Identifier = identifier;
            chunk.Length = length;
            chunk.Data = data;

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