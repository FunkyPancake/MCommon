namespace CalTp.TransportProtocol.Tp;

public record SerialTpConfig {
    public string ComPort { get; init; }
    public int Baudrate { get; init; }
    public int CommunicationTimeout { get; init; }
}