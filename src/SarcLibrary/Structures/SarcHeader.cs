using Revrs;
using System.Runtime.InteropServices;

namespace SarcLibrary.Structures;

[StructLayout(LayoutKind.Explicit, Pack = 2, Size = 0x14)]
public struct SarcHeader
{
    [FieldOffset(0x00)]
    public uint Magic;

    [FieldOffset(0x04)]
    public ushort HeaderSize;

    [FieldOffset(0x06)]
    public Endianness ByteOrderMark;

    [FieldOffset(0x08)]
    public int FileSize;

    [FieldOffset(0x0C)]
    public int DataOffset;

    [FieldOffset(0x10)]
    public ushort Version;

    public class Reverser : IStructReverser
    {
        public static void Reverse(in Span<byte> slice)
        {
            // HeaderSize
            slice[0x04..0x06].Reverse();

            // ByteOrderMark
            slice[0x06..0x08].Reverse();

            // FileSize
            slice[0x08..0x0C].Reverse();

            // DataOffset
            slice[0x0C..0x10].Reverse();

            // Version
            slice[0x10..0x12].Reverse();
        }
    }
}
