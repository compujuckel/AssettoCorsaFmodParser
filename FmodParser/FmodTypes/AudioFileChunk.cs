using FmodParser.Riff;
using FmodParser.Utils;

namespace FmodParser.FmodTypes;

[DataChunk("WAV ")]
public class AudioFileChunk : RiffChunkBase
{
    public Guid AudioFileId { get; set; }
    public byte[] Unknown1 { get; set; }
    public ushort SampleIndex { get; set; }
    public byte[] Unknown2 { get; set; }

    public AudioFileChunk(BinaryReader reader)
    {
        AudioFileId = reader.ReadGuid();
        Unknown1 = reader.ReadBytes(6);
        SampleIndex = reader.ReadUInt16();
        Unknown2 = reader.ReadToEnd();
    }

    protected override void WriteData(BinaryWriter writer)
    {
        writer.WriteStruct(AudioFileId);
        writer.Write(Unknown1);
        writer.WriteStruct(SampleIndex);
        writer.Write(Unknown2);
    }

    public override void ToTextWriter(TextWriter writer, int indent = 0)
    {
        writer.WriteIndented(indent + 1, "Audio File");
        writer.WriteIndented(indent + 1, $"Audio File ID: {AudioFileId.ToKnownString()}");
        writer.WriteIndented(indent + 1, Convert.ToHexString(Unknown1));
        writer.WriteIndented(indent + 1, $"Sample Index: {SampleIndex}");
        writer.WriteIndented(indent + 1, Convert.ToHexString(Unknown2));
    }
}