using System.IO.MemoryMappedFiles;
using DotNext.IO.MemoryMappedFiles;
using FmodParser.Riff;

namespace FmodParser;

public class FmodFile : IDisposable
{
    private readonly MemoryMappedFile _file;
    private readonly IMappedMemoryOwner _accessor;

    public readonly RiffChunkBase RootChunk;

    public FmodFile(MemoryMappedFile file, IMappedMemoryOwner accessor, RiffChunkBase rootChunk)
    {
        _file = file;
        _accessor = accessor;
        RootChunk = rootChunk;
    }

    public void Dispose()
    {
        _accessor.Dispose();
        _file.Dispose();
    }
}