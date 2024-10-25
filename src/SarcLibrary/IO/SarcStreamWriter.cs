namespace SarcLibrary.IO;

internal sealed class SarcStreamWriter(Sarc sarc, string key) : MemoryStream
{
    protected override void Dispose(bool disposing)
    {
        if (!disposing) {
            return;
        }

        sarc[key] = ToArray();
    }
}