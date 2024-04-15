using Fmod5Sharp.FmodTypes;

namespace FmodParser;

public readonly struct SampleAnalysisResult
{
    public readonly FmodSample Sample;
    public readonly double? AverageChannelDeviation;

    public SampleAnalysisResult(FmodSample sample, double? averageChannelDeviation)
    {
        Sample = sample;
        AverageChannelDeviation = averageChannelDeviation;
    }

    public string Name => Sample.Name ?? "<empty>";
    public int SizeBytes => Sample.SampleBytes.Length;
    public double DurationSeconds => (double)Sample.SampleBytes.Length / 2 / Sample.Metadata.Channels / Sample.Metadata.Frequency;
}
