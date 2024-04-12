namespace FmodParser;

public class ListChunk : RiffChunkBase
{
    public Memory<byte> ListIdentifier;
    public List<RiffChunkBase> SubChunks;
}