using CalTp.TransportProtocol;

namespace CalTp.Bootloader;

public partial class FblCommands {
    private const int AckLen = 2;

    private async Task<(ResponseCode status, uint value)> GetPropertyResponse() {
        // throw new NotImplementedException();
        return (ResponseCode.Fail, 0);
    }

    private bool ProcessCommandNoData() {
        return false;
    }


    private async Task<ResponseCode> CommandNoData(Command command, uint timeout = DefaultTimeoutMs) {
        await SendCommand(command);
        await WaitForAck();

        var resp = ResponseCode.Fail;
        for (var i = AttemptCounter; i > 0; i--) {
            resp = await GetGenericResponse(command.Type, 1);
            if (resp is ResponseCode.AppCrcCheckOutOfRange or ResponseCode.Fail or ResponseCode.Timeout) {
                SendNack();
            }
            else {
                SendAck();
                break;
            }
        }

        return resp;
    }

    private async Task WaitForAck() {
        await _tp.ReadAsync(2, AckTimeoutMs);
    }

    private async Task<(ResponseCode status, byte[] data)> CommandDataIn(Command command) {
        throw new NotImplementedException();
    }
    private async Task<ResponseCode> SendQueryAsync(byte[] request, int responseLen,
        Func<byte[], ResponseCode> action, uint timeout) {
        var cancellationToken = CancellationToken.None;
        await _tp.WriteAsync(request);
        var task = await _tp.ReadAsync(responseLen, timeout, cancellationToken);
        return task.status == TpStatus.Ok ? action.Invoke(task.data) : MapTpStatusToResponseCode(task.status);
    }

    private static ResponseCode MapTpStatusToResponseCode(TpStatus taskStatus) {
        var code = ResponseCode.Fail;
        switch (taskStatus) {
            case TpStatus.Ok:
                code = ResponseCode.Success;
                break;
            case TpStatus.NotConnected:
                break;
            case TpStatus.Timeout:
                break;
            case TpStatus.DeviceError:
                break;
            case TpStatus.InvalidLength:
                break;
            case TpStatus.InvalidMsgCounter:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(taskStatus), taskStatus, null);
        }

        return code;
    }

    //Send command and expect ACK message in return
    private async Task<ResponseCode> SendCommand(Command command, int timeout = DefaultTimeoutMs) {
        var cancellationToken = CancellationToken.None;
        await _tp.WriteAsync(BuildCommandPacket(command));
        var task = await _tp.ReadAsync(AckLen, AckTimeoutMs, cancellationToken);
        return task.status == TpStatus.Ok ? ParseAck(task.data) : MapTpStatusToResponseCode(task.status);
    }


    private async void SendNack() {
        await _tp.WriteAsync(BuildFramingPacket(PacketType.Nak));
    }

    private async void SendAck() {
        await _tp.WriteAsync(BuildFramingPacket(PacketType.Ack));
    }

    private async Task<ResponseCode> GetGenericResponse(CommandType commandType, uint timeout = DefaultTimeoutMs) {
        var readResult = await _tp.ReadAsync(GenericResponseLen, timeout);
        var status =
            ParseGenericResponse(readResult.data,
                out var tag);
        if (commandType != tag) {
            _logger.Error("Command tag mismatch. Expected {0}, received {1}", commandType, tag);
            return ResponseCode.Fail;
        }

        return status;
    }

    private async Task<byte[]> WaitForResponseAndAck(int responseLength) {
        return new byte[responseLength];
    }


    private ResponseCode GetResponseCode(Command response) {
        return (ResponseCode) response.Parameters[0];
    }

    private async Task<ResponseCode> CommandDataOut(Command command, byte[] bytes) {
        throw new NotImplementedException();
    }
}