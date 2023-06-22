namespace CommonTypes;

public readonly struct Version {
    private readonly uint _minor;
    private readonly uint _patch;

    public uint Major { get; }

    public override string ToString() {
        return $"{Major}.{_minor}.{_patch}";
    }

    public Version(uint major, uint minor, uint patch) {
        Major = major;
        _minor = minor;
        _patch = patch;
    }

    public Version(string str) {
        var split = str.Split('.');
        Major = uint.Parse(split[0]);
        _minor = uint.Parse(split[1]);
        _patch = uint.Parse(split[2]);
    }
}