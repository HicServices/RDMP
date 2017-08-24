using System;

namespace MapsDirectlyToDatabaseTable.Versioning
{
    public class InvalidPatchException : Exception
    {
        public string ScriptName { get; set; }

        public InvalidPatchException(string scriptName, string message, Exception exception=null):base(message,exception)
        {
            ScriptName = scriptName;
        }
    }
}