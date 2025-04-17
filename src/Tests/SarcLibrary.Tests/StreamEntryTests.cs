using Revrs;

namespace SarcLibrary.Tests;

public class StreamEntryTests
{
    [Fact]
    public void CanStreamEntry()
    {
        using Stream src = DataProvider.StreamTest1(Endianness.Little);
        SarcTools.JumpToEntry(src, "Test1.txt", out int size);
        
        size.Should().Be(5);
        
        byte[] buffer = new byte[5];
        src.ReadExactly(buffer);

        buffer.Should().Equal("Test1"u8.ToArray());
    }
}