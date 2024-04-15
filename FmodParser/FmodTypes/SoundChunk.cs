using Fmod5Sharp;
using Fmod5Sharp.FmodTypes;
using FmodParser.Riff;
using FmodParser.Utils;

namespace FmodParser.FmodTypes;

public class SoundChunk : RiffChunkBase
{
    public FmodSoundBank Bank;
    
    public SoundChunk(BinaryReader reader)
    {
        var bytes = reader.ReadToEnd(); // TODO
        Bank = FsbLoader.LoadFsbFromByteArray(bytes.AsMemory().TrimStart((byte)0));
    }
    
    protected override void WriteData(BinaryWriter writer)
    {
        writer.Align(16);
        Bank.ToStream(writer.BaseStream);
    }

    public override void ToTextWriter(TextWriter writer, int indent = 0)
    {
        //throw new NotImplementedException();
    }
}