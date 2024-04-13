using FmodParser.Riff;
using FmodParser.Utils;

namespace FmodParser.FmodTypes;

[DataChunk("MBSB")]
public class MixerBusChunk : RiffChunkBase
{
    public Guid MixerBusId { get; set; }
    public byte[] Unknown { get; set; }

    public MixerBusChunk(BinaryReader reader)
    {
        MixerBusId = reader.ReadGuid();
        Unknown = reader.ReadToEnd();
    }

    protected override void WriteData(BinaryWriter writer)
    {
        writer.WriteStruct(MixerBusId);
        writer.Write(Unknown);
    }

    public override void ToTextWriter(TextWriter writer, int indent = 0)
    {
        writer.WriteIndented(indent, "Mixer Bus");
        writer.WriteIndented(indent, $"Mixer Bus ID: {MixerBusId.ToKnownString()}");
        writer.WriteIndented(indent, Convert.ToHexString(Unknown));
    }
}