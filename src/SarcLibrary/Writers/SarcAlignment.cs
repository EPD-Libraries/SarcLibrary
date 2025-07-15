using Revrs;
using Revrs.Extensions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SarcLibrary.Writers;

public class SarcAlignment
{
    private const uint YAZ0_MAGIC = 0x307A6159;
    private const uint FLIM_MAGIC = 0x4D494C46;

    public static int Estimate(KeyValuePair<string, ArraySegment<byte>> sarcEntry, int minAlignment, Endianness endianness, bool legacy)
    {
        int result = minAlignment;
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
            "byml" or "baglmf" => LCM(result, 0x80),
            "bfres" or "sharc" or "sharcb" => LCM(result, 0x1000),
            "bofx" or "fmd" or "ftx" or "genvres" or "gtx" or "ofx" => LCM(result, 0x2000),
            _ => result
        };

        if (legacy && IsSarcArchive(sarcEntry.Value)) {
            result = LCM(result, 0x2000);
        }

        if (legacy || ext is not ("sarc" or "bfres" or "bcamanim" or "batpl" or "bnfprl" or "bplacement" or
            "hks or lua" or "bactcapt" or "bitemico" or "jpg" or "bmaptex" or
            "bstftex" or "bgdata" or "bgsvdata" or "hknm2" or "bmscdef" or "bars" or
            "bxml" or "bgparamlist" or "bmodellist" or "baslist" or "baiprog" or "bphysics" or
            "bchemical" or "bas" or "batcllist" or "batcl" or "baischedule" or "bdmgparam" or
            "brgconfiglist" or "brgconfig" or "brgbw" or "bawareness" or "bdrop" or "bshop" or
            "brecipe" or "blod" or "bbonectrl" or "blifecondition" or "bumii" or "baniminfo" or
            "byaml" or "byml" or "bassetting" or "hkrb" or "hkrg" or "bphyssb" or "hkcl" or "hksc" or
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

        // Make a copy to avoid mutating the
        // input data when swapping endianness.
        Span<byte> copy = stackalloc byte[0x14];
        data[0x0C..0x20].CopyTo(copy);

        int fileSize = copy[0x10..0x14].Read<int>(
            copy[0x0..0x2].Read<Endianness>(Endianness.Big)
        );

        if (fileSize != data.Length) {
            return 1;
        }

        return 1 << data[0x0E];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetCafeBflimAlignment(Span<byte> data)
    {
        if (data.Length <= 0x28 || data[^0x28..^0x24].Read<uint>() != FLIM_MAGIC) {
            return 1;
        }

        Span<byte> copy = stackalloc byte[0x4];
        data[^0x24..^0x22].CopyTo(copy); // endianness
        data[^0x8..^0x06].CopyTo(copy[2..]);

        return data[0x2..].Read<ushort>(
            copy[0x0..0x2].Read<Endianness>(Endianness.Big)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsSarcArchive(ReadOnlySpan<byte> data)
    {
        return data.Length > 0 && (
            data[0x0..0x4].Read<uint>() == Sarc.MAGIC ||
            data[0x0..0x4].Read<uint>() == YAZ0_MAGIC && data[0x11..0x15].Read<uint>() == Sarc.MAGIC
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GCD(int m, int n)
    {
        if (m == 0 || n == 0) {
            return m | n;
        }

        int shift = BitOperations.TrailingZeroCount(m | n);

        m >>= BitOperations.TrailingZeroCount(m);
        n >>= BitOperations.TrailingZeroCount(n);

        while (m != n) {
            if (m > n) {
                m -= n;
                m >>= BitOperations.TrailingZeroCount(m);
            }
            else {
                n -= m;
                n >>= BitOperations.TrailingZeroCount(n);
            }
        }

        return m << shift;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int LCM(int a, int b)
    {
        return a * b / GCD(a, b);
    }
}
