using System;

namespace CatalogueLibrary.Cloning
{
    public class MissingTableException : Exception
    {
        private readonly string _expectedTable;
        private readonly string _databaseName;

        public MissingTableException(string expectedTable, string databaseName)
        {
            _expectedTable = expectedTable;
            _databaseName = databaseName;
        }

        public override string Message
        {
            get { return "Missing table " + _expectedTable + " in target database:" + _databaseName; }
        }
    }
}