using CalTp.TransportProtocol;
using Serilog;
using Version = CommonTypes.Version;

namespace CalTp.Bootloader.BootloaderLogic;

/*TODO:
    - Write Memory high level
    - Read Memory low level
    - Parse Parameter Response
    - Repeat counter
    - error handling
    - Command data in
    - Command data out
    - Command no data
    - testy wszystkich komend
    

*/
public partial class FblCommands {
    private const int PingTimeoutMs = 1000;
    private const int DefaultTimeoutMs = 500;
    private const int AckTimeoutMs = 1000;
    private const int AttemptCounter = 3;
    private readonly ILogger _logger;
    private readonly ITransportProtocol _tp;
    private ushort Options;

    public FblCommands(ILogger logger, ITransportProtocol tp) {
        _logger = logger;
        _tp = tp;
    }

    public Version FblVersion { get; private set; }

    #region Bootloader Commands

    /// <summary>
    /// Ping is special case that dont follow normal format
    /// </summary>
    /// <returns></returns>
    public async Task<ResponseCode> Ping() {
        return await SendQueryAsync(BuildFramingPacket(PacketType.Ping), PingResponseLen, PingTimeoutMs,
            x => {
                (FblVersion, Options) = ParsePingResponse(x);
                return ResponseCode.Success;
            });
    }


    public async Task<ResponseCode> FLashEraseAll(uint memoryId = 0) {
        return await CommandNoData(new Command(CommandType.SetProperty, false,
            new[] {memoryId}));
    }

    public async Task<ResponseCode> FlashEraseRegion(uint startAddress, uint byteCount, uint memoryId = 0) {
        return await CommandNoData(new Command(CommandType.SetProperty, false,
            new[] {startAddress, byteCount, memoryId}));
    }

    public async Task<(ResponseCode, byte[])> ReadMemory() {
        await CommandDataIn();
        return (ResponseCode.Fail, new byte[] { });
    }


    public async Task<ResponseCode> WriteMemory() {
        return await CommandDataOut();
    }


    public async Task<ResponseCode> FLashSecurityDisable(ulong key) {
        return await CommandNoData(new Command(CommandType.SetProperty, false,
            new[] {(uint) (key & 0xffffffff), (uint) (key >> 32)}));
    }

    public async Task<(ResponseCode, uint)> GetProperty(PropertyTag property) {
        await SendCommand(new Command(CommandType.GetProperty, false, new[] {(uint) property}));
        await GetGetPropertyResponse();
        return (ResponseCode.Fail, 0);
    }


    public async Task<ResponseCode> Execute(uint jumpAddr, uint arg, uint stackPtrAddr) {
        return await CommandNoData(new Command(CommandType.Execute, false, new[] {jumpAddr, arg, stackPtrAddr}));
    }

    public async Task<ResponseCode> Reset() {
        return await CommandNoData(new Command(CommandType.Reset, false, Array.Empty<uint>()));
    }

    public async Task<ResponseCode> SetProperty(PropertyTag property, int value) {
        return await CommandNoData(new Command(CommandType.SetProperty, false, Array.Empty<uint>()));
    }

    public async Task<ResponseCode> FlashEraseAllUnsecure() {
        return await CommandNoData(new Command(CommandType.FlashEraseAllUnsecure, false, Array.Empty<uint>()));
    }

    #endregion

    #region Commands Implementation

    private async Task<ResponseCode> GetGetPropertyResponse() {
        // throw new NotImplementedException();
        return ResponseCode.Fail;
    }

    private bool ProcessCommandNoData() {
        return false;
    }


    private async Task<ResponseCode> CommandNoData(Command command) {
        await SendCommand(command);
        await WaitForAck();

        var resp = ResponseCode.Fail;
        for (var i = AttemptCounter; i > 0; i--) {
            resp = await GetGenericResponse(command.Type, 1);
            if (resp is ResponseCode.AppCrcCheckOutOfRange or ResponseCode.Fail or ResponseCode.Timeout) {
                SendNack();
            }
            else {
                SendAck();
                break;
            }
        }

        return resp;
    }

    private async Task WaitForAck() {
        _tp.ReadAsync(2, AckTimeoutMs);
    }

    private async Task CommandDataIn() {
        throw new NotImplementedException();
    }

    private async Task<ResponseCode> SendQueryAsync(byte[] requestBy, int responseLen, int timeout,
        Func<byte[], ResponseCode> action) {
        var cancellationToken = CancellationToken.None;
        var task = _tp.QueryAsync(BuildFramingPacket(PacketType.Ping), responseLen,
            cancellationToken);
        if (await Task.WhenAny(task, Task.Delay(PingTimeoutMs, cancellationToken)) == task &&
            task.IsCompletedSuccessfully) {
            var response = await task;
            return action(response);
        }

        return ResponseCode.Timeout;
    }

    private async Task SendCommand(Command command) {
        var request = BuildCommandPacket(command);
        _logger.Warning("{0}", request);
        await _tp.WriteAsync(request);
    }

    private void SendNack() {
        await _tp.WriteAsync(BuildFramingPacket(PacketType.Nak));
    }

    private async void SendAck() {
        await _tp.WriteAsync(BuildFramingPacket(PacketType.Ack));
    }

    private async Task<ResponseCode> GetGenericResponse(CommandType commandType, int timeout = DefaultTimeoutMs) {
        var status =
            ParseGenericResponse(await _tp.ReadAsync(GenericResponseLen, timeout),
                out var tag);
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


    private ResponseCode GetResponseCode(Command response) {
        return (ResponseCode) response.Parameters[0];
    }

    private async Task<ResponseCode> CommandDataOut() {
        throw new NotImplementedException();
    }

    #endregion
}

public enum PropertyTag {
}