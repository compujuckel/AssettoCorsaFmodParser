using System.CommandLine;
using System.Diagnostics;

namespace FmodParser;

public static class Program
{
    public const string Description = """
                                      Assetto Corsa FMOD Analyzer

                                      By default this tool will extract all samples from a sound bank and write three files:
                                      <filename>_structure.txt: A file outlining the contents of the FMOD sound bank
                                      <filename>_samples.txt: A file showing some info about included sound samples
                                      <filename>_out.bank: The sound bank with sounds downsampled to mono according to the downsample-threshold parameter
                                      """;
    
    public static void Main(string[] args)
    {
        if (!Debugger.IsAttached)
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        }
        
        var inputFileArgument = new Argument<FileInfo>("input-file", "The input sound bank (*.bank)");
        var downsampleThresholdOption = new Option<int>(aliases: ["-d", "--downsample-threshold"], 
            description: "Maximum deviation between left/right channels required for downsampling from stereo to mono",
            getDefaultValue: () => 5);
        var replaceOption = new Option<bool>(["-r", "--replace"],
            "Instead of extracting a sound bank, replace all files in the sound bank with files from a folder with the same name");

        var rootCommand = new RootCommand(Description);
        rootCommand.Add(inputFileArgument);
        rootCommand.Add(downsampleThresholdOption);
        rootCommand.Add(replaceOption);

        rootCommand.SetHandler(ActualMain, inputFileArgument, downsampleThresholdOption, replaceOption);
        rootCommand.Invoke(args);
    }

    public static void ActualMain(FileInfo inputFile, int downsampleThresholdInt, bool replace)
    {
        var downsampleThreshold = downsampleThresholdInt / 1000.0;
        var nameNoExtension = Path.GetFileNameWithoutExtension(inputFile.Name);

        if (File.Exists("GUIDs.txt"))
        {
            GuidCache.LoadFile("GUIDs.txt");
        }

        var fmodFile = RiffParser.Parse(inputFile.FullName);
        var bank = fmodFile.FindSoundBank();

        if (replace)
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

        analyzer.StereoToMono(downsampleThreshold);

        if (!replace)
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
    
        fmodFile.ToFile($"{nameNoExtension}_out.bank");

        using var outFile = File.CreateText($"{nameNoExtension}_structure.txt");
        RiffParser.Print(outFile, fmodFile.Root);
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Console.WriteLine(((Exception)e.ExceptionObject).ToString());
        Console.ReadKey();
        Environment.Exit(1);
    }
}
