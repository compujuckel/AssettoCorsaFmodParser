using System.Runtime.InteropServices;
using FmodParser.Riff;
using FmodParser.Utils;

namespace FmodParser.FmodTypes;

[DataChunk("WAIB")]
public class SingleSoundChunk : DataChunk
{
    public Guid SingleSoundId { get; set; }
    public Guid AudioFileId { get; set; }

    public SingleSoundChunk(Memory<byte> data)
    {
        SingleSoundId = MemoryMarshal.Read<Guid>(data.Span);
        AudioFileId = MemoryMarshal.Read<Guid>(data.Span[16..]);
    }

    public override void ToWriter(TextWriter writer, int indent = 0)
    {
        writer.WriteIndented(indent, "Single Sound");
        writer.WriteIndented(indent, $"Single Sound ID: {SingleSoundId.ToKnownString()}");
        writer.WriteIndented(indent, $"Audio File ID: {AudioFileId.ToKnownString()}");
    }
}