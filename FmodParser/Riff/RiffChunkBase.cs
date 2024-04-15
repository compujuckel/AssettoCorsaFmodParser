using System.Text;
using FmodParser.Utils;

namespace FmodParser.Riff;

public abstract class RiffChunkBase
{
    public Memory<byte> Identifier { get; set; }
    public int Length { get; set; }

    public void Write(BinaryWriter writer)
    {
        writer.Write(Identifier.Span);
        var lenOffset = writer.BaseStream.Position;
        writer.Write(0);
        
        WriteData(writer);

        var endOffset = writer.BaseStream.Position;
        var len = endOffset - lenOffset - 4;
        writer.BaseStream.Seek(lenOffset, SeekOrigin.Begin);
        writer.Write((uint)len);
        writer.BaseStream.Seek(endOffset, SeekOrigin.Begin);
        
        writer.Align(2);

        if (len != Length
            && !Identifier.Span.SequenceEqual("SND "u8)
            && !Identifier.Span.SequenceEqual("RIFF"u8))
        {
            throw new Exception($"Length of chunk {Encoding.ASCII.GetString(Identifier.Span)} changed. Please report this bug");
        }
    }

    protected abstract void WriteData(BinaryWriter writer);

    public abstract void ToTextWriter(TextWriter writer, int indent = 0);

    public new string ToString()
    {
        return $"Chunk {Encoding.ASCII.GetString(Identifier.Span)}, len={Length}";
    }
}