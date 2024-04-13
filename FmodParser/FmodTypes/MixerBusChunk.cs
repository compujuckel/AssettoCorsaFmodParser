using System.Runtime.InteropServices;
using FmodParser.Riff;
using FmodParser.Utils;

namespace FmodParser.FmodTypes;

[DataChunk("MBSB")]
public class MixerBusChunk : DataChunk
{
    public Guid MixerBusId { get; set; }
    public Memory<byte> Unknown { get; set; }

    public MixerBusChunk(Memory<byte> data)
    {
        MixerBusId = MemoryMarshal.Read<Guid>(data.Span);
        Unknown = data[16..];
    }

    public override void ToWriter(TextWriter writer, int indent = 0)
    {
        writer.WriteIndented(indent, "Mixer Bus");
        writer.WriteIndented(indent, $"Mixer Bus ID: {MixerBusId.ToKnownString()}");
        writer.WriteIndented(indent, Convert.ToHexString(Unknown.Span));
    }
}