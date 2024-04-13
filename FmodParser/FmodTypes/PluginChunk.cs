using FmodParser.Riff;
using FmodParser.Utils;

namespace FmodParser.FmodTypes;

[DataChunk("PEFB")]
public class PluginChunk : RiffChunkBase
{
    public Guid PluginId { get; set; }
    public byte[] Unknown { get; set; }

    public PluginChunk(BinaryReader reader)
    {
        PluginId = reader.ReadGuid();
        Unknown = reader.ReadToEnd();
    }
    
    protected override void WriteData(BinaryWriter writer)
    {
        writer.WriteStruct(PluginId);
        writer.Write(Unknown);
    }

    public override void ToTextWriter(TextWriter writer, int indent = 0)
    {
        writer.WriteIndented(indent, "Plugin");
        writer.WriteIndented(indent, $"Plugin ID: {PluginId.ToKnownString()}");
        writer.WriteIndented(indent, Convert.ToHexString(Unknown));
    }
}