using BenchmarkDotNet.Attributes;

namespace SarcLibrary.Runner.Benchmarks;

[MemoryDiagnoser(true)]
public class SarcBenchmarks
{
    private readonly byte[] _bufferLE = File.ReadAllBytes(@"D:\bin\Sarc\TitleBG-LE.pack");
    private readonly byte[] _bufferBE = File.ReadAllBytes(@"D:\bin\Sarc\TitleBG-BE.pack");
    private readonly Sarc _sarcLE = Sarc.FromBinary(File.ReadAllBytes(@"D:\bin\Sarc\TitleBG-LE.pack"));
    private readonly Sarc _sarcBE = Sarc.FromBinary(File.ReadAllBytes(@"D:\bin\Sarc\TitleBG-BE.pack"));

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

    [Benchmark]
    public void Write_LE()
    {
        using MemoryStream ms = new();
        _sarcLE.Write(ms);
    }

    [Benchmark]
    public void Write_BE()
    {
        using MemoryStream ms = new();
        _sarcBE.Write(ms);
    }
}
