namespace CalTp.Bootloader.BootloaderLogic;

internal struct Command {
    public CommandType Type;
    //if Flag is set to true, the data packets will follow in the commands sequence
    public bool Flag;
    public uint[] Parameters;

    public Command(CommandType type, bool flag, uint[] parameters) {
        Type = type;
        Flag = flag;
        Parameters = parameters;
    }
}