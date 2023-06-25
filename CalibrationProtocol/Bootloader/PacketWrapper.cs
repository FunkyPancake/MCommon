using Version = CommonTypes.Version;

namespace CalTp.Bootloader;

public partial class FblCommands {
    #region FramingPacket

    private const byte StartByte = 0x5A;
    private const byte FramingPacketHeaderLen = 6;
    private const int GenericResponseLen = 18;
    private const int PingResponseLen = 10;


    private static byte[] BuildFramingPacket(PacketType packetType, byte[]? payload = null) {
        if (payload is null) {
            return new[] {StartByte, (byte) packetType};
        }

        var len = payload.Length;
        var header = new[]
            {StartByte, (byte) packetType, (byte) (len & 0xff), (byte) ((len >> 8) & 0xff)};
        var crc = CalcCrc(header.AsSpan()[..4], payload.AsSpan());
        var packet = new byte[FramingPacketHeaderLen + payload.Length];
        packet[4] = (byte) (crc & 0xff);
        packet[5] = (byte) ((crc >> 8) & 0xff);
        header.CopyTo(packet, 0);
        payload.CopyTo(packet, FramingPacketHeaderLen);
        return packet;
    }

    private static byte[] ParseFramingPacket(byte[] bytes) {
        if (bytes[0] != StartByte)
            throw new InvalidDataException();

        var len = bytes[2] + (bytes[3] << 8);
        var crc = bytes[4] + (bytes[5] << 8);
        var calcCrc = CalcCrc(bytes.AsSpan()[..4], bytes.AsSpan()[6..]);
        if (len + FramingPacketHeaderLen != bytes.Length || calcCrc != crc) {
            throw new InvalidDataException();
        }

        var payload = bytes[6..];
        return payload;
    }

    private static (Version, ushort) ParsePingResponse(byte[] bytes) {
        var crc = bytes[PingResponseLen - 2] + (bytes[PingResponseLen - 1] << 8);
        if (bytes[0] != StartByte || bytes[1] != (byte) PacketType.PingResponse ||
            crc != CalcCrc(bytes.AsSpan()[..(PingResponseLen - 2)], Array.Empty<byte>())) {
            throw new InvalidDataException();
        }

        var fblVersion = new Version(bytes[4], bytes[3], bytes[2]);
        var options = (ushort) ((bytes[7] << 8) + bytes[6]);
        return (fblVersion, options);
    }

    #endregion

    #region CommandPacket

    private static byte[] BuildCommandPacket(Command command) {
        var len = command.Parameters.Length;
        if (len > 7)
            throw new ArgumentException("Parameters array larger than 7");
        var commandPacket = new byte[4 + 4 * len];
        var header = new byte[] {(byte) command.Type, (byte) (command.HasDataPhase ? 1 : 0), 0, (byte) len};
        header.CopyTo(commandPacket, 0);
        for (var i = 0; i < len; i++) {
            var bytes = BitConverter.GetBytes(command.Parameters[i]);
            bytes.CopyTo(commandPacket, 4 * i + 4);
        }

        return BuildFramingPacket(PacketType.Command, commandPacket);
    }

    private static Command ParseCommandPacket(byte[] bytes) {
        var command = new Command();
        var response = ParseFramingPacket(bytes);

        command.Type = (CommandType) response[0];
        command.HasDataPhase = response[1] == 1;
        var paramCount = response[3];
        if ((response.Length - 4) / 4 != paramCount) {
            throw new InvalidDataException();
        }

        command.Parameters = new uint[paramCount];
        for (var i = 0; i < paramCount; i++) {
            command.Parameters[i] = BitConverter.ToUInt32(response, 4 * i + 4);
        }

        return command;
    }

    #endregion

    private static ResponseCode ParseGenericResponse(byte[] response, out CommandType commandTag) {
        var command = ParseCommandPacket(response);
        if (command is not {Type: CommandType.ResponseGeneric, Parameters.Length: 2})
            throw new ApplicationException("");
        var statusCode = (ResponseCode) command.Parameters[0];
        commandTag = (CommandType) command.Parameters[1];
        return statusCode;
    }

    private static ResponseCode ParseGetPropertyResponse(Command command, out CommandType commandTag) {
        if (command is not {Type: CommandType.ResponseGeneric, Parameters.Length: 2})
            throw new ApplicationException("");
        var statusCode = (ResponseCode) command.Parameters[0];
        commandTag = (CommandType) command.Parameters[1];
        return statusCode;
    }

    private static ResponseCode ParseReadMemoryResponse(Command command, out CommandType commandTag) {
        if (command is not {Type: CommandType.ResponseGeneric, Parameters.Length: 2})
            throw new ApplicationException("");
        var statusCode = (ResponseCode) command.Parameters[0];
        commandTag = (CommandType) command.Parameters[1];
        return statusCode;
    }

    private static ResponseCode ParseAck(IReadOnlyList<byte> bytes) {
        return bytes[0] == StartByte && bytes[1] == (byte) PacketType.Ack ? ResponseCode.Success : ResponseCode.Fail;
    }

    private static ushort CalcCrc(ReadOnlySpan<byte> data) {
        uint crc = 0;
        crc = CalcCrcInternal(crc, data);
        return (ushort) crc;
    }

    private static ushort CalcCrc(ReadOnlySpan<byte> header, ReadOnlySpan<byte> payload) {
        uint crc = 0;
        crc = CalcCrcInternal(crc, header);
        crc = CalcCrcInternal(crc, payload);
        return (ushort) crc;
    }

    private static ushort CalcCrcInternal(uint crc, ReadOnlySpan<byte> packet) {
        uint j;
        for (j = 0; j < packet.Length; ++j) {
            uint i;
            uint b = packet[(int) j];
            crc ^= b << 8;
            for (i = 0; i < 8; ++i) {
                var temp = crc << 1;
                if ((crc & 0x8000) == 0x8000) {
                    temp ^= 0x1021;
                }

                crc = temp;
            }
        }

        return (ushort) crc;
    }
}