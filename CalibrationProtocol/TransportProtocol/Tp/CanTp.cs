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

    public event EventHandler? OnNewAsyncMessage;
    public Task<byte[]> ReadAsync(int count, int pingTimeoutMs, CancellationToken token) {
        throw new NotImplementedException();
    }

    public Task<(TpStatus status, byte[] data)> ReadAsync(int count, uint timeout, CancellationToken token = default) {
        throw new NotImplementedException();
    }

    public Task WriteAsync(byte[] data) {
        throw new NotImplementedException();
    }
}