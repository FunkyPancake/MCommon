namespace CommonTypes;

public readonly struct Version {
    private uint Major { get; }
    private uint Minor { get; }
    private uint Patch { get; }

    public override string ToString() {
        return $"{Major}.{Minor}.{Patch}";
    }

    public Version(uint major, uint minor, uint patch) {
        Major = major;
        Minor = minor;
        Patch = patch;
    }

    public Version(string str) {
        var split = str.Split('.');
        Major = uint.Parse(split[0]);
        Minor = uint.Parse(split[1]);
        Patch = uint.Parse(split[2]);
    }

    public static bool operator >(Version a, Version b) {
        if (a.Major > b.Major)
            return true;
        if (a.Major == b.Major) {
            if (a.Minor > b.Minor)
                return true;
            if (a.Minor == b.Minor) {
                return a.Patch > b.Patch;
            }
        }

        return false;
    }

    public static bool operator <(Version a, Version b) {
        if (a.Major < b.Major)
            return true;
        if (a.Major == b.Major) {
            if (a.Minor < b.Minor)
                return true;
            if (a.Minor == b.Minor) {
                return a.Patch < b.Patch;
            }
        }

        return false;
    }
    public static bool operator ==(Version a, Version b) {
        return a.Major == b.Major && a.Minor == b.Minor && a.Patch == b.Patch;
    }

    public static bool operator !=(Version a, Version b) {
        return a.Major != b.Major || a.Minor != b.Minor || a.Patch != b.Patch;
    }
}