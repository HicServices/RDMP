using System;
using System.Data;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery
{
    public interface IBulkCopy : IDisposable
    {
        int Upload(DataTable dt);
    }
}