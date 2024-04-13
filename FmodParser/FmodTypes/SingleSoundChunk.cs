using FmodParser.Riff;
using FmodParser.Utils;

namespace FmodParser.FmodTypes;

[DataChunk("WAIB")]
public class SingleSoundChunk : RiffChunkBase
{
    public Guid SingleSoundId { get; set; }
    public Guid AudioFileId { get; set; }

    public SingleSoundChunk(BinaryReader reader)
    {
        SingleSoundId = reader.ReadGuid();
        AudioFileId = reader.ReadGuid();
    }

    protected override void WriteData(BinaryWriter writer)
    {
        writer.WriteStruct(SingleSoundId);
        writer.WriteStruct(AudioFileId);
    }

    public override void ToTextWriter(TextWriter writer, int indent = 0)
    {
        writer.WriteIndented(indent, "Single Sound");
        writer.WriteIndented(indent, $"Single Sound ID: {SingleSoundId.ToKnownString()}");
        writer.WriteIndented(indent, $"Audio File ID: {AudioFileId.ToKnownString()}");
    }
}