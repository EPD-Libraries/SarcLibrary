using Revrs;

namespace SarcLibrary.Tests;

public class AlignmentTest
{
    [Fact]
    public void TestSMOAirCurrentSarcAlignment()
    {
        byte[] data = DataProvider.GetAirCurrent();
        var sarc = Sarc.FromBinary(data);
        Assert.NotNull(sarc);

        MemoryStream output = new();
        sarc.Write(output);

        DataProvider.WriteGeneratedAirCurrent(output);

        Assert.Equal(data, output.ToArray());
    }
}