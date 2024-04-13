namespace FmodParser.Riff;

public class ListChunk : RiffChunkBase
{
    public Memory<byte> ListIdentifier { get; set; }
    public List<RiffChunkBase> SubChunks { get; set; } = [];

    protected override void WriteData(BinaryWriter writer)
    {
        writer.Write(ListIdentifier.Span);
        foreach (var chunk in SubChunks)
        {
            chunk.Write(writer.BaseStream);
        }
    }

    public override void ToWriter(TextWriter writer, int indent = 0)
    {
        throw new NotImplementedException();
    }
}
