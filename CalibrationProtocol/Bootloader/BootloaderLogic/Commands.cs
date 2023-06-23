using CalTp.TransportProtocol;
using Serilog;
using Version = CommonTypes.Version;

namespace CalTp.Bootloader.BootloaderLogic;

public class Commands {
    private const int PingTimeoutMs = 1000;
    private const int CommandTimeoutMs = 500;
    private const byte StartByte = 0x5A;
    private const int AckTimeoutMs = 1000;
    private readonly ILogger _logger;
    private readonly ITransportProtocol _tp;
    private ushort Options;
    private const int AttemptCounter = 3;

    public CommonTypes.Version FblVersion { get; private set; }

    public Commands(ILogger logger, ITransportProtocol tp) {
        _logger = logger;
        _tp = tp;
    }

    #region Bootloader Commands

    public async Task<bool> Ping() {
        const int respLen = 10;
        var cancellationToken = CancellationToken.None;
        var task = _tp.QueryAsync(PacketWrapper.BuildFramingPacket(PacketType.Ping), respLen, cancellationToken);
        if (await Task.WhenAny(task, Task.Delay(PingTimeoutMs, cancellationToken)) == task &&
            task.IsCompletedSuccessfully) {
            var response = await task;
            var crc = response[respLen - 2] + (response[respLen - 1] << 8);
            if (response[0] != StartByte || response[1] != (byte) PacketType.PingResponse ||
                crc != CalcCrc(response[..(respLen - 2)])) {
                _logger.Error("");
                return false;
            }

            FblVersion = new Version(response[4], response[3], response[2]);
            Options = (ushort) ((response[7] << 8) + response[6]);
            return true;
        }

        _logger.Error("");
        return false;
    }

    public void FLashEraseAll() {
    }

    public void FlashEraseRegion() {
    }

    public void ReadMemory() {
    }

    public void WriteMemory() {
    }

    //Supported when security enabled
    public void FLashSecurityDisable() {
    }

    public void GetProperty() {
    }

    public async Task<bool> Execute(uint jumpAddr, uint arg, uint stackPtrAddr) {
        await SendCommand(new Command(CommandType.Execute, false, new[] {jumpAddr, arg, stackPtrAddr}));
        return await GetGenericResponse(CommandType.Execute) == ResponseCode.Success;
        
    }

    public void Reset() {
        CommandNoData(new Command(CommandType.Reset, false, Array.Empty<uint>()));
    }

    public void SetProperty() {
    }

    public void FlashEraseAllUnsecure() {
    }

    #endregion

    #region Commands Implementation

    private bool ProcessCommandNoData() {
        return false;
    }


    private async Task<ResponseCode> CommandNoData(Command command) {
        SendCommand(command);
        SendCommand(command);
        // WaitForAck();

        var resp = ResponseCode.Fail;
        for (var i = AttemptCounter; i > 0; i--) {
            // resp = GetGenericResponse(command.Type);
            // if (resp != ResponseCode.Success) {
                // SendNack();
            // }
            // else {
                // SendAck();
                // break;
            // }
        }

        return resp;
    }

    private async Task SendCommand(Command command) {
        var request = PacketWrapper.BuildCommandPacket(command);
        _logger.Warning("{0}",request);
        await _tp.WriteAsync(request);
    }

    private void SendNack() {
        throw new NotImplementedException();
    }

    private async Task<ResponseCode> GetGenericResponse(CommandType commandType) {
        var status = PacketWrapper.ParseGenericResponse(await _tp.ReadAsync(PacketWrapper.GenericResponseLen), out var tag);
        if (commandType != tag) {
            _logger.Error("Command tag mismatch. Expected {0}, received {1}", commandType, tag);
            return ResponseCode.Fail;
        }

        return status;
    }

    private async Task<bool> SendCommandsGetAck(byte[] request) {
        await _tp.QueryAsync(request, 2, new CancellationToken(), AckTimeoutMs);
        return true;
    }

    private async Task<byte[]> WaitForResponseAndAck(int responseLength) {
        return new byte[responseLength];
    }


    private async void SendAck() {
        await _tp.WriteAsync(PacketWrapper.BuildFramingPacket(PacketType.Ack));
    }

    private async Task<bool> GetAck() {
        return PacketWrapper.ParseAck(await _tp.ReadAsync(2));
    }

    private ResponseCode GetResponseCode(Command response) {
        return (ResponseCode) response.Parameters[0];
    }

    private static ushort CalcCrc(IReadOnlyList<byte> packet) {
        uint crc = 0;
        uint j;
        for (j = 0; j < packet.Count; ++j) {
            uint i;
            uint b = packet[(int) j];
            crc ^= b << 8;
            for (i = 0; i < 8; ++i) {
                var temp = crc << 1;
                if ((crc & 0x8000) == 0x8000) {
                    temp ^= 0x1021;
                }

                crc = temp;
            }
        }

        return (ushort) crc;
    }

    #endregion
}