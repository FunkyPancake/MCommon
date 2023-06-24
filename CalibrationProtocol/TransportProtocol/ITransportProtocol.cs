namespace CalTp.TransportProtocol;

public interface ITransportProtocol {
    IEnumerable<int> GetAvailableChannels();
    bool Connect();
    void Disconnect();
    Task<byte[]> QueryAsync(byte[] request, int responseLength, CancellationToken token, int pingTimeoutMs);
    event EventHandler? OnNewAsyncMessage;
    Task<byte[]> QueryAsync(byte[] request, int pingRespLen, CancellationToken token);
    Task<byte[]> ReadAsync(int count, int byteCount);
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