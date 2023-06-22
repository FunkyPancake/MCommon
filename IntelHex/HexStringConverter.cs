using System.Globalization;
using System.Text;

namespace IntelHex;

public static class HexStringConverter
{
    public static byte[] ToByteArray(ReadOnlySpan<char> hexString)
    {
        var arr = new byte[(hexString.Length / 2)];
        for (var i = 0; i < arr.Length; i++)
        {
            arr[i] = byte.Parse(hexString.Slice(i, 2), NumberStyles.HexNumber);
        }
        return arr;
    }

    public static string ToHexString(Span<byte> span)
    {
        var str = new StringBuilder(span.Length * 2);
        foreach (var b in span)
        {
            str.Append(b.ToString("X2"));
        }

        return str.ToString();
    }
}