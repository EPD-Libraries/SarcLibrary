using Revrs;

namespace SarcLibrary.Tests;

public class ByteOrderTest
{
    [Fact]
    public void CanReadLeFromLe()
    {
        byte[] data = DataProvider.GetTest1(Endianness.Little);
        RevrsReader reader = new(data, endianness: Endianness.Little);
        
        ImmutableSarc sarc = new(ref reader);
        sarc.Header.Magic.Should().Be(Sarc.MAGIC);
        sarc.Header.HeaderSize.Should().Be(0x14);
    }
    
    [Fact]
    public void CanReadBeFromLe()
    {
        byte[] data = DataProvider.GetTest1(Endianness.Big);
        RevrsReader reader = new(data, endianness: Endianness.Little);
        
        ImmutableSarc sarc = new(ref reader);
        sarc.Header.Magic.Should().Be(Sarc.MAGIC);
        sarc.Header.HeaderSize.Should().Be(0x14);
    }
    
    [Fact]
    public void CanReadLeFromBe()
    {
        byte[] data = DataProvider.GetTest1(Endianness.Little);
        RevrsReader reader = new(data, endianness: Endianness.Big);
        
        ImmutableSarc sarc = new(ref reader);
        sarc.Header.Magic.Should().Be(Sarc.MAGIC);
        sarc.Header.HeaderSize.Should().Be(0x14);
    }
    
    [Fact]
    public void CanReadBeFromBe()
    {
        byte[] data = DataProvider.GetTest1(Endianness.Big);
        RevrsReader reader = new(data, endianness: Endianness.Big);
        
        ImmutableSarc sarc = new(ref reader);
        sarc.Header.Magic.Should().Be(Sarc.MAGIC);
        sarc.Header.HeaderSize.Should().Be(0x14);
    }
}