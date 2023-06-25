using CalTp.TransportProtocol;
using Serilog;
using Version = CommonTypes.Version;

namespace CalTp.Bootloader;

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
    private const uint FlashCommandTimeoutMs = 10000;
    private const int PingTimeoutMs = 1000;
    private const int DefaultTimeoutMs = 500;
    private const int AckTimeoutMs = 1000;
    private const int AttemptCounter = 3;
    private readonly ILogger _logger;
    private readonly ITransportProtocol _tp;

    public FblCommands(ILogger logger, ITransportProtocol tp) {
        _logger = logger;
        _tp = tp;
    }

    public Version FblVersion { get; private set; }
    public ushort Options { get; private set; }

    /// <summary>
    /// Ping is special case that dont follow normal format
    /// </summary>
    /// <returns></returns>
    public async Task<ResponseCode> Ping() {
        return await SendQueryAsync(BuildFramingPacket(PacketType.Ping), PingResponseLen,
            x => {
                (FblVersion, Options) = ParsePingResponse(x);
                return ResponseCode.Success;
            }, PingTimeoutMs);
    }


    public async Task<ResponseCode> FLashEraseAll(uint memoryId = 0) {
        return await CommandNoData(new Command(CommandType.SetProperty, false,
            new[] {memoryId}), FlashCommandTimeoutMs);
    }


    public async Task<ResponseCode> FlashEraseRegion(uint startAddress, uint byteCount, uint memoryId = 0) {
        return await CommandNoData(new Command(CommandType.SetProperty, false,
            new[] {startAddress, byteCount, memoryId}), FlashCommandTimeoutMs);
    }

    public async Task<(ResponseCode, byte[])> ReadMemory(uint startAddress,uint byteCount) {
        var resp = await CommandDataIn(new Command(CommandType.SetProperty, true,
            new[] {startAddress, byteCount}));
        return (resp.status,resp.data);
    }


    public async Task<ResponseCode> WriteMemory(uint startAddress, byte[] bytes) {
        return await CommandDataOut(new Command(CommandType.SetProperty, true,
            new[] {startAddress, (uint) bytes.Length}),bytes);
    }


    public async Task<ResponseCode> FLashSecurityDisable(ulong key) {
        return await CommandNoData(new Command(CommandType.FlashSecurityDisable, false,
            new[] {(uint) (key >> 32),(uint) (key & 0xffffffff)}));
    }

    public async Task<(ResponseCode, uint)> GetProperty(PropertyTag property) {
        await SendCommand(new Command(CommandType.GetProperty, false, new[] {(uint) property}));

        return await GetPropertyResponse();
    }


    public async Task<ResponseCode> Execute(uint jumpAddr, uint arg, uint stackPtrAddr) {
        return await CommandNoData(new Command(CommandType.Execute, false, new[] {jumpAddr, arg, stackPtrAddr}));
    }

    public async Task<ResponseCode> Reset() {
        return await CommandNoData(new Command(CommandType.Reset, false, Array.Empty<uint>()));
    }

    public async Task<ResponseCode> SetProperty(PropertyTag property, uint value) {
        var x = await CommandNoData(new Command(CommandType.SetProperty, false, new[] {(uint) property, value}));

        var y = await GetPropertyResponse();
        if (y.status != ResponseCode.Success)
            return y.status;
        return y.value == value ? ResponseCode.Success : ResponseCode.Fail;
    }

    public async Task<ResponseCode> FlashEraseAllUnsecure() {
        return await CommandNoData(new Command(CommandType.FlashEraseAllUnsecure, false, Array.Empty<uint>()));
    }
}