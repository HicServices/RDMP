using System;

namespace MapsDirectlyToDatabaseTable.Versioning
{
    /// <summary>
    /// Thrown when an SQL update patch in a .Database assembly (e.g. CatalogueLibrary.Database) is not formed correctly
    /// </summary>
    public class InvalidPatchException : Exception
    {
        public string ScriptName { get; set; }

        public InvalidPatchException(string scriptName, string message, Exception exception=null):base(message,exception)
        {
            ScriptName = scriptName;
        }
    }
}