using Revrs;
using SarcLibrary.Structures;
using System.Runtime.InteropServices.Marshalling;

namespace SarcLibrary.Writers;

public class SfntWriter
{
    public static unsafe void Write(ref RevrsWriter writer, Span<SarcNodeData> entries)
    {
        SfntHeader header = new() {
            Magic = Sarc.SFNT_MAGIC,
            HeaderSize = 0x8,
        };

        writer.Write<SfntHeader, SfntHeader.Reverser>(header);

        for (int i = 0; i < entries.Length; i++) {
            ref SarcNodeData entry = ref entries[i];
            byte* ptr = Utf8StringMarshaller.ConvertToUnmanaged(entry.Name);
            Span<byte> bytes = new(ptr, entry.Name.Length);
            writer.Write(bytes);
            writer.Write((byte)0x0);
            writer.AlignAtEnd(0x4);
        }
    }
}
