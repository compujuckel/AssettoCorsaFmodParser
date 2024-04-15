using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using CommandLine;
using Fmod5Sharp.FmodTypes;
using JetBrains.Annotations;

namespace FmodParser;

public static class Program
{
    [UsedImplicitly(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.WithMembers)]
    public class Options
    {
        [Value(0, MetaName = "Input File", HelpText = "Input file (*.bank)", Required = true)]
        public required string InputFile { get; init; }
        
        [Option('r', "replace", HelpText = "Replace audio files in this soundbank.")]
        public bool Replace { get; init;  }
        
        [Option('d', "downsample", HelpText = "Downsample stereo to mono if audio channels match")]
        public bool Downsample { get; init; }

        [Option("downsample-threshold", HelpText = "Maximum deviation between left/right channels required for downsampling", Default = 100)]
        public int DownsampleThreshold { get; init; }
    }
    
    public static void Main(string[] args)
    {
        var options = Parser.Default.ParseArguments<Options>(args).Value;
        if (options == null) return;
        
        var inputFileName = Path.GetFileName(options.InputFile);

        if (File.Exists("GUIDs.txt"))
        {
            GuidCache.LoadFile("GUIDs.txt");
        }
        
        var fmodFile = RiffParser.Parse(options.InputFile);
        var nameNoExtension = Path.GetFileNameWithoutExtension(inputFileName);
        var bank = fmodFile.FindSoundBank();
        
        if (options.Replace)
        {
            foreach (var sample in bank.Samples)
            {
                var samplePath = Path.Combine(nameNoExtension, $"{sample.Name}.wav");
                if (File.Exists(samplePath))
                {
                    sample.ReplaceAudio(samplePath);
                }
            }
        }

        if (options.Downsample)
        {
            AnalyzeSamples(bank, options.DownsampleThreshold);
        }
        
        if (!options.Replace)
        {
            Directory.CreateDirectory(nameNoExtension);
            foreach (var sample in bank.Samples)
            {
                if (!sample.RebuildAsStandardFileFormat(out var data, out var extension))
                {
                    throw new Exception($"Failed to extract sample {sample.Name}");
                }

                var samplePath = Path.Combine(nameNoExtension, $"{sample.Name}.{extension}");
                File.WriteAllBytes(samplePath, data);
            }
        }

        if (options.Replace || options.Downsample)
        {
            fmodFile.ToFile($"{nameNoExtension}_out.bank");
        }

        using var outFile = File.CreateText($"{inputFileName}.txt");
        RiffParser.Print(outFile, fmodFile.Root);
    }

    public static void AnalyzeSamples(FmodSoundBank bank, int downsampleThreshold)
    {
        int savings = 0;
        
        foreach (var sample in bank.Samples)
        {
            FindRepetitionsSimple(sample);
            
            if (sample.Metadata.Channels == 1)
            {
                Console.WriteLine($"{sample.Name.PadRight(40)} - Mono");
            }
            else
            {
                var avgDeviation = 0.0;
                var sampleShorts = MemoryMarshal.Cast<byte, short>(sample.SampleBytes.Span);
                for (int i = 0; i < sampleShorts.Length; i += 2)
                {
                    var left = sampleShorts[i];
                    var right = sampleShorts[i + 1];

                    var deviation = Math.Abs(left - right);
                    avgDeviation += deviation;
                }

                avgDeviation /= sampleShorts.Length;

                if (avgDeviation < downsampleThreshold)
                {
                    savings += sample.SampleBytes.Length / 2;
                    Downsample(sample);
                }
                
                Console.WriteLine($"{sample.Name.PadRight(40)} - Stereo - {Math.Round(avgDeviation, 1)}");
            }
        }
        
        Console.WriteLine($"Total savings: {savings} bytes");
    }

    private static Vector512<sbyte> NarrowScaled(Span<short> input, int scale = 10)
    {
        int count = Vector512<short>.Count;
        var lower = Vector512.ShiftRightArithmetic(Vector512.Create<short>(input), scale);
        var upper = Vector512.ShiftRightArithmetic(Vector512.Create<short>(input[count..]), scale);
        return Vector512.Narrow(lower, upper);
    }

    private static readonly int ShortVecLength = Vector512<short>.Count;
    private static readonly int ByteVecLength = Vector512<sbyte>.Count;

    private static bool WindowMatches(Span<short> samples, int firstIndex, int secondIndex)
    {
        var firstVector = NarrowScaled(samples.Slice(firstIndex));
        var secondVector = NarrowScaled(samples.Slice(secondIndex));
        return firstVector == secondVector;
    }

    public static void FindRepetitionsSimple(FmodSample sample)
    {
        var samples = MemoryMarshal.Cast<byte, short>(sample.SampleBytes.Span);
        
        int vecLength = Vector512<short>.Count;
        int searchStart = (int)(3 * sample.Metadata.Frequency * sample.Metadata.Channels);
        double previousHit = searchStart;

        if (samples.Length < searchStart + vecLength) return;

        //var searchPatternBytes = NarrowScaled(samples.Slice(searchStart, vecLength * 2));

        for (int i = searchStart + vecLength; i < samples.Length - vecLength * 2; i++)
        {
            //var current = NarrowScaled(samples.Slice(i, vecLength * 2));

            int count = 0;
            while (WindowMatches(samples, searchStart, i))
            {
                count++;
                //var posSeconds1 = previousHit / sample.Metadata.Channels / sample.Metadata.Frequency;
                //var posSeconds2 = (double)i / sample.Metadata.Channels / sample.Metadata.Frequency;
                //var duration = posSeconds2 - posSeconds1;
                //Console.WriteLine($"********* Found repeating pattern {Math.Round(posSeconds1, 3)} - {Math.Round(posSeconds2, 3)} Duration {Math.Round(duration, 3)}s");
                //previousHit = i;
                
                Console.WriteLine($"HIT {count}: {searchStart} - {i}");

                searchStart += ShortVecLength;
                i += ShortVecLength;
            }
        }
    }

    public static void Downsample(FmodSample sample)
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
