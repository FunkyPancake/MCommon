namespace CalTp.Bootloader; 

public enum PropertyTag {
    CurrentVersion = 1,
    AvailablePeripherals = 2,
    FlashStartAddress = 3,
    FlashSizeInBytes = 4,
    FlashSectorSize = 5,
    FlashBlockCount = 6,
    AvailableCommands = 7,
    VerifyWrites = 0xA,
    MaxPacketSize = 0xB,
    ReservedRegions = 0xC,
    ValidateRegions = 0xD,
    RamStartAddress = 0xE,
    RamSizeInBytes = 0xF,
    SystemDeviceId = 0x10,
    FlashSecurityState = 0x11,
    UniqueDeviceId = 0x12,
    FacSupport = 0x13,
    FlashAcessSegmentSize = 0x14,
    FlashAcessSegmentCount = 0x15,
}