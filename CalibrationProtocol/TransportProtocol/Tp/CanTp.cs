namespace CalTp.TransportProtocol.Tp; 

public class CanTp : ITransportProtocol {
    public CanTp(uint idRx, uint idTx) {
        
    }

    public IEnumerable<int> GetAvailableChannels() {
        throw new NotImplementedException();
    }

    public bool Connect() {
        throw new NotImplementedException();
    }

    public void Disconnect() {
        throw new NotImplementedException();
    }

    public Task<byte[]> QueryAsync(byte[] request, int responseLength, CancellationToken token, int pingTimeoutMs) {
        throw new NotImplementedException();
    }

    public event EventHandler? OnNewAsyncMessage;
    public Task<byte[]> QueryAsync(byte[] request, int pingRespLen, CancellationToken token) {
        throw new NotImplementedException();
    }

    public Task<byte[]> ReadAsync(int count, int byteCount) {
        throw new NotImplementedException();
    }

    public Task WriteAsync(byte[] data) {
        throw new NotImplementedException();
    }
}