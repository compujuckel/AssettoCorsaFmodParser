using System.Runtime.InteropServices;

namespace FmodParser.Utils;

public static class BinaryWriterExtensions
{
    public static void WriteStruct<T>(this BinaryWriter writer, T value) where T : unmanaged
    {
        writer.Write(MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref value, 1)));
    }
    
    internal static void Align(this BinaryWriter writer, int numBytes)
    {
        var pos = writer.BaseStream.Position;
        if (pos % 32 == 0) return;
            
        for (var i = 0; i < 32 - pos % 32; i++)
        {
            writer.Write((byte)0);
        }
    }
}