using BenchmarkDotNet.Attributes;

namespace SarcLibrary.Runner.Benchmarks;

[MemoryDiagnoser(true)]
public class SarcBenchmarks
{
    private readonly byte[] _bufferLE = File.ReadAllBytes(@"D:\bin\Sarc\TitleBG-LE.pack");
    private readonly byte[] _bufferBE = File.ReadAllBytes(@"D:\bin\Sarc\TitleBG-BE.pack");
    private readonly Sarc _sarcLE = Sarc.FromBinary(File.ReadAllBytes(@"D:\bin\Sarc\TitleBG-LE.pack"));
    private readonly Sarc _sarcBE = Sarc.FromBinary(File.ReadAllBytes(@"D:\bin\Sarc\TitleBG-BE.pack"));

    private readonly MemoryStream _sarcMsLE = new();
    private readonly MemoryStream _sarcMsBE = new();

    [Benchmark]
    public void Read_LE()
    {
        Sarc _ = Sarc.FromBinary(_bufferLE);
    }

    [Benchmark]
    public void Read_BE()
    {
        Sarc _ = Sarc.FromBinary(_bufferBE);
    }

    [Benchmark]
    public void Write_LE()
    {
        _sarcLE.Write(_sarcMsLE);
    }

    [Benchmark]
    public void Write_BE()
    {
        _sarcBE.Write(_sarcMsBE);
    }
}
