using System.Globalization;
using System.Text.RegularExpressions;

namespace IntelHex;

public record HexEntry
{
    public readonly byte ByteCount;
    public readonly uint Address;
    public readonly byte RecordType;
    public readonly byte[] Data;
    public readonly byte Checksum;
    public HexEntry(Match regexLine)
    {
        ByteCount = byte.Parse(regexLine.Groups["byteCount"].Value.AsSpan(), NumberStyles.HexNumber);
        Address = uint.Parse(regexLine.Groups["address"].Value.AsSpan(), NumberStyles.HexNumber);
        RecordType = byte.Parse(regexLine.Groups["recordType"].Value.AsSpan(), NumberStyles.HexNumber);
        Data = HexStringConverter.ToByteArray(regexLine.Groups["data"].Value);
        Checksum = byte.Parse(regexLine.Groups["checksum"].Value.AsSpan(), NumberStyles.HexNumber);
    }
}