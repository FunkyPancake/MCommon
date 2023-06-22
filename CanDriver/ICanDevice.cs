
namespace CanDriver;

public interface ICanDevice {
    public delegate void NewFrameReceivedEventHandler(object sender, NewFrameRecievedEventArgs e);

    public bool Connect();
    public void Configure(CanDeviceConfig config);
    public void Disconnect();
    public void SendFrame(CanFrame frame);
    public void SendFrames(IEnumerable<CanFrame> frames);
    public void StartSendFrameCyclic(CanFrame frame, int cycleTime);
    public void StopSendFrameCyclic(CanFrame frame);
    public void SubscribeFrame(CanFrame frame, NewFrameReceivedEventHandler handler);
    public Queue<CanFrame> SubscribeFrameToQueue(CanFrame frame);
    public Queue<CanFrame>? GetSubscribedFrameQueue(CanFrame frame);
    public void UnsubscribeFrame(CanFrame frame);
    public IEnumerable<CanFrame> GetFrames(CanFrame frame);
    public bool GetFrame(ref CanFrame frame);
}

public class NewFrameRecievedEventArgs : EventArgs {
    public NewFrameRecievedEventArgs(CanFrame frame) {
        Frame = frame;
    }

    public CanFrame Frame { get; init; }
}