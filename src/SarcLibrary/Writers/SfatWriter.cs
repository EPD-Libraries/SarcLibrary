using Revrs;
using SarcLibrary.Structures;

namespace SarcLibrary.Writers;

internal class SfatWriter
{
    private const uint HASH_KEY = 0x65;
    private const ushort HEADER_SIZE = 0xC;

    public static void Write(RevrsWriter writer, SarcNodeData[] entries, bool isHashOnly)
    {
        SfatHeader header = new() {
            Magic = Sarc.SFAT_MAGIC,
            HeaderSize = HEADER_SIZE,
            NodeCount = (ushort)entries.Length,
            HashKey = HASH_KEY,
        };

        writer.Write<SfatHeader, SfatHeader.Reverser>(header);

        int dataOffset = 0;
        int nameOffset = 0;

        for (int i = 0; i < entries.Length; i++) {
            SarcNodeData entry = entries[i];

            SfatNode node = new() {
                FileNameHash = entry.Value.FileNameHash,
                FileAttributes = isHashOnly ? 0x0 : 0x01000000 | (nameOffset / 4),
                DataStartOffset = dataOffset,
                DataEndOffset = dataOffset += entry.Value.Data.Length.Align(entry.Value.Alignment)
            };

            nameOffset += (entry.Name.Length + 1).Align(0x4);
            writer.Write<SfatNode, SfatNode.Reverser>(node);
        }
    }

    public static uint GetFileNameHash(ReadOnlySpan<char> name)
    {
        long hash = 0;
        for (int i = 0; i < name.Length; i++) {
            hash = hash * HASH_KEY + (sbyte)name[i];
        }

        return (uint)hash;
    }
}
