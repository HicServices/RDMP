using System.Collections.ObjectModel;
using MapsDirectlyToDatabaseTable;

namespace Rdmp.Core.CommandLine.Interactive
{
    public class CommandLineObjectPickerArgumentValue
    {

        public string RawValue { get; }
        public int Index { get; }

        public ReadOnlyCollection<IMapsDirectlyToDatabaseTable> DatabaseEntities { get; }

        private CommandLineObjectPickerArgumentValue(string rawValue,int idx)
        {
            RawValue = rawValue;
            Index = idx;
        }


        public CommandLineObjectPickerArgumentValue(string rawValue,int idx,IMapsDirectlyToDatabaseTable[] entities):this(rawValue, idx)
        {
            DatabaseEntities = new ReadOnlyCollection<IMapsDirectlyToDatabaseTable>(entities);
        }
    }
}