using BenchmarkDotNet.Attributes;

namespace SarcLibrary.Runner.Benchmarks;

[MemoryDiagnoser(true)]
public class SarcBenchmarks
{
    private readonly byte[] _bufferLE = File.ReadAllBytes(@"D:\bin\Sarc\TitleBG-LE.pack");
    private readonly byte[] _bufferBE = File.ReadAllBytes(@"D:\bin\Sarc\TitleBG-BE.pack");

    [Benchmark]
    public void Read_LE()
    {
        Sarc sarc = Sarc.FromBinary(_bufferLE);
    }

    [Benchmark]
    public void Read_BE()
    {
        Sarc sarc = Sarc.FromBinary(_bufferBE);
    }
}
