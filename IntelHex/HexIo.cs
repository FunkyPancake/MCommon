using System.Text.RegularExpressions;

namespace IntelHex;

public static class HexIo {
    private static readonly Regex HexLineRegex =
        new(
            ":(?<byteCount>[0-9A-F]{2})(?<address>[0-9A-F]{4})(?<recordType>[0-9A-F]{2})(?<data>[0-9A-F]*)(?<checksum>[0-9A-F]{2}$)",
            RegexOptions.Compiled);

    public static Hex Read(string fileName) {
        var hex = new Hex();
        var lines = File.ReadAllLines(fileName);
        foreach (var line in lines) {
            ParseHexLine(hex, line);
        }

        return hex;
    }

    public static async Task<Hex> ReadAsync(Stream stream) {
        var hex = new Hex();
        stream.Seek(0, SeekOrigin.Begin);
        var streamReader = new StreamReader(stream);

        while (await streamReader.ReadLineAsync() is { } line) {
            ParseHexLine(hex, line);
        }

        return hex;
    }

    public static void Write(Hex hex, string fileName) {
        var file = new StreamWriter(fileName);
        foreach (var memoryRegion in hex.MemoryRegions) {
            file.WriteLine(string.Join("\r\n", GetMemoryRegionAsString(memoryRegion)));
        }

        var entryNoCrc = $"04000003{hex.EntryAddress:X8}";
        file.WriteLine($":{entryNoCrc}{CalcCrc(entryNoCrc)}");
        file.WriteLine(":00000001FF");
        file.Close();
    }

    private static void ParseHexLine(Hex hex, string line) {
        var hexEntry = new HexEntry(HexLineRegex.Match(line));

        switch (hexEntry.RecordType) {
            case 0:
                if (hex.MemoryRegions.Count == 0) {
                    hex.MemoryRegions.Add(new MemoryRegion(hexEntry.Address));
                }

                hex.MemoryRegions.Last().AddData(hexEntry);
                break;

            case 2:
                var x = (uint) BitConverter.ToUInt16(hexEntry.Data.Reverse().ToArray());
                hex.MemoryRegions.Add(new MemoryRegion(x * 16));
                break;

            case 3:
                hex.EntryAddress = BitConverter.ToUInt32(hexEntry.Data.Reverse().ToArray());
                break;

            case 4:
                if (hex.MemoryRegions.Count != 0 && (hex.MemoryRegions.Last().StartAddress & 0xffff) != 0) {
                    hex.MemoryRegions.Last().StartAddress +=
                        (uint) BitConverter.ToUInt16(hexEntry.Data.Reverse().ToArray()) << 16;
                }
                else {
                    hex.MemoryRegions.Add(
                        new MemoryRegion((uint) BitConverter.ToUInt16(hexEntry.Data.Reverse().ToArray()) << 16));
                }

                break;
        }
    }

    private static IEnumerable<string> GetMemoryRegionAsString(MemoryRegion mem) {
        var memoryString = new List<string>();

        memoryString.AddRange(GetRegionStartAddressAsString(mem));

        foreach (var memoryArea in mem.MemoryAreas) {
            memoryString.AddRange(GetLinearMemoryAreaAsString(memoryArea));
        }

        return memoryString;
    }

    private static IEnumerable<string> GetLinearMemoryAreaAsString(LinearMemoryArea memoryArea) {
        var lines = new List<string>();
        var data = memoryArea.Bytes.ToArray();
        var currentAddress = memoryArea.Offset;
        var currentDataIndex = 0;
        for (var i = data.Length; i > 0;) {
            var decBytes = i > 0x10 ? 0x10 : i;
            var hexString =
                HexStringConverter.ToHexString(data.AsSpan()[currentDataIndex..(currentDataIndex + decBytes)]);
            var lineNoCrc =
                $"{decBytes:X2}{currentAddress:X4}00{hexString}";
            AppendLine(lines, lineNoCrc);
            currentAddress += (uint) decBytes;
            currentDataIndex += decBytes;
            i -= decBytes;
        }

        return lines;
    }

    private static IEnumerable<string> GetRegionStartAddressAsString(MemoryRegion mem) {
        var lines = new List<string>();
        switch (mem.StartAddress) {
            case <= 0x10000:
                return lines;
            case > 0xffff * 16: {
                AppendLine(lines, "020000020000");
                AppendLine(lines, $"02000004{mem.StartAddress >> 16:X4}");
                break;
            }
            default: {
                AppendLine(lines, $"02000002{(mem.StartAddress & 0xffff0000) / 16:X4}");
                break;
            }
        }

        return lines;
    }

    private static void AppendLine(ICollection<string> lines, string lineNoCrc) {
        lines.Add($":{lineNoCrc}{CalcCrc(lineNoCrc)}");
    }

    private static string CalcCrc(ReadOnlySpan<char> lineNoCrc) {
        var sum = HexStringConverter.ToByteArray(lineNoCrc)
            .Aggregate(0, (current, b) => current + b);
        return $"{(byte) (0x100 - sum):X2}";
    }
}