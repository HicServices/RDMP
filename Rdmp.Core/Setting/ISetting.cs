using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.Core.Setting;

public interface ISetting: IMapsDirectlyToDatabaseTable
{
    string Key { get; set; }
    string Value { get; set; }
}