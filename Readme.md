# Sarc Library

Modern **S**EAD/**S**EPD **arc**hive reader written in managed C#

## Usage

### Reading a Sarc File

```cs
byte[] data = File.ReadAllBytes("content/Pack/Bootup.pack");
Sarc sarc = Sarc.FromBinary(data);
```

### Writing a Sarc File

```cs
/* ... */

using MemoryStream ms = new();
sarc.Write(ms);
```

## Benchmarks

| Function                                | Elapsed  | Allocated |
|:----------------------------------------|:--------:|:---------:|
| Read TitleBG (143MB, LE)                | 24.27 ms |    140 MB |
| Read TitleBG (75MB, BE)                 | 12.06 ms |     74 MB |
|                                         |          |           |
| Write TitleBG (143MB, LE)               | 12.43 ms |     66 KB |
| Write TitleBG (75MB, BE)                |  6.41 ms |     66 KB |
|                                         |          |           |
| Read TitleBG (Immutable) (143MB, LE)    | 16.61 ns |         - |
| Read TitleBG (Immutable) (75MB, BE)     | 16.27 ns |         - |

### Improvments Over Last Version

| Function                                | Speed Increased |
|:----------------------------------------|----------------:|
| Read TitleBG (143MB, LE)                |            2.1% |
| Read TitleBG (75MB, BE)                 |            2.4% |
|                                         |                 |
| Write TitleBG (143MB, LE)               |           10.3% |
| Write TitleBG (75MB, BE)                |            6.4% |
|                                         |                 |
| Read TitleBG (Immutable) (143MB, LE)    |    3,071,643.6% |
| Read TitleBG (Immutable) (75MB, BE)     |    2,417,910.5% |

### Install

[![NuGet](https://img.shields.io/nuget/v/SarcLibrary.svg)](https://www.nuget.org/packages/SarcLibrary) [![NuGet](https://img.shields.io/nuget/dt/SarcLibrary.svg)](https://www.nuget.org/packages/SarcLibrary)

#### NuGet
```powershell
Install-Package SarcLibrary
```

#### Build From Source
```batch
git clone https://github.com/EPD-Libraries/SarcLibrary.git
dotnet build SarcLibrary
```

Special thanks to **[Léo Lam](https://github.com/leoetlino)** for his extensive research on EPD file formats.
