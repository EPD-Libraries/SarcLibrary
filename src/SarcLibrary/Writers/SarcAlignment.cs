using Revrs;
using Revrs.Extensions;
using System.Runtime.CompilerServices;

namespace SarcLibrary.Writers;

public class SarcAlignment
{
    private const int MIN_ALIGNMENT = 0x4;

    public static int Estimate(KeyValuePair<string, ArraySegment<byte>> sarcEntry, Endianness endianness, bool legacy)
    {
        int result = MIN_ALIGNMENT;
        ReadOnlySpan<char> ext = Path.GetExtension(sarcEntry.Key.AsSpan());

        if (ext.Length > 1) {
            ext = ext[1..];
        }

        result = ext switch {
            "bffnt" => LCM(result, endianness == Endianness.Big ? 0x2000 : 0x1000),
            "aglatex" or "aglblm" or "aglccr" or
            "aglclwd" or "aglcube" or "agldof" or
            "aglenv" or "aglenvset" or "aglfila" or
            "agllmap" or "agllref" or "aglshpp" or
            "baglatex" or "baglblm" or "baglccr" or
            "baglclwd" or "baglcube" or "bagldof" or
            "baglenv" or "baglenvset" or "baglfila" or
            "bagllmap" or "bagllref" or "baglshpp" or
            "bglght" or "bglpbd" or "bglpbm" or
            "bgsdw" or "bksky" or "bpref" or
            "glght" or "glpbd" or "glpbm" or
            "gsdw" or "ksky" or "pref" => LCM(result, 8),
            "baglmf" => LCM(result, 0x80),
            "sharc" or "sharcb" => LCM(result, 0x1000),
            "bofx" or "fmd" or "ftx" or "genvres" or "gtx" or "ofx" => LCM(result, 0x2000),
            _ => result
        };

        if (!legacy) {
            return result;
        }

        if (IsSarcArchive(sarcEntry.Value)) {
            result = LCM(result, 0x2000);
        }

        if (ext is not ("sarc" or "bfres" or "bcamanim" or "batpl or bnfprl" or "bplacement" or
            "hks or lua" or "bactcapt" or "bitemico" or "jpg" or "bmaptex" or
            "bstftex" or "bgdata" or "bgsvdata" or "hknm2" or "bmscdef" or "bars" or
            "bxml" or "bgparamlist" or "bmodellist" or "baslist" or "baiprog" or "bphysics" or
            "bchemical" or "bas" or "batcllist" or "batcl" or "baischedule" or "bdmgparam" or
            "brgconfiglist" or "brgconfig" or "brgbw" or "bawareness" or "bdrop" or "bshop" or
            "brecipe" or "blod" or "bbonectrl" or "blifecondition" or "bumii" or "baniminfo" or
            "byaml" or "bassetting" or "hkrb" or "hkrg" or "bphyssb" or "hkcl" or "hksc" or
            "hktmrb" or "brgcon" or "esetlist" or "bdemo" or "bfevfl" or "bfevtm")) {
            result = LCM(result, GetBinaryFileAlignment(sarcEntry.Value));

            if (endianness == Endianness.Big) {
                result = LCM(result, GetCafeBflimAlignment(sarcEntry.Value));
            }
        }

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetBinaryFileAlignment(Span<byte> data)
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
    private static int GetCafeBflimAlignment(Span<byte> data)
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
    private static int GCD(int a, int b)
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
        return a * b / GCD(a, b);
    }
}
