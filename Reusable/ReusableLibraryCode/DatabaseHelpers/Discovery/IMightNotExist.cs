namespace ReusableLibraryCode.DatabaseHelpers.Discovery
{
    /// <summary>
    /// Describes a database object that might not exist.  You can use methods that have keyword 'Expect' (e.g. DiscoveredServer.ExpectDatabase("bob")) to return
    /// an object (DiscoveredDatabase in the example) without first checking that they exist.  Call IMightNotExist.Exists to confirm whether it still exists.
    /// 
    /// The opposite approach is to use 'Discover' methods e.g. DiscoveredServer.DiscoverDatabases() to return all the DiscoveredDatbases found on the server.
    /// </summary>
    public interface IMightNotExist
    {
        bool Exists(IManagedTransaction transaction = null);
    }
}