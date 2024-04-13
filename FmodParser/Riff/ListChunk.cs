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
            chunk.Write(writer);
        }
    }

    public override void ToTextWriter(TextWriter writer, int indent = 0)
    {
        throw new NotImplementedException();
    }
}
