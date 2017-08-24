namespace ReusableLibraryCode.DatabaseHelpers.Discovery
{
    public interface IMightNotExist
    {
        bool Exists(IManagedTransaction transaction = null);
    }
}