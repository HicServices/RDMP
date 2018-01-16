using System;
using System.Data;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery
{
    /// <summary>
    /// Cross database type implementation of Bulk Insert.  Each database API handles this differently, the differences are abstracted here through this
    /// interface such that the programmer doesn't need to know what type of database he is uploading a DataTable to in order for it to still work.
    /// </summary>
    public interface IBulkCopy : IDisposable
    {
        int Upload(DataTable dt);
    }
}