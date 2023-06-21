using Peak.Can.Basic;
using Serilog;

namespace CanDriver.PeakCan;

public class PeakCan : ICanDevice {
    private readonly Dictionary<uint, int> _broadcastDictionary = new();
    private readonly Dictionary<uint, ICanDevice.NewFrameReceivedEventHandler> _eventHandlers = new();
    private readonly Dictionary<uint, CanFrame> _lastFrame = new Dictionary<uint, CanFrame>();
    private readonly ILogger _logger;
    private readonly Dictionary<uint, Queue<CanFrame>> _rxQueues = new();
    private CanDeviceConfig _config = null!;
    private PcanChannel _handle = PcanChannel.None;
    private Worker _worker = new();

    public PeakCan(ILogger logger) {
        _logger = logger;
    }

    public bool Connect() {
        _worker = new Worker(_handle, ConvertBitrate(_config.Baudrate));
        _worker.MessageAvailable += OnMessageAvailable;
        _worker.Start();

        _logger.Information("Opened channel {Channel}, bitrate {Bitrate}", _handle, _config.Baudrate);
        return true;
    }

    public void Configure(CanDeviceConfig config) {
        _config = config;
        var parameter = ParameterValue.LookUp.GetCriteriaString(PcanDevice.PcanUsb, null, null, null);
        var status = Api.LookUpChannel(parameter, out var handle);
        if (status != PcanStatus.OK) {
            _logger.Error("Pcan Status = {status}", status);
            return;
        }

        _handle = handle;
    }

    public void Disconnect() {
        _logger.Information($"Close channel {_handle}");
        _worker.Stop(true, true);
        Api.Uninitialize(_handle);
        _lastFrame.Clear();
        _rxQueues.Clear();
        _eventHandlers.Clear();
    }

    public void SendFrame(CanFrame frame) {
        var msg = GetPcanMessage(frame);
        if (!_worker.Transmit(msg, out var status)) {
            _logger.Error("Transmit failed, reason: {status}", status);
        }
    }

    public void SendFrames(IEnumerable<CanFrame> frames) {
        foreach (var frame in frames) {
            SendFrame(frame);
        }
    }

    public void StartSendFrameCyclic(CanFrame frame, int cycleTime) {
        int broadcastId;
        if (_broadcastDictionary.ContainsKey(frame.Id)) {
            broadcastId = _broadcastDictionary[frame.Id];
            _worker.UpdateBroadcast(broadcastId, cycleTime, GetPcanMessage(frame));
            _logger.Information("Update cyclic transmit, frame ID:{id:X}, new cycle time: {cycleTime}", frame.Id,
                cycleTime);
        }
        else {
            broadcastId = _worker.AddBroadcast(GetPcanMessage(frame), cycleTime);
            _broadcastDictionary.Add(frame.Id, broadcastId);
            _logger.Information("Add new cyclic transmit, frame ID:{id:X}, cycle time: {cycleTime}", frame.Id,
                cycleTime);
        }

        _worker.ResumeBroadcast(broadcastId);
    }

    public void StopSendFrameCyclic(CanFrame frame) {
        if (_broadcastDictionary.ContainsKey(frame.Id)) {
            var broadcastId = _broadcastDictionary[frame.Id];
            _worker.PauseBroadcast(broadcastId);
            _logger.Information("Cyclic transmit stopped, frame ID:{id:X}", frame.Id);
        }
        else {
            _logger.Error("Frame ID:{id:X}is not cyclic TX", frame.Id);
        }
    }

    public Queue<CanFrame> SubscribeFrameToQueue(CanFrame frame) {
        if (_rxQueues.TryAdd(frame.Id, new Queue<CanFrame>()) || _rxQueues.ContainsKey(frame.Id)) {
            return _rxQueues[frame.Id];
        }

        _logger.Fatal("Frame cannot be subscribed to queue, frame ID:{id}", frame.Id);
        throw new InvalidOperationException("Frame cannot be added to queue");
    }

    public Queue<CanFrame>? GetSubscribedFrameQueue(CanFrame frame) {
        return _rxQueues.ContainsKey(frame.Id) ? _rxQueues[frame.Id] : null;
    }

    public void SubscribeFrame(CanFrame frame, ICanDevice.NewFrameReceivedEventHandler handler) {
        _eventHandlers.Add(frame.Id, handler);
        _logger.Information("Subscribe frame ID:{id:X}, event handler: {handler}", frame.Id,
            nameof(handler));
    }

    public void UnsubscribeFrame(CanFrame frame) {
        var id = frame.Id;
        if (_eventHandlers.ContainsKey(id)) {
            _eventHandlers.Remove(id);
        }

        if (_rxQueues.ContainsKey(id)) {
            _rxQueues[id].Clear();
            _rxQueues.Remove(id);
        }

        _logger.Information("Unsubscribe frame ID:{id:X}", frame.Id);
    }

    public IEnumerable<CanFrame> GetFrames(CanFrame frame) {
        var list = new List<CanFrame>();
        var id = frame.Id;
        if (!_rxQueues.TryGetValue(id, out var queue))
            return list;

        while (queue.Count > 0) {
            list.Add(queue.Dequeue());
        }

        return list;
    }

    public bool GetFrame(ref CanFrame frame) {
        var id = frame.Id;
        if (_rxQueues.ContainsKey(id)) {
            frame = _rxQueues[id].Dequeue();
            return true;
        }

        if (!_lastFrame.ContainsKey(id))
            return false;
        frame = _lastFrame[id];
        return true;
    }

    private void OnMessageAvailable(object? sender, MessageAvailableEventArgs e) {
        if (_worker.Dequeue(e.QueueIndex, out var message, out var timestamp)) {
            var frame = BuildPcanMessage(message, timestamp);
            if (_rxQueues.TryGetValue(frame.Id, out var queue)) {
                queue.Enqueue(frame);
            }

            if (_eventHandlers.TryGetValue(frame.Id, out var ev)) {
                ev.Invoke(this, new NewFrameRecievedEventArgs(frame));
            }

            if (!_lastFrame.TryAdd(frame.Id, frame)) {
                _lastFrame[frame.Id] = frame;
            }
        }
        else {
            _logger.Error("Cannot dequeue queue with Index:{idx}", e.QueueIndex);
        }
    }

    private static PcanMessage GetPcanMessage(CanFrame frame) {
        var msg = new PcanMessage {
            ID = frame.Id & 0x7fffffff,
            MsgType = GetMessageType(frame.Id),
            DLC = frame.Dlc,
            Data = frame.Payload
        };
        return msg;
    }

    private static CanFrame BuildPcanMessage(PcanMessage frame, ulong timestamp) {
        var msg = new CanFrame {
            Id = frame.ID | (frame.MsgType == MessageType.Extended ? 0x80000000 : 0),
            Dlc = frame.DLC,
            Payload = frame.Data,
            Timestamp = timestamp
        };
        return msg;
    }

    private static Bitrate ConvertBitrate(Baudrate bitrate) {
        var value = bitrate switch {
            Baudrate.Baud125k => Bitrate.Pcan125,
            Baudrate.Baud250k => Bitrate.Pcan250,
            Baudrate.Baud500k => Bitrate.Pcan500,
            Baudrate.Baud1000k => Bitrate.Pcan1000,
            _ => throw new ArgumentOutOfRangeException(nameof(bitrate), bitrate, null)
        };

        return value;
    }

    private static MessageType GetMessageType(uint id) {
        const uint extIdFlag = 0x80000000;
        return (id & extIdFlag) == extIdFlag ? MessageType.Extended : MessageType.Standard;
    }
}