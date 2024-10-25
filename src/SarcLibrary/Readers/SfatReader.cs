using Revrs;
using SarcLibrary.Structures;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;

namespace SarcLibrary.Readers;

public readonly ref struct SfatReader
{
    public readonly SfatHeader Header;
    public readonly Span<SfatNode> Nodes;

    public unsafe ref SfatNode this[string name] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            byte* ptr = Utf8StringMarshaller.ConvertToUnmanaged(name);
            return ref this[
                new Span<byte>(ptr, name.Length)
            ];
        }
    }

    public ref SfatNode this[ReadOnlySpan<byte> key] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get =>
            ref Nodes[
                GetIndex(GetHash(key))
            ];
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
        foreach (byte t in key) {
            hash = hash * Header.HashKey + t;
        }

        return hash;
    }

    private int GetIndex(uint hash)
    {
        double l = 0;
        double r = Nodes.Length - 1;
        while (l <= r) {
            int m = (int)Math.Floor((l + r) / 2);
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
