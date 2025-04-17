using System.Buffers.Binary;
using System.Runtime.InteropServices.Marshalling;
using Revrs;
using Revrs.Extensions;
using SarcLibrary.Readers;
using SarcLibrary.Structures;

namespace SarcLibrary;

public static class SarcTools
{
    public static unsafe bool JumpToEntry(Stream src, string key, out int size)
    {
        byte* ptr = Utf8StringMarshaller.ConvertToUnmanaged(key);
        return JumpToEntry(src, new ReadOnlySpan<byte>(ptr, key.Length), out size);
    }
    
    public static bool JumpToEntry(Stream src, ReadOnlySpan<byte> key, out int size)
    {
        if (!src.CanSeek) {
            throw new InvalidOperationException("Input stream ust be seekable!");
        }
        
        long startPos = src.Position;
        
        if (!CheckHeader(src, out SarcHeader header, out Endianness endianness)) {
            throw new InvalidDataException("Invalid SARC magic!");
        }
        
        SfatHeader sfat = src.Read<SfatHeader, SfatHeader.Reverser>(endianness);
        uint hash = SfatReader.GetHash(key, sfat.HashKey);
        (int dataOffset, int dataEnd) = GetEntryIndex(src, ref sfat, hash, endianness);

        if (dataOffset < 0 || dataEnd < 0) {
            size = -1;
            return false;
        }

        size = dataEnd - dataOffset;
        src.Seek(startPos + header.DataOffset + dataOffset, SeekOrigin.Begin);
        return true;
    }

    private static bool CheckHeader(Stream src, out SarcHeader header, out Endianness endianness)
    {
        header = src.Read<SarcHeader, SarcHeader.Reverser>(endianness = Endianness.Big);
        
        if (header.ByteOrderMark != Endianness.Big) {
            endianness = (Endianness)BinaryPrimitives.ReverseEndianness((ushort)endianness);
            src.Seek(-0x14, SeekOrigin.Current);
            header = src.Read<SarcHeader, SarcHeader.Reverser>(endianness);
        }

        return header.Magic == Sarc.MAGIC;
    }

    private static (int, int) GetEntryIndex(Stream src, ref SfatHeader sfat, uint hash, Endianness endianness)
    {
        long sfatEntriesStartPos = src.Position;
        
        double l = 0;
        double r = sfat.NodeCount - 1;
        while (l <= r) {
            int m = (int)Math.Floor((l + r) / 2);

            src.Seek(sfatEntriesStartPos + 16 * m, SeekOrigin.Begin);
            SfatNode node = src.Read<SfatNode, SfatNode.Reverser>(endianness);
            
            if (node.FileNameHash < hash) {
                l = m + 1;
            }
            else if (node.FileNameHash > hash) {
                r = m - 1;
            }
            else {
                return (node.DataStartOffset, node.DataEndOffset);
            }
        }

        return (-1, -1);
    }
}