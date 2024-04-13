using System.IO.MemoryMappedFiles;
using DotNext.IO.MemoryMappedFiles;
using FmodParser.Riff;

namespace FmodParser;

public class FmodFile : IDisposable
{
    private readonly MemoryMappedFile _file;
    private readonly IMappedMemoryOwner _accessor;

    public readonly ListChunk Root;

    public FmodFile(MemoryMappedFile file, IMappedMemoryOwner accessor, ListChunk root)
    {
        _file = file;
        _accessor = accessor;
        Root = root;
    }

    public void Dispose()
    {
        _accessor.Dispose();
        _file.Dispose();
    }
}