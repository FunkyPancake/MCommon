using Serilog;

namespace CalTp;

public class CalTp {
    private readonly  _tp;
    private ILogger _logger;

    public CalTp(Tp tp, ILogger logger) {
        _tp = tp;
        _logger = logger;
        throw new NotImplementedException();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task<bool> Connect() {
        _tp.Connect();
        var status = await _tp.Query(BuildCommand(Command.Connect), 1);
        if (status.Status != TpStatus.Ok) {
            ConnectionStatus = false;
            _logger.Error("");
            return CmdStatus.Ok;
        }
        ConnectionStatus = true;
        return CmdStatus.Ok;
    }
    /// <summary>
    /// 
    /// </summary>
    public void Disconnect() {
        _commands.Reset();
        _isConnected = false;
    }

    /// <summary>
    /// 
    /// </summary>
    public void GetSoftwareVersion() {
        if (!_isConnected) {
            _logger.Error("GetSoftwareVersion() - Target not connected.");
            return;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool Program() {
        if (!_isConnected) {
            _logger.Error("GetSoftwareVersion() - Target not connected.");
            return false;
        }

        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool Verify() {
        if (!_isConnected) {
            _logger.Error("GetSoftwareVersion() - Target not connected.");
            return false;
        }

        return true;
    }
}
