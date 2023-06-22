namespace CalTp.Bootloader.BootloaderLogic;

internal static class PacketWrapper {
    private const byte StartByte = 0x5A;
    private const byte FramingPacketHeaderLen = 6;

    public static byte[] BuildFramingPacket(PacketType packetType, byte[]? payload = null) {
        var header = new List<byte> {
            StartByte, (byte) packetType
        };
        if (payload is null) {
            return header.ToArray();
        }

        var len = payload.Length;
        header.AddRange(new[] {(byte) (len & 0xff), (byte) ((len >> 8) & 0xff)});
        var dataForCrc = header.Concat(payload).ToArray();
        var crc = CalcCrc(dataForCrc);
        var packet = new byte[FramingPacketHeaderLen + payload.Length];
        packet[4] = (byte) (crc & 0xff);
        packet[5] = (byte) ((crc >> 8) & 0xff);
        header.CopyTo(packet, 0);
        payload.CopyTo(packet, FramingPacketHeaderLen);
        return packet.ToArray();
    }

    public static byte[] BuildCommandPacket(CommandPacket command) {
        var len = command.Parameters.Length;
        if (len > 7)
            throw new ArgumentException("Parameters array larger than 7");
        var commandPacket = new byte[4 + 4 * len];
        var header = new byte[] {(byte) command.Type, (byte) (command.Flag ? 1 : 0), 0, (byte) len};
        header.CopyTo(commandPacket, 0);
        for (var i = 0; i < len; i++) {
            var bytes = BitConverter.GetBytes(command.Parameters[i]);
            bytes.CopyTo(commandPacket, 4 * i + 4);
        }

        return BuildFramingPacket(PacketType.Command, commandPacket);
    }

    public static byte[] ParseFramingPacket(byte[] bytes) {
        if (bytes[0] != StartByte)
            throw new InvalidDataException();

        var len = bytes[2] + (bytes[3] << 8);
        var crc = bytes[4] + (bytes[5] << 8);
        var arrayForCrcCalc = bytes[..4].Concat(bytes[6..]).ToArray();
        var calcCrc = CalcCrc(arrayForCrcCalc);
        if (len + FramingPacketHeaderLen != bytes.Length || calcCrc != crc) {
            throw new InvalidDataException();
        }

        var payload = bytes[6..];
        return payload;
    }

    public static CommandPacket ParseCommandPacket(byte[] bytes) {
        var command = new CommandPacket();
        var response = ParseFramingPacket(bytes);

        command.Type = (Command) response[0];
        command.Flag = response[1] == 1;
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


    public static bool ParsePingResponse(byte[] bytes, out byte[] response) {
        response = Array.Empty<byte>();
        if (bytes.Length != 10 || bytes[0] != StartByte || bytes[1] != (byte) PacketType.PingResponse) {
            return false;
        }

        response = bytes[2..8];
        var crc = bytes[8] + (bytes[9] << 8);
        return crc == CalcCrc(bytes[..8]);
    }

    public static bool ParseAck(byte[] bytes) {
        return bytes[0] == StartByte && bytes[1] == (byte) PacketType.Ack;
    }

    private static ushort CalcCrc(IReadOnlyList<byte> packet) {
        uint crc = 0;
        uint j;
        for (j = 0; j < packet.Count; ++j) {
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