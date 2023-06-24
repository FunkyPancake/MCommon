namespace CalTp;
/// <summary>
/// 
/// </summary>
public enum Command {
    /// <summary>
    /// Establishes the connection, as a response an ecu cal config is provided
    /// 
    /// </summary>
    Connect = 0xff,
    /// <summary>
    /// Terminates the connection
    /// </summary>
    Disconnect = 0xfe,
    /// <summary>
    /// Resets the ECU
    /// </summary>
    Reset,
    ReadMemory=0xf5,
    WriteMemory=0xf0,
    /// <summary>
    /// Flashes HEX file 
    /// </summary>
    Program,
    ConfigureCyclicRead,
    StartCyclicRead,
    StopCyclicRead,
    ClearCyclicRead,
    /// <summary>
    /// Gets ECU name, HW version, SW version
    /// </summary>
    GetEcuId,
    /// <summary>
    /// Terminates the connection
    /// </summary>
    GetSwVersion,
    /// <summary>
    /// Jumps to bootloader
    /// </summary>
    JumpToFbl
}