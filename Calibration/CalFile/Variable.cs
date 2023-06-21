namespace Calibration.CalFile;

public class Variable {
    private IUnitGroup _unitGroup;
    private uint valueRaw;

    public dynamic Value { get; }

    public Variable(Guid id, IUnitGroup unitGroup) {
        _unitGroup = unitGroup;
    }

    public override string ToString() {
        return base.ToString();
    }
}