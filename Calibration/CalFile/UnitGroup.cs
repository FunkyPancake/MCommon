using System.Xml.Serialization;

namespace Calibration.CalFile;

public class UnitGroup : IUnitGroup {
    public Type Type { get; init; }

    public UnitGroup() {
        Id = Guid.NewGuid();
        Format = "";
        Type = typeof(string);
    }

    public UnitGroup(Guid id,string formatString) {
        Id = Guid.NewGuid();
        Format = formatString;
    }

    [XmlAttribute] public Guid Id { get; init; }

    [XmlElement] public string Format { get; init; }

    public uint GetRaw() {
        return 0;
    }

    public float GetPhys() {
        return 1;
    }

    public string GetString(uint value) {
        return string.Format(Format, value);
    }
}