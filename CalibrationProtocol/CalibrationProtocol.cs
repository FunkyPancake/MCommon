using CalTp.Bootloader;
using CalTp.TransportProtocol;
using CalTp.TransportProtocol.Tp;
using CommonTypes;
using IntelHex;
using Serilog;
using Version = CommonTypes.Version;

namespace CalTp;

public enum CmdStatus {
    Ok,
}

public class CalibrationProtocol {
    private readonly FblCommands _fblCommands;
    private readonly ILogger _logger;
    private readonly ITransportProtocol _tp;
    private bool _inBootloader = false;

    public CalibrationProtocol(ILogger logger, ITransportProtocol tp) {
        _logger = logger;
        _tp = tp;
    }

    //Constructor used for CAN tp
    public CalibrationProtocol(ILogger logger, (uint rx, uint tx) frame) {
        _logger = logger;
        _tp = new CanTp(frame.rx, frame.tx);
        _fblCommands = new FblCommands(logger, _tp);
    }

    public bool ConnectionStatus { get; private set; } = false;

    public async Task<CmdStatus> JumpToFbl() {
        try {
            if (await _fblCommands.Ping() != ResponseCode.Success) {
                _logger.Error("Cannot connect to the target.");
            }
        }
        catch (BootloaderExceptions e) {
            _logger.Error("Command failed, error {}", e.Message);
            _inBootloader = false;
            return CmdStatus.Ok;
        }

        _logger.Information("Connection successful.");
        _logger.Information("Bootloader version = {0}", _fblCommands.FblVersion);
        _inBootloader = true;
        return CmdStatus.Ok;
    }

    public async Task<CmdStatus> Program(Hex swPackageHex) {
        if (ConnectionStatus && _inBootloader) {
            // await _tp.QueryAsync(BuildCommand(Command.Program), 3);
            return (CmdStatus) 0;
        }

        return CmdStatus.Ok;
    }

    public async Task<CmdStatus> Connect() {
        _tp.Connect();
        // var status = await _tp.QueryAsync(BuildCommand(Command.Connect), 1, new CancellationToken(), 1);
        // if (status.Status != TpStatus.Ok) {
        //     ConnectionStatus = false;
        //     _logger.Error("");
        //     return CmdStatus.Ok;
        // }

        ConnectionStatus = true;
        return CmdStatus.Ok;
    }


    public async Task<CmdStatus> Disconnect() {
        // await _tp.QueryAsync(BuildCommand(Command.Disconnect), 1);
        _tp.Disconnect();
        ConnectionStatus = false;
        return (CmdStatus) 0;
    }

    public async Task<CmdStatus> Reset() {
        // var status = await _tp.QueryAsync(BuildCommand(Command.Reset), 1);
        // return (CmdStatus) status.Status;
        return CmdStatus.Ok;
    }

    public async Task<(CmdStatus, byte[])> ReadMemory(uint addr, uint size) {
        var addressBytes = GetAddressBytes(addr);
        var sizeBytes = GetSizeBytes((ushort) size);
        var payload = addressBytes.Concat(sizeBytes).ToArray();
        // var status =
            // await _tp.QueryAsync();
                // BuildCommand(Command.ReadMemory, payload), (byte) size + 1);

        // return ((CmdStatus, byte[])) (status.Status, status.Data);
        return (CmdStatus.Ok, Array.Empty<byte>());

    }

    public async Task<CmdStatus> WriteMemory(uint addr, byte[] data) {
        var addressBytes = GetAddressBytes(addr);
        var sizeBytes = GetSizeBytes((ushort) data.Length);
        var payload = addressBytes.Concat(sizeBytes).Concat(data).ToArray();
        // var status =
            // await _tp.QueryAsync(
                // BuildCommand(Command.WriteMemory, payload), 1);

        // return (CmdStatus) status.Status;
        return CmdStatus.Ok;
    }

    public async Task<CmdStatus> ConfigureCyclicReadBlock(int readFrequency, int size, Tuple<uint, uint>[] blockDesc) {
        return (CmdStatus) 0;
    }

    public async Task<CmdStatus> StartCyclicRead() {
        return (CmdStatus) 0;
    }

    public async Task<CmdStatus> StopCyclicRead() {
        return (CmdStatus) 0;
    }

    public async Task<CmdStatus> ClearReadBlockConfig() {
        return (CmdStatus) 0;
    }

    public async Task<CmdStatus> GetControlBlock() {
        return (CmdStatus) 0;
    }

    private async Task<CmdStatus> ProcessCommand() {
        return (CmdStatus) 0;
    }

    protected void OnCyclicDataRead() {
    }

    private byte[] BuildCommand(Command command, byte[]? payload = null) {
        if (payload == null) {
            return new[] {(byte) command};
        }

        var data = new byte[payload.Length + 1];
        data[0] = (byte) command;
        Buffer.BlockCopy(payload, 0, data, 1, payload.Length);
        return data;
    }

    private CmdStatus ProcessCommand(Command cmd) {
        return 0;
    }

    private byte[] GetAddressBytes(uint value) {
        return BitConverter.GetBytes(value);
    }

    private byte[] GetSizeBytes(ushort value) {
        return BitConverter.GetBytes(value);
    }

    public async Task<EcuIdent> GetEcuIdent() {
        throw new NotImplementedException();
    }

    public async Task<Version> GetSwVersion() {
        throw new NotImplementedException();
    }
}