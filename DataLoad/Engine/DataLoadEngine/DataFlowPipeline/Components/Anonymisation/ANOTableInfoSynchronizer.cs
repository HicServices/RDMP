using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using ReusableLibraryCode.Checks;

namespace DataLoadEngine.DataFlowPipeline.Components.Anonymisation
{
    public class ANOTableInfoSynchronizer
    {
        private readonly TableInfo _tableToSync;

        public ANOTableInfoSynchronizer(TableInfo tableToSync)
        {
            _tableToSync = tableToSync;
            
        }

        public void Synchronize(ICheckNotifier notifier)
        {
            var preLoadDiscardedColumns = _tableToSync.PreLoadDiscardedColumns;
            
            IdentifierDumper dumper = new IdentifierDumper(_tableToSync);
            dumper.Check(notifier);

            CheckForDuplicateANOVsRegularNames();
            
            var columnInfosWithANOTransforms = _tableToSync.ColumnInfos.Where(c => c.ANOTable_ID != null).ToArray();

            if (!columnInfosWithANOTransforms.Any())
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        "There are no ANOTables configured for this table so skipping ANOTable checking",
                        CheckResult.Success));
            
            foreach (ColumnInfo columnInfoWithANOTransform in columnInfosWithANOTransforms)
            {
                ANOTable anoTable = columnInfoWithANOTransform.ANOTable;
                anoTable.Check(new ThrowImmediatelyCheckNotifier());
                
                if(!anoTable.GetRuntimeDataType(LoadStage.PostLoad).Equals(columnInfoWithANOTransform.Data_type))
                    throw new ANOConfigurationException("Mismatch between anoTable.GetRuntimeDataType(LoadStage.PostLoad) = " + anoTable.GetRuntimeDataType(LoadStage.PostLoad) + " and column " + columnInfoWithANOTransform + " datatype = " +columnInfoWithANOTransform.Data_type);
                
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        "ANOTable " + anoTable + " has shared compatible datatype " + columnInfoWithANOTransform.Data_type + " with ColumnInfo " +
                        columnInfoWithANOTransform, CheckResult.Success));
            }
        }

        private void CheckForDuplicateANOVsRegularNames()
        {
            //make sure he doesn't have 2 columns called MyCol and ANOMyCol in the same table as this will break RAW creation and is symptomatic of a botched anonymisation configuration change
            var colNames = _tableToSync.ColumnInfos.Select(c => c.GetRuntimeName()).ToArray();
            var duplicates = colNames.Where(c => colNames.Any(c2 => c2.Equals(ANOTable.ANOPrefix + c))).ToArray();

            if (duplicates.Any())
                throw new ANOConfigurationException("The following columns exist both in their identifiable state and ANO state in TableInfo " + _tableToSync + " (this is not allowed).  The offending column(s) are:" + string.Join(",", duplicates.Select(s => "'" + s + "' & '" + ANOTable.ANOPrefix + s + "'")));

        }
    }
}
