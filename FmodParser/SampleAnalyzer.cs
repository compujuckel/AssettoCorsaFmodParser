using System.Runtime.InteropServices;
using Fmod5Sharp.FmodTypes;

namespace FmodParser;

public class SampleAnalyzer
{
    private readonly FmodSoundBank _bank;
    private readonly List<SampleAnalysisResult> _results;

    public SampleAnalyzer(FmodSoundBank bank)
    {
        _bank = bank;
        _results = AnalyzeSamples();
    }

    private List<SampleAnalysisResult> AnalyzeSamples()
    {
        var ret = new List<SampleAnalysisResult>();
        
        foreach (var sample in _bank.Samples)
        {
            var deviation = CalculateChannelDeviation(sample);
            
            ret.Add(new SampleAnalysisResult(sample, deviation));
        }

        return ret;
    }

    public void StereoToMono(double threshold)
    {
        int savingsTotal = 0;
        
        foreach (var result in _results)
        {
            if (result.AverageChannelDeviation < threshold)
            {
                Console.Write($"Converting {result.Sample.Name} to mono... ");
                
                var sizeBefore = result.SizeBytes;
                StereoToMono(result.Sample);
                var savings = sizeBefore - result.SizeBytes;
                savingsTotal += savings;
                
                Console.WriteLine($"done, saved {savings} bytes");
            }
        }
        
        Console.WriteLine($"Saved {savingsTotal / 1024:N0} KiB total");
    }

    public void PrintResults(TextWriter writer)
    {
        var nameWidth = _results.Max(r => r.Name.Length) + 1;
        
        writer.WriteLine($"{"Name".PadRight(nameWidth)} CH {"Size",6} {"Duration",7} {"Dev",3}");
        foreach (var result in _results.OrderByDescending(r => r.SizeBytes))
        {
            writer.Write($"{result.Name} ".PadRight(nameWidth, '.'));
            writer.Write($" {result.Sample.Metadata.Channels,2}");
            writer.Write($" {result.SizeBytes / 1024,6}");
            writer.Write($" {TimeSpan.FromSeconds(result.DurationSeconds),7:m':'ss'.'fff}");
            writer.Write($" {result.AverageChannelDeviation * 1000,3:N0}");
            writer.WriteLine();
        }
        writer.WriteLine();
        writer.WriteLine($"{"Field",-10} Explanation");
        writer.WriteLine($"{"CH",-10} No. of channels");
        writer.WriteLine($"{"Size",-10} Size in KiB");
        writer.WriteLine($"{"Dev",-10} Deviation between left/right channels");
        writer.WriteLine();
    }

    private double? CalculateChannelDeviation(FmodSample sample)
    {
        if (sample.Metadata.Channels <= 1) return null;
        
        var avgDeviation = 0.0;
        var sampleShorts = MemoryMarshal.Cast<byte, short>(sample.SampleBytes.Span);
        for (int i = 0; i < sampleShorts.Length; i += 2)
        {
            var left = sampleShorts[i];
            var right = sampleShorts[i + 1];

            var deviation = Math.Abs(left - right);
            avgDeviation += deviation;
        }

        return avgDeviation / short.MaxValue / sampleShorts.Length;
    }

    private static void StereoToMono(FmodSample sample)
    {
        var buf = new byte[sample.SampleBytes.Length / 2];
        
        var inShorts = MemoryMarshal.Cast<byte, short>(sample.SampleBytes.Span);
        var outShorts = MemoryMarshal.Cast<byte, short>(buf.AsSpan());

        int j = 0;
        for (int i = 0; i < inShorts.Length; i += 2)
        {
            outShorts[j++] = (short)((inShorts[i] + inShorts[i+1]) / 2);
        }

        sample.SampleBytes = buf;
    }
}