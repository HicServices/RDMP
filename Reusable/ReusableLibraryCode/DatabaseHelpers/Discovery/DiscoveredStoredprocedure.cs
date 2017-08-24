namespace ReusableLibraryCode.DatabaseHelpers.Discovery
{
    public class DiscoveredStoredprocedure
    {
        public string Name { get; set; }

        public DiscoveredStoredprocedure(string name)
        {
            Name = name;
        }
    }
}