using Revrs;
using System.Runtime.InteropServices;

namespace SarcLibrary.Structures;

[StructLayout(LayoutKind.Explicit, Pack = 2, Size = 0x0C)]
public struct SfatHeader
{
    [FieldOffset(0x00)]
    public uint Magic;

    [FieldOffset(0x04)]
    public ushort HeaderSize;

    [FieldOffset(0x06)]
    public ushort NodeCount;

    [FieldOffset(0x08)]
    public uint HashKey;

    public class Reverser : IStructReverser
    {
        public static void Reverse(in Span<byte> slice)
        {
            // HeaderSize
            slice[0x04..0x06].Reverse();

            // NodeCount
            slice[0x06..0x08].Reverse();

            // HashKey
            slice[0x08..0x0C].Reverse();
        }
    }
}
