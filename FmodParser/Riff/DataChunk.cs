using FmodParser.Utils;

namespace FmodParser.Riff;

public class DataChunk : RiffChunkBase
{
    public Memory<byte> Data { get; set; }

    public override void ToWriter(TextWriter writer, int indent = 0)
    {
        writer.WriteIndented(indent, Convert.ToHexString(Data.Span));
    }

    protected override void WriteData(BinaryWriter writer)
    {
        writer.Write(Data.Span);
    }
}