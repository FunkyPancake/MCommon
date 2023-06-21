namespace CalTp.Bootloader;

public interface ITransportProtocol
{
    void Send(byte[] cmd);
    byte[] GetBytes(int i, int timeout);
}