using CalTp.TransportProtocol;
using Serilog;
using Version = CommonTypes.Version;

namespace CalTp.Bootloader.BootloaderLogic;

public class Commands {
    private const int PingTimeoutMs = 1000;
    private const int CommandTimeoutMs = 500;
    private const byte StartByte = 0x5A;
    private const int AckTimeoutMs = 1000;
    private const int AttemptCounter = 3;
    private readonly ILogger _logger;
    private readonly ITransportProtocol _tp;
    private ushort Options;

    public Commands(ILogger logger, ITransportProtocol tp) {
        _logger = logger;
        _tp = tp;
    }

    public CommonTypes.Version FblVersion { get; private set; }

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

    public async Task<ResponseCode> FLashEraseAll(uint memoryId = 0) {
        return await CommandNoData(new Command(CommandType.SetProperty, false,
            new[] {memoryId}));
    }

    public async Task<ResponseCode> FlashEraseRegion(uint startAddress, uint byteCount, uint memoryId = 0) {
        return await CommandNoData(new Command(CommandType.SetProperty, false,
            new[] {startAddress, byteCount, memoryId}));
    }

    public async Task<(ResponseCode,byte[])> ReadMemory() {
         await CommandDataIn();
         return (ResponseCode.Fail, new byte[]{});
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
            resp = await GetGenericResponse(command.Type);
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
        // throw new NotImplementedException();
    }

    private async Task CommandDataIn() {
        throw new NotImplementedException();
    }

    private async Task SendCommand(Command command) {
        var request = PacketWrapper.BuildCommandPacket(command);
        _logger.Warning("{0}", request);
        await _tp.WriteAsync(request);
    }

    private void SendNack() {
        // throw new NotImplementedException();
    }

    private async Task<ResponseCode> GetGenericResponse(CommandType commandType) {
        var status =
            PacketWrapper.ParseGenericResponse(await _tp.ReadAsync(PacketWrapper.GenericResponseLen), out var tag);
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

    private async Task<ResponseCode> CommandDataOut() {
        throw new NotImplementedException();
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

public enum PropertyTag {
}