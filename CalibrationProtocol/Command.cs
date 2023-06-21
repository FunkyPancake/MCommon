namespace CalTp; 

public enum Command {
    Connect = 0xff,
    Disconnect = 0xfe,
    Reset,
    GetControlBlock,
    ReadMemory=0xf5,
    WriteMemory=0xf0,
    Program,
    ConfigureCyclicRead,
    StartCyclicRead,
    StopCyclicRead,
    ClearCyclicRead,
    JumpToFbl
}