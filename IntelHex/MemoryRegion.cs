namespace IntelHex;

internal class MemoryRegion
{
    public uint StartAddress { get; set; }

    public readonly IList<LinearMemoryArea> MemoryAreas;
    private uint _currentAddress;
    public MemoryRegion(uint startAddress)
    {
        StartAddress = startAddress;
        MemoryAreas = new List<LinearMemoryArea>();
    }
    public MemoryRegion()
    {
        StartAddress = 0;
        MemoryAreas = new List<LinearMemoryArea>();
    }
    internal void AddData(HexEntry hexEntry)
    {
        if ((_currentAddress & 0xffff) != hexEntry.Address || MemoryAreas.Count == 0)
        {
            MemoryAreas.Add(new LinearMemoryArea(hexEntry.Address));
            _currentAddress = hexEntry.Address;
        }
        MemoryAreas.Last().Bytes.AddRange(hexEntry.Data);
        _currentAddress += hexEntry.ByteCount;
    }
}