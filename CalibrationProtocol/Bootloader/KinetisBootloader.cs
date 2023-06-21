using CalTp.Bootloader.BootloaderLogic;
using Serilog;

namespace CalTp.Bootloader;

/// <summary>
/// 
/// </summary>
public class KinetisBootloader {
    private readonly ILogger _logger;
    private readonly Commands _commands;
    private bool _isConnected = false;
    private bool _isFileLoaded = false;

    /// <summary>
    /// 
    /// </summary>
    public CommonTypes.Version BootloaderVersion { get; private set; } = new(0, 0, 0);

    public SoftwareVersion ApplicationVersion { get; private set; } = new(0, 0, 0);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="tp"></param>
    public KinetisBootloader(ILogger logger, ITransportProtocol tp) {
        _logger = logger;
        _commands = new Commands(logger, tp);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool Connect() {
        SoftwareVersion version;
        try {
            if (!_commands.Ping(out version)) {
                _logger.Error("Cannot connect to the target.");
            }
        }
        catch (CommandFailedException e) {
            _logger.Error("Command failed, error {}", e.Message);
            _isConnected = false;
            return false;
        }

        _logger.Information("Connection successful.");
        _logger.Information("Bootloader version = {}.{}.{}", version.Major, version.Minor, version.Bugfix);
        BootloaderVersion = version;
        _isConnected = true;
        return true;
    }


}