// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.DataViewing;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Repositories.Construction;
using ReusableLibraryCode.Icons.IconProvision;
using System.Drawing;
using System.IO;

namespace Rdmp.Core.CommandExecution.AtomicCommands.DataViewing
{
    /// <summary>
    /// Run the SQL for a full <see cref="CohortIdentificationConfiguration"/> and which should
    /// return a list of identifiers from live datasets/query cache
    /// </summary>
    public class ExecuteCommandViewCohortIdentificationConfiguration : ExecuteCommandViewDataBase, IAtomicCommand
    {
        private readonly CohortIdentificationConfiguration _cic;
        private readonly bool _useCache;

        [UseWithObjectConstructor]
        public ExecuteCommandViewCohortIdentificationConfiguration(IBasicActivateItems activator,
            [DemandsInitialization("The cohort builder query you wan to execute")]
            CohortIdentificationConfiguration cic,
            [DemandsInitialization("True (default) to use the query cache (if available) or false to run suppressing caching")]
            bool useCache = true,
            [DemandsInitialization(ToFileDescription)]
            FileInfo toFile = null) : base(activator,toFile)
        {
            this._cic = cic;
            this._useCache = useCache;
            SuggestedCategory = "View";
        }

        public override string GetCommandName()
        {
            if (_useCache)
                return "Query Builder SQL/Results";

            return "Query Builder SQL/Results (No Cache)";
        }
        public override Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.SQL);
        }
        protected override IViewSQLAndResultsCollection GetCollection()
        {
            return new ViewCohortIdentificationConfigurationSqlCollection(_cic)
            {
                UseQueryCache = _useCache
            };
        }
    }
}