using System.Text.RegularExpressions;
using IntelHex;
using Xunit;

namespace IntelHexTests;

public class UnitTest1
{
    [Theory]
    [InlineData(":00000001FF",0,0,1,new byte[]{},0xff)]
    [InlineData(":020000020000",2,0,2,new byte[]{0x00},0)]
    public void Test1(string input, byte byteCount, uint address, byte recordType, byte[] data, byte checksum)
    {
        Regex hexLineRegex =
            new(
                ":(?<byteCount>[0-9A-F]{2})(?<address>[0-9A-F]{4})(?<recordType>[0-9A-F]{2})(?<data>[0-9A-F]*)(?<checksum>[0-9A-F]{2}$)",
                RegexOptions.Compiled);
        var actual = new HexEntry(hexLineRegex.Match(input));
        Assert.Equal(byteCount, actual.ByteCount);
        Assert.Equal(address,actual.Address);
        Assert.Equal(recordType,actual.RecordType);
        Assert.Equal(data,actual.Data);
        Assert.Equal(checksum,actual.Checksum);
    }
}