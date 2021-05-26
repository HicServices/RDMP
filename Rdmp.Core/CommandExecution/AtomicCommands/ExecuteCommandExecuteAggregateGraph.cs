// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using System.IO;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.DataExport.DataExtraction;
using Rdmp.Core.DataViewing;
using Rdmp.Core.Icons.IconProvision;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    public class ExecuteCommandExecuteAggregateGraph : BasicCommandExecution, IAtomicCommand
    {
        private readonly AggregateConfiguration _aggregate;
        private readonly FileInfo _toFile;

        public ExecuteCommandExecuteAggregateGraph(IBasicActivateItems activator, AggregateConfiguration aggregate, FileInfo toFile=null) : base(activator)
        {
            _aggregate = aggregate;
            this._toFile = toFile;
            if (aggregate.IsCohortIdentificationAggregate)
                SetImpossible("AggregateConfiguration is a Cohort aggregate");

            SetImpossibleIfFailsChecks(aggregate);

            UseTripleDotSuffix = true;
        }

        public override string GetCommandHelp()
        {
            return "Assembles and runs the graph query and renders the results as a graph";
        }

        public override void Execute()
        {
            base.Execute();

            if(_toFile != null)
            {
                var collection = new ViewAggregateExtractUICollection(_aggregate);
                var point = collection.GetDataAccessPoint();
                var db = DataAccessPortal.GetInstance().ExpectDatabase(point, DataAccessContext.InternalDataProcessing);
                using (var fs = File.OpenWrite(_toFile.FullName))
                {
                    var toRun = new ExtractTableVerbatim(db.Server, collection.GetSql(),fs, ",", null);
                    toRun.DoExtraction();
                }   
            }
            else
            {
                BasicActivator.ShowGraph(_aggregate);
            }
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return CatalogueIcons.Graph;
        }
    }
}
