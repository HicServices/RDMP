using System.Collections.Generic;
using HIC.Logging;

namespace DataLoadEngine.Migration
{
    public interface IMigrationStrategy
    {
        void Execute(IEnumerable<MigrationColumnSet> toMigrate, IDataLoadInfo dataLoadInfo);
    }
}