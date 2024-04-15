using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;
using FmodParser.FmodTypes;
using FmodParser.Riff;

namespace FmodParser;

public class ChunkFactory
{
    private static uint Int(ReadOnlySpan<byte> id) => MemoryMarshal.Read<uint>(id);

    private readonly Dictionary<uint, Func<BinaryReader, RiffChunkBase>> _map = new()
    {
        { Int("WAV "u8), reader => new AudioFileChunk(reader) },
        { Int("EVTB"u8), reader => new EventChunk(reader) },
        { Int("LCNT"u8), reader => new ListCountChunk(reader) },
        { Int("MBSB"u8), reader => new MixerBusChunk(reader) },
        { Int("MODU"u8), reader => new ModulatorChunk(reader) },
        { Int("MUIB"u8), reader => new MultiSoundChunk(reader) },
        { Int("PEFB"u8), reader => new PluginChunk(reader) },
        { Int("WAIB"u8), reader => new SingleSoundChunk(reader) },
        { Int("SND "u8), reader => new SoundChunk(reader) },
        { Int("TLNB"u8), reader => new TimelineChunk(reader) },
    };

    public RiffChunkBase GetDataChunk(Memory<byte> identifier, int length, Memory<byte> data)
    {
        var identifierInt = MemoryMarshal.Read<uint>(identifier.Span);

        if (_map.TryGetValue(identifierInt, out var factory))
        {
            using var stream = data.AsStream();
            using var reader = new BinaryReader(stream);
            var chunk = factory(reader);
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