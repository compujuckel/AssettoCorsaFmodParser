using FmodParser.Riff;
using FmodParser.Utils;

namespace FmodParser.FmodTypes;

public class ModulatorChunk : RiffChunkBase
{
    public Guid ModulatorId { get; set; }
    public Guid SourceId { get; set; }
    public byte[] Unknown { get; set; }

    public ModulatorChunk(BinaryReader reader)
    {
        ModulatorId = reader.ReadGuid();
        SourceId = reader.ReadGuid();
        Unknown = reader.ReadToEnd();
    }
    
    protected override void WriteData(BinaryWriter writer)
    {
        writer.WriteStruct(ModulatorId);
        writer.WriteStruct(SourceId);
        writer.Write(Unknown);
    }

    public override void ToTextWriter(TextWriter writer, int indent = 0)
    {
        writer.WriteIndented(indent, "Modulator");
        writer.WriteIndented(indent, $"Modulator ID: {ModulatorId.ToKnownString()}");
        writer.WriteIndented(indent, $"Source ID: {SourceId.ToKnownString()}");
        writer.WriteIndented(indent, Convert.ToHexString(Unknown));
    }
}