namespace CalTp.Bootloader.BootloaderLogic;

internal enum Command
{
    FlashEraseAll = 0x01,
    FlashEraseRegion = 0x02,
    ReadMemory = 0x03,
    WriteMemory = 0x04,
    FlashSecurityDisable = 0x06,
    GetProperty = 0x07,
    Execute = 0x09,
    Reset = 0x0B,
    SetProperty = 0x0C,
    FlashEraseAllUnsecure = 0x0D,
    ResponseGeneric = 0xA0,
    ResponseReadMemory = 0xA3,
    ResponseGetProperty = 0xA7
}