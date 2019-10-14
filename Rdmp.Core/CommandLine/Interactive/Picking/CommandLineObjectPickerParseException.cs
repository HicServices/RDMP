using System;

namespace Rdmp.Core.CommandLine.Interactive
{
    internal class CommandLineObjectPickerParseException : Exception
    {
        public int Index { get; }
        public string RawValue { get; }

        public CommandLineObjectPickerParseException(string message, int index, string rawValue):base(message)
        {
            Index = index;
            RawValue = rawValue;
        }
    }
}