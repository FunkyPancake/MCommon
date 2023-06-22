namespace CalTp.TransportProtocol;

public interface ITransportProtocol {
    IEnumerable<int> GetAvailableChannels();
    bool Connect();
    void Disconnect();
    Task<(TpStatus Status, byte[] Data)> QueryAsync(byte[] command, int responseLength);

    event EventHandler? OnNewAsyncMessage;
    void Send(byte[] cmd);
    byte[] GetBytes(int i, int timeout);
}

public enum TpStatus {
    Ok,
    NotConnected,
    Timeout,
    DeviceError,
    InvalidLength,
    InvalidMsgCounter,
}