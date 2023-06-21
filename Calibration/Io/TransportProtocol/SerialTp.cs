using System.IO.Ports;
using System.Text;
using Serilog;

namespace Calibration.Io.TransportProtocol;

public class SerialTp : ITransportProtocol, IDisposable {
    private readonly ILogger _logger;
    private readonly SerialTpConfig _config;
    private readonly SerialPort _serialPort;
    private byte _rxFrameCounter;
    private byte _txFrameCounter;
    private int _timeout;
    private readonly SemaphoreSlim _semaphoreSlim = new(1);

    public SerialTp(ILogger logger, SerialTpConfig config) {
        _logger = logger;
        _config = config;
        _serialPort = new SerialPort(_config.ComPort) {
            Encoding = Encoding.UTF8,
            BaudRate = _config.Baudrate,
            DataBits = 8,
            Parity = Parity.None,
            StopBits = StopBits.One,
            Handshake = Handshake.None,
            RtsEnable = false,
            DtrEnable = false
        };
    }

    public IEnumerable<int> GetAvailableChannels() {
        return SerialPort.GetPortNames().Select(portName => int.Parse(portName[3..]));
    }

    public bool Connect() {
        if (_serialPort.IsOpen) {
            return true;
        }

        try {
            _serialPort.Open();
            _serialPort.DiscardInBuffer();
            _serialPort.DiscardOutBuffer();
        }
        catch (IOException) {
            _logger.Error("Invalid port, {channel} doesn't exist or busy", _config.ComPort);
            return false;
        }

        _timeout = _config.CommunicationTimeout;
        _rxFrameCounter = 0;
        _txFrameCounter = 0;

        return true;
    }

    public void Disconnect() {
        _serialPort.Close();
    }

    public async Task<(TpStatus Status, byte[] Data)> Query(byte[] command, int responseLength) {
        if (!_serialPort.IsOpen) {
            _logger.Error("Interface not connected.");
            return (TpStatus.NotConnected, Array.Empty<byte>());
        }

        var request = new byte[command.Length + 3];
        new[] {
            (byte) ((command.Length & 0xff00) >> 8), (byte) (command.Length & 0xff), _txFrameCounter
        }.CopyTo(request, 0);
        command.CopyTo(request, 3);
        var bytesToRead = responseLength + 3;

        _logger.Debug("Request: {data}", LogRaw(request));
        await _semaphoreSlim.WaitAsync();
        _serialPort.Write(request, 0, request.Length);
        _txFrameCounter++;

        var response = new byte[bytesToRead];
        var task = ReadBytesAsync(response, bytesToRead);
        var timeoutTask = Task.Delay(_timeout);
        if (await Task.WhenAny(task, timeoutTask) == timeoutTask) {
            _logger.Error("Communication timeout");
            return (TpStatus.Timeout, Array.Empty<byte>());
        }

        _semaphoreSlim.Release();
        if (task.Result != bytesToRead) {
            _logger.Error("Rx Data to short, expected:{expected}, received:{bytes}", bytesToRead, task.Result);
        }


        _logger.Debug("Response: {data}", LogRaw(response[..task.Result]));
        if (response[2] != _rxFrameCounter) {
            _rxFrameCounter++;
            _logger.Error("Communication error - missing rx frames.");
            return (TpStatus.InvalidMsgCounter, Array.Empty<byte>());
        }

        _rxFrameCounter++;
        return (TpStatus.Ok, response[3..]);
    }

    public event EventHandler? OnNewAsyncMessage;

    private async Task<int> ReadBytesAsync(byte[] buffer, int bytesToRead) {
        var bytesRead = 0;
        while (bytesRead < bytesToRead) {
            var read = await _serialPort.BaseStream.ReadAsync(buffer.AsMemory(bytesRead, bytesToRead - bytesRead));
            bytesRead += read;
        }

        return bytesRead;
    }

    private static string LogRaw(IReadOnlyCollection<byte> data) {
        var str = new StringBuilder(data.Count * 4 + 1);
        foreach (var b in data) {
            str.Append($"0x{b:x2} ");
        }

        return str.ToString();
    }

    public void Dispose() {
        GC.SuppressFinalize(this);
    }
}