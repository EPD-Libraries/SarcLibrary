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

using MemoryStream ms = new();
sarc.Write(ms);

#endif