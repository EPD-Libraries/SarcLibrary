using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;

namespace SarcLibrary;

public readonly ref struct ImmutableSarcEntry(Span<byte> name, Span<byte> data, uint nameHash)
{
    private readonly Span<byte> _name = name;
    private readonly Span<byte> _data = data;
    private readonly uint _nameHash = nameHash;

    public readonly Span<byte> Data {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _data;
    }

    public unsafe string Name {
        get {
            if (_name.IsEmpty) {
                return $"0x{_nameHash:x2}";
            }

            fixed (byte* ptr = _name) {
                return Utf8StringMarshaller.ConvertToManaged(ptr)
                    ?? string.Empty;
            }
        }
    }

    public unsafe void Deconstruct(out string name, out Span<byte> data)
    {
        name = Name;
        data = Data;
    }
}
