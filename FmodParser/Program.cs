using CommandLine;
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

        [Option("downsample-threshold", HelpText = "Maximum deviation between left/right channels required for downsampling", Default = 5)]
        public int DownsampleThreshold { get; init; }

        public double DownsampleThresholdPerMille => DownsampleThreshold / 1000.0;
    }
    
    public static void Main(string[] args)
    {
        var options = Parser.Default.ParseArguments<Options>(args).Value;
        if (options == null) return;
        
        var inputFileName = Path.GetFileName(options.InputFile);
        var nameNoExtension = Path.GetFileNameWithoutExtension(inputFileName);

        if (File.Exists("GUIDs.txt"))
        {
            GuidCache.LoadFile("GUIDs.txt");
        }
        
        var fmodFile = RiffParser.Parse(options.InputFile);
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

        var analyzer = new SampleAnalyzer(bank);
        using (var log = File.CreateText($"{nameNoExtension}_samples.txt"))
        {
            analyzer.PrintResults(log);
        }

        if (options.Downsample)
        {
            analyzer.StereoToMono(options.DownsampleThresholdPerMille);
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

        using var outFile = File.CreateText($"{nameNoExtension}_structure.txt");
        RiffParser.Print(outFile, fmodFile.Root);
    }
}
