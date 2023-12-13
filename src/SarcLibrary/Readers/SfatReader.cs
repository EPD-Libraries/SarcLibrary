using Revrs;
using SarcLibrary.Structures;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;

namespace SarcLibrary.Readers;

public readonly ref struct SfatReader
{
    public readonly SfatHeader Header;
    public readonly Span<SfatNode> Nodes;

    public unsafe readonly ref SfatNode this[string name] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            byte* ptr = Utf8StringMarshaller.ConvertToUnmanaged(name);
            return ref this[
                new Span<byte>(ptr, name.Length)
            ];
        }
    }

    public readonly ref SfatNode this[ReadOnlySpan<byte> key] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            return ref Nodes[
                GetIndex(GetHash(key))
            ];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SfatReader(ref RevrsReader reader)
    {
        Header = reader.Read<SfatHeader, SfatHeader.Reverser>();
        if (Header.Magic != Sarc.SFAT_MAGIC) {
            throw new InvalidDataException("Invalid SFAT header!");
        }

        Nodes = reader.ReadSpan<SfatNode, SfatNode.Reverser>(Header.NodeCount);
    }

    public uint GetHash(ReadOnlySpan<byte> key)
    {
        uint hash = 0;
        for (int i = 0; i < key.Length; i++) {
            hash = (hash * Header.HashKey) + key[i];
        }

        return hash;
    }

    private int GetIndex(uint hash)
    {
        int m;
        double l = 0;
        double r = Nodes.Length - 1;
        while (l <= r) {
            m = (int)Math.Floor((l + r) / 2);
            if (Nodes[m].FileNameHash < hash) {
                l = m + 1;
            }
            else if (Nodes[m].FileNameHash > hash) {
                r = m - 1;
            }
            else {
                return m;
            }
        }

        return -1;
    }
}
