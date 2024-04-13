using FmodParser.Riff;
using FmodParser.Utils;

namespace FmodParser.FmodTypes;

[DataChunk("MUIB")]
public class MultiSoundChunk : RiffChunkBase
{
    public Guid MultiSoundId { get; set; }

    public MultiSoundChunk(BinaryReader reader)
    {
        MultiSoundId = reader.ReadGuid();
    }
    
    protected override void WriteData(BinaryWriter writer)
    {
        writer.WriteStruct(MultiSoundId);
    }

    public override void ToTextWriter(TextWriter writer, int indent = 0)
    {
        writer.WriteIndented(indent, "Multi Sound");
        writer.WriteIndented(indent, $"Multi Sound ID: {MultiSoundId.ToKnownString()}");
    }
}