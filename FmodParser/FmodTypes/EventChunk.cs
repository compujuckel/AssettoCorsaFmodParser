using FmodParser.Riff;
using FmodParser.Utils;

namespace FmodParser.FmodTypes;

[DataChunk("EVTB")]
public class EventChunk : RiffChunkBase
{
    public Guid EventId { get; set; }
    public byte[] Unknown1 { get; set; }
    public Guid TimelineId { get; set; }
    public Guid MixerInputId { get; set; }
    public Guid EventMixerMasterId { get; set; }
    public int MaxInstances { get; set; }
    public ushort Priority { get; set; }
    public byte[] Unknown2 { get; set; }
    public Guid[] Parameters { get; set; }
    public byte[] Unknown3 { get; set; }
    public float Scale { get; set; }
    public byte[] Unknown4 { get; set; }
    public float Cooldown { get; set; }
    public byte[] Unknown5 { get; set; }

    public EventChunk(BinaryReader reader)
    {
        EventId = reader.ReadGuid();
        Unknown1 = reader.ReadBytes(16);
        TimelineId = reader.ReadGuid();
        MixerInputId = reader.ReadGuid();
        EventMixerMasterId = reader.ReadGuid();
        MaxInstances = reader.ReadInt32();
        Priority = reader.ReadUInt16();
        Unknown2 = reader.ReadBytes(11);

        var hasUnknownExtraField = (Unknown2[9] & 0x10) == 0x10;

        var numParameters = Unknown2[7] >> 1;
        Parameters = new Guid[numParameters];
        for (int i = 0; i < numParameters; i++)
        {
            Parameters[i] = reader.ReadGuid();
        }

        Unknown3 = reader.ReadBytes(hasUnknownExtraField ? 4 : 2);
        Scale = reader.ReadSingle();
        Unknown4 = reader.ReadBytes(4);
        Cooldown = reader.ReadSingle();
        Unknown5 = reader.ReadToEnd();
    }

    protected override void WriteData(BinaryWriter writer)
    {
        writer.WriteStruct(EventId);
        writer.Write(Unknown1);
        writer.WriteStruct(TimelineId);
        writer.WriteStruct(MixerInputId);
        writer.WriteStruct(EventMixerMasterId);
        writer.Write(MaxInstances);
        writer.Write(Priority);
        writer.Write(Unknown2);

        foreach (var param in Parameters)
        {
            writer.WriteStruct(param);
        }
        
        writer.Write(Unknown3);
        writer.Write(Scale);
        writer.Write(Unknown4);
        writer.Write(Cooldown);
        writer.Write(Unknown5);
    }

    public override void ToTextWriter(TextWriter writer, int indent = 0)
    {
        writer.WriteIndented(indent + 1, "Event");
        writer.WriteIndented(indent + 1, $"Event ID: {EventId.ToKnownString()}");
        writer.WriteIndented(indent + 1, Convert.ToHexString(Unknown1));
        writer.WriteIndented(indent + 1, $"Timeline ID: {TimelineId.ToKnownString()}");
        writer.WriteIndented(indent + 1, $"Mixer Input ID: {MixerInputId.ToKnownString()}");
        writer.WriteIndented(indent + 1, $"Event Mixer Master ID: {EventMixerMasterId.ToKnownString()}");
        writer.WriteIndented(indent + 1, $"Max Instances: {MaxInstances}");
        writer.WriteIndented(indent + 1, $"Priority: {Priority}");
        writer.WriteIndented(indent + 1, Convert.ToHexString(Unknown2));

        foreach (var param in Parameters)
        {
            writer.WriteIndented(indent + 2, $"- Parameter ID: {param.ToKnownString()}");
        }
        
        writer.WriteIndented(indent + 1, Convert.ToHexString(Unknown3));
        writer.WriteIndented(indent + 1, $"Scale: {Scale}");
        writer.WriteIndented(indent + 1, Convert.ToHexString(Unknown4));
        writer.WriteIndented(indent + 1, $"Cooldown: {Cooldown}");
        writer.WriteIndented(indent + 1, Convert.ToHexString(Unknown5));
    }
}