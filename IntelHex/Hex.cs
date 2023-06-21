using System.Collections.ObjectModel;
namespace IntelHex;

public class Hex
{
    internal uint EntryAddress;
    internal readonly ICollection<MemoryRegion> MemoryRegions = new Collection<MemoryRegion>();

    public void SetMemoryRange(uint address, byte[] memory)
    {
        var memoryArea = GetMemoryArea(address);
        var startIndex = (int)((address & 0xffff) - memoryArea.Offset);
        for (var i = 0; i < memory.Length; i++)
        {
            memoryArea.Bytes[startIndex + i] = memory[i];
        }
    }

    public Span<byte> GetMemoryRange(uint address, uint size)
    {
        var memoryArea = GetMemoryArea(address);
        var startIndex = (int)((address & 0xffff) - memoryArea.Offset);
        return memoryArea.Bytes.ToArray().AsSpan().Slice(startIndex, (int)size);
    }

    private LinearMemoryArea GetMemoryArea(uint address)
    {
        foreach (var memoryRegion in MemoryRegions)
        {
            if (memoryRegion.StartAddress > address)
                break;
            foreach (var memoryArea in memoryRegion.MemoryAreas)
            {
                var sectionStart = memoryRegion.StartAddress + memoryArea.Offset;
                var sectionEnd = sectionStart + memoryArea.Bytes.Count;
                if (sectionStart > address)
                    break;
                if (sectionEnd < address)
                    continue;
                return memoryArea;
            }
        }

        throw new InvalidOperationException();
    }
}