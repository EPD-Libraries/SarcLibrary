using Revrs;

namespace SarcLibrary.Tests;

public static class DataProvider
{
    public static byte[] GetTest1(Endianness endianness)
    {
        return File.ReadAllBytes(endianness switch
        {
            Endianness.Little => "Data/Test1-LE.sarc",
            _ => "Data/Test1-BE.sarc"
        });
    }

    public static Stream StreamTest1(Endianness endianness)
    {
        return File.OpenRead(endianness switch
        {
            Endianness.Little => "Data/Test1-LE.sarc",
            _ => "Data/Test1-BE.sarc"
        });
    }
    
    
    public static byte[] GetAirCurrent()
    {
        return File.ReadAllBytes("Data/AirCurrent.sarc");
    }
    public static void WriteGeneratedAirCurrent(MemoryStream stream)
    {
        File.WriteAllBytes("Data/AirCurrentOutput.sarc", stream.ToArray());
    }
}