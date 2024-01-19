#if RELEASE

using BenchmarkDotNet.Running;
using SarcLibrary.Runner.Benchmarks;

BenchmarkRunner.Run<SarcBenchmarks>();

#else

using SarcLibrary;

byte[] buffer = File.ReadAllBytes(args[0]);
Sarc sarc = Sarc.FromBinary(buffer);

using FileStream fs = File.Create(args[1]);
sarc.Write(fs);

#endif