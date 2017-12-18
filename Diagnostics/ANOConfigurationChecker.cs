using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using DataLoadEngine.DataFlowPipeline.Components.Anonymisation;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.Checks;

namespace Diagnostics
{
    public class ANOConfigurationChecker:ICheckable
    {
        private readonly IRepository _repository;
        ANOTable[] _allAnoTables;

        public ANOConfigurationChecker(IRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Calls .Check on all ANOTables and IdentifierDumers in the database
        /// </summary>
        /// <param name="notifier"></param>
        public void Check(ICheckNotifier notifier)
        {
            _allAnoTables = _repository.GetAllObjects<ANOTable>().ToArray();

            CheckAllANOTables(notifier);
            CheckFieldnames(notifier);
        }

        private void CheckFieldnames(ICheckNotifier notifier)
        {
            foreach (ColumnInfo col in _repository.GetAllObjects<ColumnInfo>())
                col.Check(notifier);
        }

        private void CheckAllANOTables(ICheckNotifier notifier)
        {

            if (!_allAnoTables.Any())
                notifier.OnCheckPerformed(new CheckEventArgs("There are no ANOTables in the data catalogue",
                    CheckResult.Warning));

            foreach (var ano in _allAnoTables)
            {
                try
                {
                    ano.Check(new ThrowImmediatelyCheckNotifier());
                    notifier.OnCheckPerformed(new CheckEventArgs(ano.TableName + " is synchronized", CheckResult.Success, null));
                }
                catch (ANOConfigurationException exception)
                {
                    notifier.OnCheckPerformed(new CheckEventArgs(exception.Message, CheckResult.Fail, exception));
                }
            }


            //build a dictionary for performance, no sense querying the server once for every catalogue
            Dictionary<TableInfo, PreLoadDiscardedColumn[]> InfoForIdentifierDump = new Dictionary<TableInfo, PreLoadDiscardedColumn[]>();

            PreLoadDiscardedColumn[] columns = _repository.GetAllObjects<PreLoadDiscardedColumn>().ToArray();

            foreach (TableInfo tableInfo in _repository.GetAllObjects<TableInfo>().ToArray())
                InfoForIdentifierDump.Add(tableInfo, columns.Where(col => col.TableInfo_ID == tableInfo.ID && 
                   col.GoesIntoIdentifierDump()).ToArray());

            foreach (TableInfo tableInfo in InfoForIdentifierDump.Keys)
            {
                //if there are no d
                if (!InfoForIdentifierDump[tableInfo].Any())
                    continue;

                try
                {
                    var identifierDumper = new IdentifierDumper(tableInfo);
                    identifierDumper.Check(new ThrowImmediatelyCheckNotifier());
                    notifier.OnCheckPerformed(new CheckEventArgs("Identifier dump " + identifierDumper.GetRuntimeName() + " passed validation", CheckResult.Success, null));
                }
                catch (ANOConfigurationException exception)
                {
                    notifier.OnCheckPerformed(new CheckEventArgs(exception.Message, CheckResult.Fail, exception));
                }
                catch (Exception exception)
                {
                    notifier.OnCheckPerformed(new CheckEventArgs("Fatal Error:" + exception.Message, CheckResult.Fail, exception));
                }

            }
        }
    }
}
