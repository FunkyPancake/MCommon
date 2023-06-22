namespace CalTp.Bootloader.BootloaderLogic;

internal struct CommandPacket {
    public Command Type;
    public bool Flag;
    public uint[] Parameters;

    public CommandPacket(Command type, bool flag, uint[] parameters) {
        Type = type;
        Flag = flag;
        Parameters = parameters;
    }
}