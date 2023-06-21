namespace Calibration.CalFile.CalItems;

public class Scalar : ICalItem {
    public Guid Id { get; init; }
    public string Name { get; init; }

    public Scalar(Guid id, string name) {
        Id = id;
        Name = name;
    }
}