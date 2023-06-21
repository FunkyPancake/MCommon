using Serilog;

namespace CalTp.Bootloader.BootloaderLogic;

internal class Commands {
    private const int PingTimeoutMs = 1000;
    private const int CommandTimeoutMs = 500;

    private const int AckTimeoutMs = 1000;
    private readonly ILogger _logger;
    private readonly ITransportProtocol _tp;

    public Commands(ILogger logger, ITransportProtocol tp) {
        _logger = logger;
        _tp = tp;
    }

    public void Execute(uint jumpAddr, uint arg, uint stackPtrAddr) {
        var cmd = new[] {jumpAddr, arg, stackPtrAddr};
    }

    public void FLashEraseAll() {
    }

    public void FlashEraseRegion() {
    }

    public void WriteMemory() {
    }

    public void ReadMemory() {
    }

    //Supported when security enabled
    public void FLashSecurityDisable() {
    }

    public void GetProperty() {
    }

    public void Reset() {
        CommandNoData(new CommandPacket(Command.Reset, false, Array.Empty<uint>()));
    }

    public void SetProperty() {
    }

    public void FlashEraseAllUnsecure() {
    }

    private bool ProcessCommandNoData() {
        return false;
    }

    public bool Ping(out SoftwareVersion pingData) {
        pingData = null!;
        _tp.Send(PacketWrapper.BuildFramingPacket(PacketType.Ping));
        var status = PacketWrapper.ParsePingResponse(_tp.GetBytes(10, PingTimeoutMs), out var response);
        if (status) {
            pingData = new SoftwareVersion(Major: response[2], Minor: response[1], Bugfix: response[0]);
        }

        return status;
    }

    private ResponseCode CommandNoData(CommandPacket command) {
        _tp.Send(PacketWrapper.BuildCommandPacket(command));
        GetAck();
        var response = PacketWrapper.ParseCommandPacket(_tp.GetBytes(18, 0));
        if ((Command) response.Parameters[1] != command.Type) {
            _logger.Error("Response command tag mismatch, request: {}response:{}", command.Type,
                response.Parameters[1]);
            return ResponseCode.Fail;
        }

        SendAck();
        return GetResponseCode(response);
    }


    private void SendAck() {
        _tp.Send(PacketWrapper.BuildFramingPacket(PacketType.Ack));
    }

    private bool GetAck() {
        return PacketWrapper.ParseAck(_tp.GetBytes(2, AckTimeoutMs));
    }

    private ResponseCode GetResponseCode(CommandPacket response) {
        return (ResponseCode) response.Parameters[0];
    }
}