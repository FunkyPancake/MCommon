namespace Calibration.CalFile.CalItems; 

public interface ICalItem {
    public Guid Id { get; init; }
    public string Name { get; init; }
}