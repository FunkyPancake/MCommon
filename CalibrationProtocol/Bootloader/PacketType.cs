namespace CalTp.Bootloader.BootloaderLogic;

internal enum PacketType
{
    Ack = 0xA1,
    Nak = 0xA2,
    AckAbort = 0xA3,
    Command = 0xA4,
    Data = 0xA5,
    Ping = 0xA6,
    PingResponse = 0xA7
}