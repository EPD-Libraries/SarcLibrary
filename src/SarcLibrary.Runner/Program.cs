#if RELEASE

using BenchmarkDotNet.Running;
using SarcLibrary.Runner.Benchmarks;

BenchmarkRunner.Run<SarcBenchmarks>();

#else

using SarcLibrary;

byte[] buffer = File.ReadAllBytes(args[0]);
Sarc sarc = Sarc.FromBinary(buffer);

using MemoryStream ms = sarc.OpenWrite("Test.txt");
ms.Write("Test"u8);

using FileStream fs = File.Create(args[1]);
sarc.Write(fs);

#endif