using System.Diagnostics.CodeAnalysis;
using System.IO.Hashing;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Text;
using DotNext.IO.MemoryMappedFiles;
using Fmod5Sharp;

namespace FmodParser;

public static class RiffParser
{
    public static RiffChunkBase? Parse(string filename)
    {
        using var file = MemoryMappedFile.CreateFromFile(filename, FileMode.Open);
        var accessor = file.CreateMemoryAccessor(0, 0, MemoryMappedFileAccess.Read);

        var result = ParseChunk(accessor.Memory, out _, out var chunk);
        return chunk;
    }

    public static bool ParseChunk(Memory<byte> data, out int bytesRead, [NotNullWhen(true)] out RiffChunkBase? chunk)
    {
        var identifier = data[..4];
        var length = MemoryMarshal.Read<int>(data[4..8].Span);
        var content = data[8..(length + 8)];
        
        Console.WriteLine($"Chunk {Encoding.ASCII.GetString(identifier.Span)}, length {length}");

        if (identifier.Span.SequenceEqual("RIFF"u8) || identifier.Span.SequenceEqual("LIST"u8))
        {
            var subIdentifier = content[..4];
            var children = new List<RiffChunkBase>();
            
            Console.WriteLine($"Sub ident {Encoding.ASCII.GetString(subIdentifier.Span)}");

            var subData = content[4..];
            var subBytesRead = 0;
            while (subBytesRead < content.Length - 4 && ParseChunk(subData, out var outBytesRead, out var outChunk))
            {
                Console.WriteLine($"before subBytesRead {subBytesRead} / {content.Length}");
                children.Add(outChunk);
                subData = subData[outBytesRead..];
                subBytesRead += outBytesRead;
                Console.WriteLine($"after subBytesRead {subBytesRead} / {content.Length}");
            }

            chunk = new ListChunk
            {
                Identifier = identifier,
                ListIdentifier = subIdentifier,
                Length = length,
                SubChunks = children
            };
            
            Console.WriteLine("End list");
        }
        else
        {
            chunk = new DataChunk
            {
                Identifier = identifier,
                Length = length,
                Data = content
            };
        }

        bytesRead = 8 + length + (length % 2 == 1 ? 1 : 0);
        return true;
    }

    public static void Print(TextWriter writer, RiffChunkBase chunk, int indentation = 0)
    {
        if (chunk is ListChunk list)
        {
            WriteIndented(writer, indentation, $"[{IdentToStr(list.Identifier)}] ({IdentToStr(list.ListIdentifier)}) count={list.SubChunks.Count} len={list.Length}");
            
            if (list.ListIdentifier.Span.SequenceEqual("BEFX"u8))
            {
                WriteIndented(writer, indentation + 1, "Effects");
            }
            else if (list.ListIdentifier.Span.SequenceEqual("IBSS"u8))
            {
                WriteIndented(writer, indentation + 1, "Input Busses");
            }
            else if (list.ListIdentifier.Span.SequenceEqual("GBSS"u8))
            {
                WriteIndented(writer, indentation + 1, "Group Busses");
            }
            else if (list.ListIdentifier.Span.SequenceEqual("RBSS"u8))
            {
                WriteIndented(writer, indentation + 1, "Return Busses");
            }
            else if (list.ListIdentifier.Span.SequenceEqual("MBSS"u8))
            {
                WriteIndented(writer, indentation + 1, "Master Busses");
            }
            else if (list.ListIdentifier.Span.SequenceEqual("EVTS"u8))
            {
                WriteIndented(writer, indentation + 1, "Events");
            }
            
            foreach (var subChunk in list.SubChunks)
            {
                Print(writer, subChunk, indentation + 1);
            }
        }
        else if (chunk is DataChunk data)
        {
            WriteIndented(writer, indentation, $"[{IdentToStr(data.Identifier)}] len={data.Length}");
            if (data.Identifier.Span.SequenceEqual("LCNT"u8))
            {
                WriteIndented(writer, indentation + 1, $"List count: {MemoryMarshal.Read<int>(data.Data.Span)}");
            }
            else if (data.Identifier.Span.SequenceEqual("MBSB"u8))
            {
                WriteIndented(writer, indentation + 1, "Mixer Bus SB ??");
                var guid = new Guid(data.Data[..16].Span);
                WriteIndented(writer, indentation + 1, $"Bus ID: {guid.ToString()}");
                WriteIndented(writer, indentation + 1, Convert.ToHexString(data.Data[16..].Span));
            }
            else if (data.Identifier.Span.SequenceEqual("BNKI"u8))
            {
                WriteIndented(writer, indentation + 1, "Bank ??");
                var guid = new Guid(data.Data[..16].Span);
                WriteIndented(writer, indentation + 1, $"Bank ID: {guid.ToString()}");
                WriteIndented(writer, indentation + 1, Convert.ToHexString(data.Data[16..].Span));
            }
            else if (data.Identifier.Span.SequenceEqual("EVTB"u8))
            {
                WriteIndented(writer, indentation + 1, "Event ??");
                var guid = new Guid(data.Data[..16].Span);
                WriteIndented(writer, indentation + 1, $"Event ID: {guid.ToString()}");
                WriteIndented(writer, indentation + 1, Convert.ToHexString(data.Data.Slice(16, 16).Span));
                int tlidOff = 16 * 2;
                guid = new Guid(data.Data.Slice(tlidOff, 16).Span);
                WriteIndented(writer, indentation + 1, $"Timeline ID: {guid.ToString()}");

                int mixerInputOff = 16 * 3;
                guid = new Guid(data.Data.Slice(mixerInputOff, 16).Span);
                WriteIndented(writer, indentation + 1, $"Mixer Input ID: {guid.ToString()}");
                
                int eventMixerMasterOff = 16 * 4;
                guid = new Guid(data.Data.Slice(eventMixerMasterOff, 16).Span);
                WriteIndented(writer, indentation + 1, $"Event Mixer Master ID: {guid.ToString()}");

                WriteIndented(writer, indentation + 1, Convert.ToHexString(data.Data.Slice(eventMixerMasterOff + 16).Span));
            }
            else if (data.Identifier.Span.SequenceEqual("TLNB"u8))
            {
                int off = 0;
                int len = 16;
                
                WriteIndented(writer, indentation + 1, "Timeline ??");
                var guid = new Guid(data.Data.Slice(off, len).Span);
                WriteIndented(writer, indentation + 1, $"Timeline ID: {guid.ToString()}");
                off += len;
                
                guid = new Guid(data.Data.Slice(off, 16).Span);
                WriteIndented(writer, indentation + 1, $"Event ID: {guid.ToString()}");
                off += len;
                
                WriteIndented(writer, indentation + 1, Convert.ToHexString(data.Data.Slice(off).Span));
            }
            else if (data.Identifier.Span.SequenceEqual("IBSB"u8))
            {
                int off = 0;
                int len = 16;
                
                WriteIndented(writer, indentation + 1, "Mixer Input ??");
                
                var guid = new Guid(data.Data.Slice(off, len).Span);
                WriteIndented(writer, indentation + 1, $"Mixer Input ID: {guid.ToString()}");
                off += len;

                len = 2;
                WriteIndented(writer, indentation + 1, Convert.ToHexString(data.Data.Slice(off, len).Span));
                off += len;

                len = 16;
                guid = new Guid(data.Data.Slice(off, len).Span);
                WriteIndented(writer, indentation + 1, $"Output Bus ID: {guid.ToString()}");
                off += len;
                
                WriteIndented(writer, indentation + 1, Convert.ToHexString(data.Data.Slice(off).Span));
            }
            else if (data.Identifier.Span.SequenceEqual("RBSB"u8))
            {
                int off = 0;
                int len = 16;
                
                WriteIndented(writer, indentation + 1, "Mixer Return ??");
                
                var guid = new Guid(data.Data.Slice(off, len).Span);
                WriteIndented(writer, indentation + 1, $"Mixer Return ID: {guid.ToString()}");
                off += len;

                len = 2;
                WriteIndented(writer, indentation + 1, Convert.ToHexString(data.Data.Slice(off, len).Span));
                off += len;

                len = 16;
                guid = new Guid(data.Data.Slice(off, len).Span);
                WriteIndented(writer, indentation + 1, $"Output Bus ID: {guid.ToString()}");
                off += len;
                
                WriteIndented(writer, indentation + 1, Convert.ToHexString(data.Data.Slice(off).Span));
            }
            else if (data.Identifier.Span.SequenceEqual("GBSB"u8))
            {
                int off = 0;
                int len = 16;
                
                WriteIndented(writer, indentation + 1, "Event Mixer Group ??");
                
                var guid = new Guid(data.Data.Slice(off, len).Span);
                WriteIndented(writer, indentation + 1, $"Event Mixer Group ID: {guid.ToString()}");
                off += len;

                len = 2;
                WriteIndented(writer, indentation + 1, Convert.ToHexString(data.Data.Slice(off, len).Span));
                off += len;

                len = 16;
                guid = new Guid(data.Data.Slice(off, len).Span);
                WriteIndented(writer, indentation + 1, $"Output Bus ID: {guid.ToString()}");
                off += len;
                
                WriteIndented(writer, indentation + 1, Convert.ToHexString(data.Data.Slice(off).Span));
            }
            else if (data.Identifier.Span.SequenceEqual("BEFB"u8))
            {
                int off = 0;
                int len = 16;
                
                WriteIndented(writer, indentation + 1, "Effect ??");
                
                var guid = new Guid(data.Data.Slice(off, len).Span);
                WriteIndented(writer, indentation + 1, $"Effect ID: {guid.ToString()}");
                off += len;
                
                WriteIndented(writer, indentation + 1, Convert.ToHexString(data.Data.Slice(off).Span));
            }
            else if (data.Identifier.Span.SequenceEqual("INST"u8))
            {
                int off = 0;
                int len = 16;
                
                WriteIndented(writer, indentation + 1, "INST ??");
                
                var guid = new Guid(data.Data.Slice(off, len).Span);
                WriteIndented(writer, indentation + 1, $"Timeline ID: {guid.ToString()}");
                off += len;
                
                WriteIndented(writer, indentation + 1, Convert.ToHexString(data.Data.Slice(off).Span));
            }
            else if (data.Identifier.Span.SequenceEqual("WAV "u8))
            {
                int off = 0;
                int len = 16;
                
                WriteIndented(writer, indentation + 1, "Audio File");
                
                var guid = new Guid(data.Data.Slice(off, len).Span);
                WriteIndented(writer, indentation + 1, $"Audio File ID: {guid.ToString()}");
                off += len;
                
                WriteIndented(writer, indentation + 1, Convert.ToHexString(data.Data.Slice(off).Span));
            }
            else if (data.Identifier.Span.SequenceEqual("WAIB"u8))
            {
                int off = 0;
                int len = 16;
                
                WriteIndented(writer, indentation + 1, "Single Sound");
                
                var guid = new Guid(data.Data.Slice(off, len).Span);
                WriteIndented(writer, indentation + 1, $"Single Sound ID: {guid.ToString()}");
                off += len;
                
                guid = new Guid(data.Data.Slice(off, len).Span);
                WriteIndented(writer, indentation + 1, $"Audio File ID: {guid.ToString()}");
                off += len;
            }
            else if (data.Identifier.Span.SequenceEqual("SND "u8))
            {
                WriteIndented(writer, indentation + 1, "[omitted]");
                var hash = Convert.ToHexString(XxHash3.Hash(data.Data.Span));
                var bytes = data.Data.Span.TrimStart(stackalloc byte[] { 0 }).ToArray();
                File.WriteAllBytes($"{hash}.fsb", bytes);
                Directory.CreateDirectory(hash);
                
                var fmod = FsbLoader.LoadFsbFromByteArray(bytes);
                WriteIndented(writer, indentation + 1, $"No. of samples: {fmod.Samples.Count}");

                var i = 0;
                foreach (var sample in fmod.Samples)
                {
                    WriteIndented(writer, indentation + 1, $"Sample size {sample.SampleBytes.Length} bytes");
                    
                    if(!sample.RebuildAsStandardFileFormat(out var sampleData, out var extension))
                    {
                        Console.WriteLine($"Failed to extract sample {i}");
                        continue;
                    }
            
                    var filePath = Path.Combine(hash, $"{i:X}_{sample.Name ?? i.ToString()}.{extension}");
                    File.WriteAllBytes(filePath, sampleData);

                    i++;
                }
            }
            else
            {
                WriteIndented(writer, indentation + 1, Convert.ToHexString(data.Data.Span));
            }
        }
    }

    private static void WriteIndented(TextWriter writer, int indent, string str)
    {
        for (int i = 0; i < indent; i++)
        {
            writer.Write("    ");
        }

        writer.WriteLine(str);
    }

    private static string IdentToStr(Memory<byte> ident)
    {
        return Encoding.ASCII.GetString(ident.Span);
    }
}