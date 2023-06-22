namespace CommonTypes;

public struct EcuIdent {
    public EcuIdent(string ecuName, Version hwVersion) {
        EcuName = ecuName;
        HwVersion = hwVersion;
    }

    public string EcuName { get; }
    public Version HwVersion { get; }
}