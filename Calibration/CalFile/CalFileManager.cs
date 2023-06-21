using System.Xml;
using System.Xml.Linq;
using Calibration.CalFile.CalItems;
using Serilog;

namespace Calibration.CalFile;

public class CalFileManager {
    private readonly ILogger _logger;
    public const string ConfigFileExtension = ".xml";

    public CalFileManager(ILogger logger) {
        _logger = logger;
    }

    public async Task<Cal> Load(string filePath) {
        var cal = new CalInternal();
        Dictionary<Guid, Variable> variables = new();
        Dictionary<Guid, IUnitGroup> unitGroups = new();

        XElement calTree = new("Calibration");
        if (!File.Exists(filePath) || Path.GetExtension(filePath) != ConfigFileExtension) {
            throw new ApplicationException("Incorrect file extension or file doesn't exist");
        }

        var tree = await LoadTree(filePath);

        var definitionBlock = tree.Element("DefinitionBlock")!;
        foreach (var element in definitionBlock.Element("SystemDefinition")?.Elements()!) {
        }

        foreach (var element in tree.Element("UnitGroups")?.Elements()!) {
            ParseUnitGroup(element);
        }

        foreach (var element in tree?.Element("Variables")?.Elements()!) {
            ParseVariable(element);
        }

        foreach (var element in tree.Element("Calibration")?.Elements()!) {
            BuildTree(calTree, element);
        }

        return cal;
    }

    public void Save(string filePath, ref Cal cal) {
    }


    private static async Task<XElement> LoadTree(string filePath) {
        using var streamReader = new StreamReader(filePath);
        var settings = new XmlReaderSettings() {
            Async = true,
            IgnoreWhitespace = true,
            IgnoreComments = true
        };
        var reader = XmlReader.Create(streamReader, settings);
        var token = new CancellationToken();
        var tree = await XElement.LoadAsync(reader, LoadOptions.None, token);
        if (token.IsCancellationRequested) {
            throw new OperationCanceledException();
        }

        return tree;
    }

    private void BuildTree(XContainer parent, XElement element) {
        if (element.Name == "CalGroup") {
            var name = element.Attribute("name")!.Value;
            var node = new XElement("CalGroup", new XAttribute("name", name));
            parent.Add(node);
            var subElements = element.Elements().Where(x => x.Name == "CalGroup" || x.Name == "CalItem");
            foreach (var subElement in subElements) {
                BuildTree(node, subElement);
            }
        }
        else {
            if (element.Name != "CalItem")
                return;
            var name = element.Element("LongName")!.Value;
            Enum.TryParse(element.Attribute("type")!.Value, out CalItemType type);
            var idAttribute = element.Attribute("id")!;
            var node = new XElement("CalItem", new XAttribute("name", name), idAttribute);
            parent.Add(node);
            var id = Guid.Parse(idAttribute.Value);
            ICalItem item = type switch {
                CalItemType.Scalar => new Scalar(id, name),
                CalItemType.Axis => new Axis(),
                CalItemType.Curve => new Curve(),
                CalItemType.Map => new Map(),
                _ => throw new ArgumentOutOfRangeException()
            };

            cal.Add(id, item);
        }
    }


    public XElement GetCalTree() {
        return _calTree;
    }

    public ICalItem GetCalById(Guid id) {
        return _cal[id];
    }

    private void ParseVariable(XElement element) {
        var id = Guid.Parse(element.Attribute("id")!.Value);
        var tmp = element.Element("UnitGroup")!.Attribute("id")!.Value;
        var unitGroupId = Guid.Parse(tmp);
        _variables.Add(id, new Variable(id, _unitGroups[unitGroupId]));
    }

    private void ParseUnitGroup(XElement element) {
        var id = Guid.Parse(element!.Attribute("id")!.Value);

        _unitGroups.Add(id, new UnitGroup(id, "texst sample"));
    }

    public async void Save(Config config) {
    }

    private void ParseDefinitions() {
    }

    private void ParseValues() {
    }

    private void SaveDefinitions() {
    }

    private void SaveValues() {
    }

    private class CalInternal : Cal {
        public CalInternal() : base() {
        }
    }
}