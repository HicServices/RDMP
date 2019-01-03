using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Progress;

namespace LoadModules.Generic.Mutilators
{
    /// <summary>
    /// This component will make all tables matching the <see cref="MatchingTablesMutilator.TableRegexPattern"/> distinct.  It should only be run in RAW.
    /// 
    /// <para>The RAW=>STAGING migration will already take care of identical records.  This means this component is only useful for
    /// debugging failed RAW batches or running SQL that is intented to resolve PK collisions / data integrity issues where identical 
    /// duplicates make the process slow / difficult.</para>
    /// </summary>
    public class Distincter : MatchingTablesMutilator
    {
        public Distincter():base(LoadStage.AdjustRaw)
        {
            
        }
        protected override void MutilateTable(IDataLoadEventListener job, ITableInfo tableInfo, DiscoveredTable table)
        {
            table.MakeDistinct(Timeout);
        }
    }
}
