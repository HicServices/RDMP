namespace ReusableLibraryCode.DatabaseHelpers.Discovery
{
    /// <summary>
    /// Cross database type reference to a stored proceedure (function) on a database.
    /// </summary>
    public class DiscoveredStoredprocedure
    {
        public string Name { get; set; }

        public DiscoveredStoredprocedure(string name)
        {
            Name = name;
        }
    }
}