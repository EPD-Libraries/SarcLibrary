#if RELEASE

using BenchmarkDotNet.Running;
using SarcLibrary.Runner.Benchmarks;

BenchmarkRunner.Run<SarcBenchmarks>();

#else

using SarcLibrary;

FileStream fs = File.OpenRead(args[0]);
SarcTools.JumpToEntry(fs, "Event/ResidentEvent.byml", out int size);

using FileStream fsout = File.Create("D:\\bin\\Sarc\\output.byml");
fs.CopyTo(fsout, size);

#endif