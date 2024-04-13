using System.Runtime.InteropServices;

namespace FmodParser.Utils;

public static class BinaryWriterExtensions
{
    public static void WriteStruct<T>(this BinaryWriter writer, T value) where T : unmanaged
    {
        writer.Write(MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref value, 1)));
    }
}