using System.Runtime.InteropServices;

namespace FmodParser.Utils;

public static class MemoryExtensions
{
    public static T ReadAt<T>(this Memory<byte> self, int position = 0) where T : unmanaged
    {
        return MemoryMarshal.Read<T>(self.Span[position..]);
    }
}