﻿using Revrs;
using SarcLibrary.Structures;
using System.Runtime.InteropServices.Marshalling;

namespace SarcLibrary.Writers;

public class SfntWriter
{
    public unsafe static void Write(RevrsWriter writer, SarcNodeData[] entries)
    {
        SfntHeader header = new() {
            Magic = Sarc.SFNT_MAGIC,
            HeaderSize = 0x8,
        };

        writer.Write<SfntHeader, SfntHeader.Reverser>(header);

        for (int i = 0; i < entries.Length; i++) {
            SarcNodeData entry = entries[i];
            byte* ptr = Utf8StringMarshaller.ConvertToUnmanaged(entry.Name);
            Span<byte> bytes = new(ptr, entry.Name.Length);
            writer.Write(bytes);
            writer.Write((byte)0x0);
            writer.Align(0x4);
        }
    }
}
