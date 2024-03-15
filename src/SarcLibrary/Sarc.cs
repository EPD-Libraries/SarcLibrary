global using SarcNodeData = (string Name, (uint FileNameHash, byte[] Data, int Alignment) Value);

using Revrs;
using SarcLibrary.Structures;
using SarcLibrary.Writers;

namespace SarcLibrary;

public class Sarc : Dictionary<string, byte[]>
{
    internal const uint SARC_MAGIC = 0x43524153;
    internal const uint SFAT_MAGIC = 0x54414653;
    internal const uint SFNT_MAGIC = 0x544E4653;

    /// <summary>
    /// The <see langword="byte-order"/> of the <see cref="Sarc"/>.
    /// </summary>
    public Endianness Endianness { get; set; } = Endianness.Little;

    /// <summary>
    /// The version of the <see cref="Sarc"/>.
    /// </summary>
    public int Version { get; private set; } = 0x100;

    /// <summary>
    /// When <see langword="true"/>, the SFNT (string) section will not be written.
    /// </summary>
    public bool IsHashOnly { get; set; } = false;

    /// <summary>
    /// Reads an <see cref="ImmutableSarc"/> from the input <paramref name="data"/> and copies the files into managed memory.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static Sarc FromBinary(Span<byte> data)
    {
        RevrsReader reader = new(data);
        ImmutableSarc sarc = new(ref reader);
        return FromImmutable(ref sarc);
    }

    /// <summary>
    /// Create a new <see cref="Sarc"/> object from an <see cref="ImmutableSarc"/>
    /// </summary>
    /// <param name="sarc"></param>
    /// <returns></returns>
    public static Sarc FromImmutable(ref ImmutableSarc sarc)
    {
        Sarc managed = [];
        managed.Endianness = sarc.Header.ByteOrderMark;
        managed.Version = sarc.Header.Version;

        foreach ((var fileName, var fileData) in sarc) {
            managed.Add(fileName, fileData.ToArray());
        }

        return managed;
    }

    /// <summary>
    /// Write the <see cref="Sarc"/> to the provided <paramref name="stream"/>.
    /// </summary>
    /// <param name="stream">The ouput stream to write to.</param>
    /// <param name="endianness">The <see langword="byte-order"/> to write the data in (defaults to <see cref="Endianness"/>).</param>
    public unsafe void Write(Stream stream, Endianness? endianness = null, bool legacy = false)
    {
        endianness ??= Endianness;
        RevrsWriter writer = new(stream, endianness.Value);

        writer.Move(sizeof(SarcHeader));

        SarcNodeData[] sorted = this
            .Select(x => (x.Key, Value: (
                Hash: SfatWriter.GetFileNameHash(x.Key), x.Value,
                Alignment: SarcAlignment.Estimate(x, endianness.Value, legacy)
            )))
            .OrderBy(x => x.Value.Hash)
            .ToArray();

        SfatWriter.Write(writer, sorted, IsHashOnly);

        if (!IsHashOnly) {
            SfntWriter.Write(writer, sorted);
        }

        int sarcAlignment = 1;
        foreach ((var _, var value) in sorted) {
            sarcAlignment = SarcAlignment.LCM(sarcAlignment, value.Alignment);
        }

        writer.Align(sarcAlignment);

        int dataOffset = (int)writer.Position;
        foreach ((var _, var value) in sorted) {
            writer.Align(value.Alignment);
            writer.Write(value.Data);
        }

        SarcHeader header = new() {
            Magic = SARC_MAGIC,
            HeaderSize = 0x14,
            ByteOrderMark = Endianness.Big,
            FileSize = (int)writer.Position,
            DataOffset = dataOffset,
            Version = (ushort)Version
        };

        writer.Seek(0);
        writer.Write<SarcHeader, SarcHeader.Reverser>(header);
    }
}
