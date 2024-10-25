using System.Text;

// ReSharper disable once CheckNamespace
namespace SarcLibrary;

public enum Endian : ushort { Big = 0xFFFE, Little = 0xFEFF }

[Obsolete("Use SarcLibrary.Sarc or SarcLibrary.ImmutableSarc instead.")]
public class SarcFile : Dictionary<string, byte[]>
{
    public Endian Endian { get; set; }
    public bool HashOnly { get; set; } = false;
    public bool Legacy { get; set; } = false;

    public SarcFile()
    {
        Endian = BitConverter.IsLittleEndian ? Endian.Little : Endian.Big;
    }

    public SarcFile(Stream stream)
    {
        using BinaryReader reader = new(stream);

        Span<byte> sarc = stackalloc byte[4];
        stream.Read(sarc);
        if (!sarc.SequenceEqual(SarcHelper._sarc)) {
            throw new InvalidDataException("Invalid SARC magic");
        }

        stream.Seek(2, SeekOrigin.Current);
        Endian = (Endian)reader.ReadUInt16();

        int fileSize = reader.ReadInt32();
        int dataOffset = reader.ReadInt32();
        stream.Seek(10, SeekOrigin.Current);

        ushort count = reader.ReadUInt16();
        stream.Seek(4, SeekOrigin.Current);

        if (Endian == Endian.Big) {
            fileSize.SwapEndian();
            dataOffset.SwapEndian();
            count.SwapEndian();
        }

        Span<(uint Hash, int StringOffset, int DataStart, int DataEnd)> nodes = stackalloc (uint, int, int, int)[count];

        for (int i = 0; i < count; i++) {
            uint hash = reader.ReadUInt32();
            int attributes = reader.ReadInt32();
            int dataStart = reader.ReadInt32();
            int dataEnd = reader.ReadInt32();

            if (Endian == Endian.Big) {
                hash.SwapEndian();
                attributes.SwapEndian();
                dataStart.SwapEndian();
                dataEnd.SwapEndian();
            }

            HashOnly = (byte)(attributes >> 24) != 1;
            int strOffset = (attributes & 0xFFFF) * 4;

            nodes[i] = (hash, strOffset, dataStart, dataEnd);
        }

        stream.Seek(8, SeekOrigin.Current);

        if (!HashOnly) {
            Span<byte> stringTableBuffer = reader.ReadBytes((int)(dataOffset - stream.Position)).AsSpan();
            for (int i = 0; i < count; i++) {
                if (i == count - 1) {
                    Add(Encoding.UTF8.GetString(stringTableBuffer[nodes[i].StringOffset..]).Replace(SarcHelper.NULL, SarcHelper.EMPTY),
                       reader.ReadBytes(nodes[i].DataEnd - nodes[i].DataStart));
                }
                else {
                    Add(Encoding.UTF8.GetString(stringTableBuffer[nodes[i].StringOffset..nodes[i + 1].StringOffset]).Replace(SarcHelper.NULL, SarcHelper.EMPTY),
                       reader.ReadBytes(nodes[i].DataEnd - nodes[i].DataStart));
                    stream.Seek(nodes[i + 1].DataStart - nodes[i].DataEnd, SeekOrigin.Current);
                }
            }
        }
        else {
            stream.Seek(dataOffset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++) {
                byte[] buffer = reader.ReadBytes(nodes[i].DataEnd - nodes[i].DataStart);
                Add($"{nodes[i].Hash:x8}.{SarcHelper.GuessFileExtension(buffer)}", buffer);

                if (i != count - 1) {
                    stream.Seek(nodes[i + 1].DataStart - nodes[i].DataEnd, SeekOrigin.Current);
                }
            }
        }
    }

    public static SarcFile FromBinary(byte[] data)
    {
        using MemoryStream ms = new(data);
        return new SarcFile(ms);
    }

    public static SarcFile FromBinary(string sarcFile)
    {
        using FileStream fs = File.OpenRead(sarcFile);
        return new SarcFile(fs);
    }

    public static SarcFile FromBinary(Stream stream)
    {
        return new SarcFile(stream);
    }

    public byte[] ToBinary()
    {
        using MemoryStream ms = new();
        ToBinary(ms);
        return ms.ToArray();
    }

    public void ToBinary(string outputFile)
    {
        using FileStream fs = File.Create(outputFile);
        ToBinary(fs);
    }

    public void ToBinary(Stream stream)
    {
        using BinaryWriter writer = new(stream);

        // Allocate sorted keys/values
        string[] fileNames = [.. Keys];
        uint[] keys = new uint[Count];
        for (int i = 0; i < Count; i++) {
            keys[i] = SarcHelper.GetHash(fileNames[i]);
        }

        Array.Sort(keys, fileNames);

        stream.Seek(0x14, SeekOrigin.Begin);

        // Write data nodes (SFAT)
        writer.Write(SarcHelper._sfat);
        writer.Write(0x0C.AsInt16(Endian));
        writer.Write(Count.AsInt16(Endian));
        writer.Write(SarcHelper.HASH_KEY.AsUInt32(Endian));

        Span<int> alignments = stackalloc int[Count];
        int relStringOffset = 0;
        int relDataOffset = 0;
        int fileAlignment = 1;
        for (int i = 0; i < Count; i++) {
            string fileName = fileNames[i];

            // Calculate data and string offsets
            Span<byte> buffer = this[fileName].AsSpan();
            int alignment = SarcHelper.GetFileAlignment(fileName, buffer, this);
            int dataStart = relDataOffset.Align(alignment);
            int dataEnd = dataStart + buffer.Length;
            alignments[i] = alignment;

            writer.Write(keys[i].AsUInt32(Endian));
            writer.Write((HashOnly ? 0x00 : (0x01000000 | (relStringOffset / 4))).AsInt32(Endian));
            writer.Write(dataStart.AsInt32(Endian));
            writer.Write(dataEnd.AsInt32(Endian));

            relDataOffset = dataEnd;
            relStringOffset += (fileName.Length + 4) & -4;
            fileAlignment = SarcHelper.LCM(fileAlignment, alignment);
        }

        // Write string table (SFNT)
        writer.Write(SarcHelper._sfnt);
        writer.Write(0x08.AsInt16(Endian));
        writer.Write((ushort)0x00);

        for (int i = 0; i < Count; i++) {
            byte[] buffer = Encoding.UTF8.GetBytes(fileNames[i]);
            byte[] aligned = new byte[buffer.Length + 4 & -4];
            Array.Copy(buffer, aligned, buffer.Length);
            writer.Write(aligned);
        }

        stream.Align(fileAlignment);
        int dataOffset = (int)stream.Position;

        // Write data
        for (int i = 0; i < Count; i++) {
            stream.Align(alignments[i]);
            writer.Write(this[fileNames[i]]);
        }

        int fileSize = (int)stream.Position;

        stream.Seek(0, SeekOrigin.Begin);
        writer.Write(SarcHelper._sarc);
        writer.Write(0x14.AsInt16(Endian));
        writer.Write((ushort)Endian);
        writer.Write(fileSize.AsInt32(Endian));
        writer.Write(dataOffset.AsInt32(Endian));
        writer.Write(0x0100.AsInt16(Endian)); // SARC Version
        writer.Write((ushort)0x00); // Reserved
    }

    public Task ExtractToDirectory(string outputDirectory, Func<string, byte[], byte[]>? operation = null)
    {
        return Parallel.ForEachAsync(this, async (sarcFile, cancellationToken) => {
            string file = Path.Combine(outputDirectory, sarcFile.Key[0] == '/' ? sarcFile.Key.Remove(0, 1) : sarcFile.Key);
            Directory.CreateDirectory(Path.GetDirectoryName(file) ?? "");
            await File.WriteAllBytesAsync(file, operation?.Invoke(sarcFile.Key, sarcFile.Value) ?? sarcFile.Value, cancellationToken);
        });
    }

    public static SarcFile LoadFromDirectory(string directory, Func<string, byte[], byte[]>? operation = null, string searchPattern = "*.*", SearchOption searchOption = SearchOption.AllDirectories)
    {
        SarcFile sarc = [];
        foreach (var file in Directory.GetFiles(directory, searchPattern, searchOption)) {
            byte[] data = File.ReadAllBytes(file);
            string name = Path.GetRelativePath(directory, file).Replace('\\', '/');
            sarc.Add(name, operation?.Invoke(name, data) ?? data);
        }

        return sarc;
    }
}
