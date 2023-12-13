#if RELEASE

using BenchmarkDotNet.Running;
using SarcLibrary.Runner.Benchmarks;

BenchmarkRunner.Run<SarcBenchmarks>();

#else
using Revrs;
using SarcLibrary;

byte[] buffer = File.ReadAllBytes(args[0]);
RevrsReader reader = new(buffer);
ImmutableSarc immutableSarc = new(ref reader);
Sarc sarc = Sarc.FromImmutable(ref immutableSarc);

foreach ((var name, var data) in immutableSarc) {
    Console.WriteLine($"{name}: {data.Length}");
}

using FileStream fs = File.Create("D:\\bin\\Sarc\\Test.pack");
sarc.Write(fs);

#endif