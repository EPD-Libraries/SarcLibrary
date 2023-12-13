using Revrs;
using SarcLibrary.Structures;
using System.Runtime.CompilerServices;

namespace SarcLibrary.Readers;

public readonly ref struct SfntReader
{
    public readonly SfntHeader Header;
    public readonly Span<byte> RawNameData;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SfntReader(ref RevrsReader reader, int dataOffset)
    {
        Header = reader.Read<SfntHeader, SfntHeader.Reverser>();
        if (Header.Magic != Sarc.SFNT_MAGIC) {
            throw new InvalidDataException("Invalid SFNT header!");
        }

        RawNameData = reader.Data[reader.Position..dataOffset];
    }
}
