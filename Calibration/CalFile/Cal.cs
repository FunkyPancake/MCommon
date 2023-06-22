namespace Calibration.CalFile;

public abstract class Cal {
    protected Cal() {
    }

    public ICalVariables Variables { get; } = null;
    public ICalParameters Parameters { get; set; } = null;
    protected void Add(){}
}

public interface ICalParameters {
}

public interface ICalVariables {
}