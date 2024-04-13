﻿namespace FmodParser.Riff;

public abstract class RiffChunkBase
{
    public Memory<byte> Identifier { get; set; }
    public int Length { get; set; }

    public void Write(Stream stream)
    {
        using var writer = new BinaryWriter(stream);
        writer.Write(Identifier.Span);
        var lenOffset = writer.BaseStream.Position;
        writer.Write(0);
        
        WriteData(writer);

        var endOffset = writer.BaseStream.Position;
        var len = endOffset - lenOffset + 4;
        writer.BaseStream.Seek(lenOffset, SeekOrigin.Begin);
        writer.Write((uint)len);
        writer.BaseStream.Seek(endOffset, SeekOrigin.Begin);
    }

    protected abstract void WriteData(BinaryWriter writer);

    public abstract void ToWriter(TextWriter writer, int indent = 0);
}