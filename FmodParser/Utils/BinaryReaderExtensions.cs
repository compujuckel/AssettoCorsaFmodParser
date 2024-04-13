using System.Runtime.InteropServices;

namespace FmodParser.Utils;

public static class BinaryReaderExtensions
{
    public static Guid ReadGuid(this BinaryReader reader)
    {
        ReadOnlySpan<long> span = stackalloc long[] { reader.ReadInt64(), reader.ReadInt64() };
        return MemoryMarshal.Read<Guid>(MemoryMarshal.AsBytes(span));
    }

    public static byte[] ReadToEnd(this BinaryReader reader)
    {
        var count = reader.BaseStream.Length - reader.BaseStream.Position;
        return reader.ReadBytes((int)count);
    }
}