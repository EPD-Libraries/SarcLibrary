using BenchmarkDotNet.Attributes;
using Revrs;

namespace SarcLibrary.Runner.Benchmarks;

[MemoryDiagnoser(true)]
public class ImmutableSarcBenchmarks
{
    private readonly byte[] _bufferLE = File.ReadAllBytes(@"D:\bin\Sarc\TitleBG-LE.pack");
    private readonly byte[] _bufferBE = File.ReadAllBytes(@"D:\bin\Sarc\TitleBG-BE.pack");

    [Benchmark]
    public void Read_LE()
    {
        RevrsReader reader = new(_bufferLE);
        ImmutableSarc sarc = new(ref reader);
    }

    [Benchmark]
    public void Read_BE()
    {
        RevrsReader reader = new(_bufferBE);
        ImmutableSarc sarc = new(ref reader);
    }
}
