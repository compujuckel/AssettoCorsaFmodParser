using System.IO.MemoryMappedFiles;
using DotNext.IO.MemoryMappedFiles;
using Fmod5Sharp.FmodTypes;
using FmodParser.FmodTypes;
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

    public void ToFile(string path)
    {
        using var file = File.Create(path);
        ToStream(file);
    }

    public void ToStream(Stream stream)
    {
        using var writer = new BinaryWriter(stream);
        Root.Write(writer);
    }

    public FmodSoundBank FindSoundBank()
    {
        var chunk = (SoundChunk)Root.SubChunks.First(c => c.Identifier.Span.SequenceEqual("SND "u8));
        return chunk.Bank;
    }

    public void Dispose()
    {
        _accessor.Dispose();
        _file.Dispose();
    }
}