global using SarcNodeData = (string Name, (uint FileNameHash, System.ArraySegment<byte> Data, int Alignment) Value);
using CommunityToolkit.HighPerformance.Buffers;
using Revrs;
using SarcLibrary.Structures;
using SarcLibrary.Writers;

namespace SarcLibrary;

public class Sarc : Dictionary<string, ArraySegment<byte>>
{
    public const uint SARC_MAGIC = 0x43524153;
    public const uint SFAT_MAGIC = 0x54414653;
    public const uint SFNT_MAGIC = 0x544E4653;
    public const int MIN_ALIGNMENT = 0x4;

    /// <summary>
    /// The <see langword="byte-order"/> of the <see cref="Sarc"/>.
    /// </summary>
    public Endianness Endianness { get; set; } = Endianness.Little;

    /// <summary>
    /// The version of the <see cref="Sarc"/>.
    /// </summary>
    public int Version { get; set; } = 0x100;

    /// <summary>
    /// The minimum guessed alignment of the <see cref="Sarc"/>.
    /// </summary>
    public int MinAlignment { get; set; } = MIN_ALIGNMENT;

    /// <summary>
    /// When <see langword="true"/>, the SFNT (string) section will not be written.
    /// </summary>
    public bool IsHashOnly { get; set; } = false;

    /// <summary>
    /// Reads an <see cref="ImmutableSarc"/> from the input <paramref name="data"/>.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static Sarc FromBinary(ArraySegment<byte> data)
    {
        RevrsReader reader = new(data);
        ImmutableSarc sarc = new(ref reader);
        return FromImmutable(ref sarc, data);
    }

    /// <summary>
    /// Create a new <see cref="Sarc"/> object from an <see cref="ImmutableSarc"/>
    /// </summary>
    /// <param name="sarc"></param>
    /// <returns></returns>
    public static Sarc FromImmutable(ref ImmutableSarc sarc, ArraySegment<byte> data)
    {
        Sarc result = [];
        result.Endianness = sarc.Header.ByteOrderMark;
        result.Version = sarc.Header.Version;

        foreach (var (fileName, dataStartOffset, dataEndOffset) in sarc) {
            result.MinAlignment = SarcAlignment.GCD(result.MinAlignment, sarc.Header.DataOffset + dataStartOffset);
            result.Add(fileName, data[dataStartOffset..dataEndOffset]);
        }

        return result;
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

        int sarcAlignment = 1;
        using SpanOwner<SarcNodeData> sorted = SpanOwner<SarcNodeData>.Allocate(Count);

        int i = -1;
        foreach (KeyValuePair<string, ArraySegment<byte>> entry in this) {
            uint hash = SfatWriter.GetFileNameHash(entry.Key);

            int alignment = SarcAlignment.Estimate(entry, MinAlignment, endianness.Value, legacy);
            sarcAlignment = SarcAlignment.LCM(sarcAlignment, alignment);

            sorted.Span[++i] = (entry.Key, Value: (hash, entry.Value, alignment));
        }

        sorted.Span.Sort((SarcNodeData x, SarcNodeData y) => x.Value.FileNameHash.CompareTo(y.Value.FileNameHash));

        SfatWriter.Write(ref writer, sorted.Span, IsHashOnly);

        if (!IsHashOnly) {
            SfntWriter.Write(ref writer, sorted.Span);
        }

        writer.Align(sarcAlignment);

        int dataOffset = (int)writer.Position;
        foreach ((var _, var value) in sorted.Span) {
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
