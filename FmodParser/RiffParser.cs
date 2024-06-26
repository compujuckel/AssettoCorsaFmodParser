﻿using System.Diagnostics.CodeAnalysis;
using System.IO.Hashing;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using DotNext.IO.MemoryMappedFiles;
using Fmod5Sharp;
using FmodParser.Riff;
using FmodParser.Utils;

namespace FmodParser;

public static class RiffParser
{
    private static readonly ChunkFactory ChunkFactory = new();
    
    public static FmodFile Parse(string filename)
    {
        var file = MemoryMappedFile.CreateFromFile(filename, FileMode.Open);
        var accessor = file.CreateMemoryAccessor(0, 0, MemoryMappedFileAccess.Read);

        var result = ParseChunk(accessor.Memory, out _, out var chunk);
        return new FmodFile(file, accessor, (ListChunk)chunk!);
    }

    public static bool ParseChunk(Memory<byte> data, out int bytesRead, [NotNullWhen(true)] out RiffChunkBase? chunk)
    {
        var identifier = data[..4];
        var length = MemoryMarshal.Read<int>(data[4..8].Span);
        var content = data[8..(length + 8)];

        if (identifier.Span.SequenceEqual("RIFF"u8) || identifier.Span.SequenceEqual("LIST"u8))
        {
            var subIdentifier = content[..4];
            var children = new List<RiffChunkBase>();

            var subData = content[4..];
            var subBytesRead = 0;
            while (subBytesRead < content.Length - 4 && ParseChunk(subData, out var outBytesRead, out var outChunk))
            {
                children.Add(outChunk);
                subData = subData[outBytesRead..];
                subBytesRead += outBytesRead;
            }

            chunk = new ListChunk
            {
                Identifier = identifier,
                ListIdentifier = subIdentifier,
                Length = length,
                SubChunks = children
            };
        }
        else
        {
            chunk = ChunkFactory.GetDataChunk(identifier, length, content);
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
            if (data.Identifier.Span.SequenceEqual("BNKI"u8))
            {
                WriteIndented(writer, indentation + 1, "Bank ??");
                var guid = new Guid(data.Data[..16].Span);
                WriteIndented(writer, indentation + 1, $"Bank ID: {guid.ToKnownString()}");
                WriteIndented(writer, indentation + 1, Convert.ToHexString(data.Data[16..].Span));
            }
            else if (data.Identifier.Span.SequenceEqual("IBSB"u8))
            {
                int off = 0;
                int len = 16;
                
                WriteIndented(writer, indentation + 1, "Mixer Input ??");
                
                var guid = new Guid(data.Data.Slice(off, len).Span);
                WriteIndented(writer, indentation + 1, $"Mixer Input ID: {guid.ToKnownString()}");
                off += len;

                len = 2;
                WriteIndented(writer, indentation + 1, Convert.ToHexString(data.Data.Slice(off, len).Span));
                off += len;

                len = 16;
                guid = new Guid(data.Data.Slice(off, len).Span);
                WriteIndented(writer, indentation + 1, $"Output Bus ID: {guid.ToKnownString()}");
                off += len;
                
                WriteIndented(writer, indentation + 1, Convert.ToHexString(data.Data.Slice(off).Span));
            }
            else if (data.Identifier.Span.SequenceEqual("RBSB"u8))
            {
                int off = 0;
                int len = 16;
                
                WriteIndented(writer, indentation + 1, "Mixer Return ??");
                
                var guid = new Guid(data.Data.Slice(off, len).Span);
                WriteIndented(writer, indentation + 1, $"Mixer Return ID: {guid.ToKnownString()}");
                off += len;

                len = 2;
                WriteIndented(writer, indentation + 1, Convert.ToHexString(data.Data.Slice(off, len).Span));
                off += len;

                len = 16;
                guid = new Guid(data.Data.Slice(off, len).Span);
                WriteIndented(writer, indentation + 1, $"Output Bus ID: {guid.ToKnownString()}");
                off += len;
                
                WriteIndented(writer, indentation + 1, Convert.ToHexString(data.Data.Slice(off).Span));
            }
            else if (data.Identifier.Span.SequenceEqual("GBSB"u8))
            {
                int off = 0;
                int len = 16;
                
                WriteIndented(writer, indentation + 1, "Event Mixer Group ??");
                
                var guid = new Guid(data.Data.Slice(off, len).Span);
                WriteIndented(writer, indentation + 1, $"Event Mixer Group ID: {guid.ToKnownString()}");
                off += len;

                len = 2;
                WriteIndented(writer, indentation + 1, Convert.ToHexString(data.Data.Slice(off, len).Span));
                off += len;

                len = 16;
                guid = new Guid(data.Data.Slice(off, len).Span);
                WriteIndented(writer, indentation + 1, $"Output Bus ID: {guid.ToKnownString()}");
                off += len;
                
                WriteIndented(writer, indentation + 1, Convert.ToHexString(data.Data.Slice(off).Span));
            }
            else if (data.Identifier.Span.SequenceEqual("BEFB"u8))
            {
                int off = 0;
                int len = 16;
                
                WriteIndented(writer, indentation + 1, "Effect ??");
                
                var guid = new Guid(data.Data.Slice(off, len).Span);
                WriteIndented(writer, indentation + 1, $"Effect ID: {guid.ToKnownString()}");
                off += len;
                
                WriteIndented(writer, indentation + 1, Convert.ToHexString(data.Data.Slice(off).Span));
            }
            else if (data.Identifier.Span.SequenceEqual("INST"u8))
            {
                int off = 0;
                int len = 16;
                
                WriteIndented(writer, indentation + 1, "INST ??");
                
                var guid = new Guid(data.Data.Slice(off, len).Span);
                WriteIndented(writer, indentation + 1, $"Timeline ID: {guid.ToKnownString()}");
                off += len;
                
                WriteIndented(writer, indentation + 1, Convert.ToHexString(data.Data.Slice(off).Span));
            }
            else if (data.Identifier.Span.SequenceEqual("SND "u8))
            {
                WriteIndented(writer, indentation + 1, "[omitted]");
            }
            else
            {
                data.ToTextWriter(writer, indentation + 1);
            }
        }
        else
        {
            WriteIndented(writer, indentation, $"[{IdentToStr(chunk.Identifier)}] len={chunk.Length}");
            chunk.ToTextWriter(writer, indentation + 1);
        }
    }

    public static void WriteIndented(TextWriter writer, int indent, string str)
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