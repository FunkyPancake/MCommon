namespace IntelHex;

internal record LinearMemoryArea(uint Offset)
{
    public readonly uint Offset = Offset;
    public readonly List<byte> Bytes = new();
}