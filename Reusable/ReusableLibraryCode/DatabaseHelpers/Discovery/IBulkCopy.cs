using System;
using System.Data;
using System.Data.Common;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery
{
    /// <summary>
    /// Cross database type implementation of Bulk Insert.  Each database API handles this differently, the differences are abstracted here through this
    /// interface such that the programmer doesn't need to know what type of database he is uploading a DataTable to in order for it to still work.
    /// </summary>
    public interface IBulkCopy : IDisposable
    {
        int Upload(DataTable dt);
        int Timeout { get; set; }

        /// <summary>
        /// Notifies the <see cref="IBulkCopy"/> that the table schema has been changed mid insert! e.g. a column changing data type. This change must have taken place on the same
        /// <see cref="DbTransaction"/> as the bulkc copy.
        /// </summary>
        void InvalidateTableSchema();
    }
}