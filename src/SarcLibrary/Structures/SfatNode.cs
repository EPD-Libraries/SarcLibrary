using Revrs;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SarcLibrary.Structures;

[StructLayout(LayoutKind.Explicit, Pack = 4, Size = 0x10)]
public struct SfatNode
{
    [FieldOffset(0x0)]
    public uint FileNameHash;

    [FieldOffset(0x4)]
    public int FileAttributes;

    [FieldOffset(0x8)]
    public int DataStartOffset;

    [FieldOffset(0xC)]
    public int DataEndOffset;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly int GetNameOffset() => GetNameOffset(out _);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly int GetNameOffset(out bool isHashOnly)
    {
        isHashOnly = (byte)(FileAttributes >> 24) != 1;
        return isHashOnly ? -1 : (FileAttributes & 0xFFFF) * 4;
    }

    public class Reverser : IStructReverser
    {
        public static void Reverse(in Span<byte> slice)
        {
            // FileNameHash
            slice[0x00..0x04].Reverse();

            // FileAttributes
            slice[0x04..0x08].Reverse();

            // DataStartOffset
            slice[0x08..0x0C].Reverse();

            // DataEndOffset
            slice[0x0C..0x10].Reverse();
        }
    }
}
