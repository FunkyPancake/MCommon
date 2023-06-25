namespace CalTp.Bootloader;

internal struct Command {
    public CommandType Type;
    //if Flag is set to true, the data packets will follow in the commands sequence
    public bool HasDataPhase;
    public uint[] Parameters;

    public Command(CommandType type, bool hasDataPhase, uint[] parameters) {
        Type = type;
        HasDataPhase = hasDataPhase;
        Parameters = parameters;
    }
}