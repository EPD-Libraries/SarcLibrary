using Revrs;
using Revrs.Extensions;
using System.Runtime.CompilerServices;

namespace SarcLibrary.Writers;

public class SarcAlignment
{
    private const int MIN_ALIGNMENT = 0x4;

    private static readonly HashSet<string> _ukingFactoryNames = [
        "sarc", "bfres", "bcamanim", "batpl, bnfprl", "bplacement",
        "hks, lua", "bactcapt", "bitemico", "jpg", "bmaptex",
        "bstftex", "bgdata", "bgsvdata", "hknm2", "bmscdef", "bars",
        "bxml", "bgparamlist", "bmodellist", "baslist", "baiprog", "bphysics",
        "bchemical", "bas", "batcllist", "batcl", "baischedule", "bdmgparam",
        "brgconfiglist", "brgconfig", "brgbw", "bawareness", "bdrop", "bshop",
        "brecipe", "blod", "bbonectrl", "blifecondition", "bumii", "baniminfo",
        "byaml", "bassetting", "hkrb", "hkrg", "bphyssb", "hkcl", "hksc",
        "hktmrb", "brgcon", "esetlist", "bdemo", "bfevfl", "bfevtm"
    ];

    private static readonly Dictionary<string, int> _alignments = new() {
        { "aglatex", 8 }, { "baglatex", 8 }, { "aglblm", 8 }, { "baglblm", 8 },
        { "aglccr", 8 }, { "baglccr", 8 }, { "aglclwd", 8 }, { "baglclwd", 8 },
        { "aglcube", 8 }, { "baglcube", 8 }, { "agldof", 8 }, { "bagldof", 8 },
        { "aglenv", 8 }, { "baglenv", 8 }, { "aglenvset", 8 }, { "baglenvset", 8 },
        { "aglfila", 8 }, { "baglfila", 8 }, { "agllmap", 8 }, { "bagllmap", 8 },
        { "agllref", 8 }, { "bagllref", 8 }, { "aglshpp", 8 }, { "baglshpp", 8 },
        { "glght", 8 }, { "bglght", 8 }, { "glpbd", 8 }, { "bglpbd", 8 },
        { "glpbm", 8 }, { "bglpbm", 8 }, { "gsdw", 8 }, { "bgsdw", 8 },
        { "ksky", 8 }, { "bksky", 8 }, { "ofx", -8192 }, { "bofx", -8192 },
        { "pref", 8 }, { "bpref", 8 }, { "sharc", 0x1000 }, { "sharcb", 0x1000 },
        { "baglmf", 0x80 }, { "fmd", -8192 }, { "ftx", -8192 }, { "genvres", -8192 },
        { "gtx", 0x2000 }
    };

    public static int Estimate(KeyValuePair<string, byte[]> sarcEntry, Endianness endianness, bool legacy)
    {
        int result = MIN_ALIGNMENT;
        string ext = Path.GetExtension(sarcEntry.Key);

        if (ext.Length > 1) {
            ext = ext[1..];
        }

        if (ext is "bffnt") {
            result = LCM(result, endianness == Endianness.Big ? 0x2000 : 0x1000);
        }
        else if (_alignments.TryGetValue(ext, out int extensionMatchedAlignment)) {
            result = LCM(result, extensionMatchedAlignment);
        }

        if (!legacy) {
            return result;
        }

        if (IsSarcArchive(sarcEntry.Value)) {
            result = LCM(result, 0x2000);
        }

        if (!_ukingFactoryNames.Contains(ext)) {
            result = LCM(result, GetBinaryFileAlignment(sarcEntry.Value));

            if (endianness == Endianness.Big) {
                result = LCM(result, GetCafeBflimAlignment(sarcEntry.Value));
            }
        }

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetBinaryFileAlignment(Span<byte> data)
    {
        if (data.Length <= 0x20) {
            return 1;
        }

        int fileSize = data[0x1C..0x20].Read<int>(
            data[0x0C..0x0E].Read<Endianness>()
        );

        if (fileSize != data.Length) {
            return 1;
        }

        return 1 << data[0x0E];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetCafeBflimAlignment(Span<byte> data)
    {
        if (data.Length <= 0x28 || !data[^0x28..^0x24].SequenceEqual("FLIM"u8)) {
            return 1;
        }

        Endianness endianness = data[^0x24..^0x22].Read<Endianness>();
        return data[^0x8..^0x06].Read<int>(endianness);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsSarcArchive(ReadOnlySpan<byte> data)
    {
        return data.Length > 0 && (
            data[0x0..0x4].SequenceEqual("SARC"u8) ||
            data[0x0..0x4].SequenceEqual("Yaz0"u8) && data[0x11..0x15].SequenceEqual("SARC"u8)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GCD(int a, int b)
    {
        while (a != 0 && b != 0) {
            if (a > b) {
                a %= b;
            }
            else {
                b %= a;
            }
        }

        return a | b;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int LCM(int a, int b)
    {
        return a / GCD(a, b) * b;
    }
}
