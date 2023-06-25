namespace CalTp.TransportProtocol;

public interface ITransportProtocol {
    IEnumerable<int> GetAvailableChannels();
    bool Connect();
    void Disconnect();
    event EventHandler? OnNewAsyncMessage;
    Task<(TpStatus status, byte[] data)> ReadAsync(int count,uint timeout,CancellationToken token = default);
    Task WriteAsync(byte[] data);

}

public enum TpStatus {
    Ok,
    NotConnected,
    Timeout,
    DeviceError,
    InvalidLength,
    InvalidMsgCounter,
}