// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.CatalogueLibrary.Data;
using Rdmp.Core.CatalogueLibrary.Data.DataLoad;
using Rdmp.Core.DataLoad.Engine.Pipeline.Components.Anonymisation;
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
        /// Calls .Check on all ANOTables and IdentifierDumper logic (PreLoadDiscardedColumns etc) in the database
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
