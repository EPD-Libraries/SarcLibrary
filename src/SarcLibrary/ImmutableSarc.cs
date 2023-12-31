﻿using Revrs;
using SarcLibrary.Readers;
using SarcLibrary.Structures;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;

namespace SarcLibrary;

public unsafe readonly ref struct ImmutableSarc
{
    public readonly SarcHeader Header;
    public readonly SfatReader SfatReader;
    public readonly SfntReader SfntReader;
    public readonly Span<byte> Data;

    public unsafe readonly ImmutableSarcEntry this[string name] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            byte* ptr = Utf8StringMarshaller.ConvertToUnmanaged(name);
            return this[
                new Span<byte>(ptr, name.Length)
            ];
        }
    }

    public readonly ImmutableSarcEntry this[ReadOnlySpan<byte> key] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            ref SfatNode node = ref SfatReader[key];
            int nameOffset = node.GetNameOffset();

            return new(
                nameOffset >= 0 ? SfntReader.RawNameData[nameOffset..] : [],
                Data[node.DataStartOffset..node.DataEndOffset],
                node.FileNameHash
            );
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ImmutableSarc(ref RevrsReader reader)
    {
        ref SarcHeader header = ref reader.Read<SarcHeader, SarcHeader.Reverser>();
        if (header.ByteOrderMark is Endianness.Little) {
            // Reverse the buffer back to LE
            // since it's initially read in BE
            reader.Reverse<SarcHeader, SarcHeader.Reverser>(0);
            reader.Endianness = Endianness.Little;
        }

        if (header.Magic != Sarc.SARC_MAGIC) {
            throw new InvalidDataException("Invalid SARC magic!");
        }

        if (header.HeaderSize != 0x14) {
            throw new InvalidDataException("Invalid SARC header!");
        }

        SfatReader = new SfatReader(ref reader);
        SfntReader = new SfntReader(ref reader, header.DataOffset);
        Data = reader.Data[header.DataOffset..header.FileSize];

        Header = header;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Enumerator GetEnumerator() => new(this);

    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref struct Enumerator(ImmutableSarc sarc)
    {
        private readonly ImmutableSarc _sarc = sarc;
        private readonly int _length = sarc.SfatReader.Nodes.Length;
        private int _index = -1;

        public readonly ImmutableSarcEntry Current {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                ref SfatNode node = ref _sarc.SfatReader.Nodes[_index];
                int nameOffset = node.GetNameOffset();

                return new(
                    nameOffset >= 0 ? _sarc.SfntReader.RawNameData[nameOffset..] : [],
                    _sarc.Data[node.DataStartOffset..node.DataEndOffset],
                    node.FileNameHash
                );
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            if (++_index >= _length) {
                return false;
            }

            return true;
        }
    }
}
