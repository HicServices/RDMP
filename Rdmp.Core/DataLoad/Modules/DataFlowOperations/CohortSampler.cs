// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CohortCommitting.Pipeline;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataFlowPipeline.Requirements;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Rdmp.Core.DataLoad.Modules.DataFlowOperations
{
    /// <summary>
    /// Component for reproducibly pulling a random sample of records from a cohort being committed.
    /// </summary>
    public class CohortSampler : IPluginDataFlowComponent<DataTable>, IPipelineRequirement<CohortCreationRequest>
    {
        private IExternalCohortTable _ect;
        private IProject _project;
        bool _firstBatch = true;

        [DemandsInitialization("The number of unique patient identifiers you want returned from the input data")]
        public int SampleSize { get; set; }

        [DemandsInitialization("Optional.  The name of the identifier column that you are submitting.  Set this if it is different than the destination cohort private identifier field")]
        public string PrivateIdentifierColumnName { get; set; }

        [DemandsInitialization("Determines components behaviour if not enough unique identifiers are being comitted.  True to crash.  False to pass on however many records there are.")]
        public bool FailIfNotEnoughIdentifiers { get; set; }

        public void Abort(IDataLoadEventListener listener)
        {
        }

        public void Check(ICheckNotifier notifier)
        {
        }

        public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
        }

        public void PreInitialize(CohortCreationRequest value, IDataLoadEventListener listener)
        {
            _ect = value.NewCohortDefinition.LocationOfCohort;
            _project = value.Project;
        }

        public DataTable ProcessPipelineData(DataTable toProcess, IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            if (!_firstBatch)
                throw new Exception("Expected to get the whole cohort at once but got multiple batches.  This component only works if the Source returns all data at once");

            if (_project.ProjectNumber == null)
                throw new Exception("Project must have a ProjectNumber so that it can be used as a seed in random cohort sampling");

            var expectedFieldName = GetPrivateFieldName();
            
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,$"Looking for column called '{expectedFieldName}' in the data in order to produce a sample"));

            if (!toProcess.Columns.Contains(expectedFieldName))
                throw new Exception($"Could not find a column called {expectedFieldName} in the data");


            // get all the unique values
            HashSet<object> uniques = new HashSet<object>();

            foreach(DataRow row in toProcess.Rows)
            {
                var val = row[expectedFieldName];

                if(val != DBNull.Value)
                {
                    uniques.Add(val);
                }
            }

            _firstBatch = false;


            Random r = new Random(_project.ProjectNumber.Value);

#pragma warning disable SCS0005 // Weak random number generator.
            var chosen = uniques.OrderBy(v => r.Next()).Take(SampleSize).ToList();
#pragma warning restore SCS0005 // Weak random number generator.

            if(chosen.Count < SampleSize && FailIfNotEnoughIdentifiers)
            {
                throw new Exception($"Cohort only contains {chosen.Count} unique identifiers.  This is less than the requested sample size of {SampleSize} and {nameof(FailIfNotEnoughIdentifiers)} is true");
            }

            DataTable dtToReturn = new DataTable();
            dtToReturn.Columns.Add(expectedFieldName);

            foreach(var val in chosen)
            {
                dtToReturn.Rows.Add(val);
            }

            return dtToReturn;
        }

        private string GetPrivateFieldName()
        {
            if (!string.IsNullOrWhiteSpace(PrivateIdentifierColumnName))
                return PrivateIdentifierColumnName;

            var syntax = _ect.GetQuerySyntaxHelper();
            return syntax.GetRuntimeName(_ect.PrivateIdentifierField);
        }
    }
}
