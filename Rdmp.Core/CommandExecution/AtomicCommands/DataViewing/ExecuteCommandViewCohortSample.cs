// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataViewing;
using System.IO;

namespace Rdmp.Core.CommandExecution.AtomicCommands.DataViewing
{
    /// <summary>
    /// Fetches the private and release identifiers for a given <see cref="ExtractableCohort"/> optionally to file
    /// </summary>
    class ExecuteCommandViewCohortSample : ExecuteCommandViewDataBase
    {
        private readonly bool _includeCohortID;

        public ExtractableCohort Cohort { get; }
        public int Sample { get; }


        public ExecuteCommandViewCohortSample(IBasicActivateItems activator,
            [DemandsInitialization("The cohort that you want to fetch records for")]
            ExtractableCohort cohort,
            [DemandsInitialization("Optional. The maximum number of records to retrieve")]
            int sample = 100,
            [DemandsInitialization(ToFileDescription)]
            FileInfo toFile = null,

            [DemandsInitialization("True to include the OriginId of the cohort when extracting",DefaultValue = true)]
            bool includeCohortID = true):base(activator,toFile)
        {
            Cohort = cohort;
            Sample = sample;
            _includeCohortID = includeCohortID;
        }

        protected override IViewSQLAndResultsCollection GetCollection()
        {
            return new ViewCohortExtractionUICollection(Cohort)
            {
                Top = Sample,
                IncludeCohortID = _includeCohortID
            };
        }
    }
}
