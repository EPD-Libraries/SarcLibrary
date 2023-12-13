using Revrs;
using System.Runtime.InteropServices;

namespace SarcLibrary.Structures;

[StructLayout(LayoutKind.Explicit, Pack = 2, Size = 0x8)]
public struct SfntHeader
{
    [FieldOffset(0x00)]
    public uint Magic;

    [FieldOffset(0x04)]
    public ushort HeaderSize;

    public class Reverser : IStructReverser
    {
        public static void Reverse(in Span<byte> slice)
        {
            // HeaderSize
            slice[0x04..0x06].Reverse();
        }
    }
}
