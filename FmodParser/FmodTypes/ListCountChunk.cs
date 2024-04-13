using System.Runtime.InteropServices;
using FmodParser.Riff;

namespace FmodParser.FmodTypes;

[DataChunk("LCNT")]
public class ListCountChunk : DataChunk
{
    public int ListCount { get; set; }

    public ListCountChunk(Memory<byte> data)
    {
        ListCount = MemoryMarshal.Read<int>(data.Span);
    }
}