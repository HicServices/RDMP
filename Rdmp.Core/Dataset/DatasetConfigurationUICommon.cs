// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Rdmp.Core.CohortCreation.Execution;
using Rdmp.Core.CohortCreation.Execution.Joinables;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Cohort.Joinables;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.QueryCaching.Aggregation;

namespace Rdmp.Core.Dataset;

/// <summary>
/// Common methods used by Cohort Builder UI implementations.  Eliminates
/// code duplication and makes it possible to add new UI formats later
/// e.g. web/console etc
/// </summary>
public class DatasetConfigurationUICommon
{
    //public CohortIdentificationConfiguration Configuration;

    //public ExternalDatabaseServer QueryCachingServer;
    //private CohortAggregateContainer _root;
    //private CancellationTokenSource _cancelGlobalOperations;
    //private ISqlParameter[] _globals;
    //public CohortCompilerRunner Runner;

    ///// <summary>
    ///// User interface layer for modal dialogs, showing Exceptions etc
    ///// </summary>
    //public IBasicActivateItems Activator;

    ///// <summary>
    ///// Duration in seconds to allow tasks to run for before cancelling
    ///// </summary>
    //public int Timeout = 3000;

    //public CohortCompiler Compiler { get; }



    /// <summary>
    /// User interface layer for modal dialogs, showing Exceptions etc
    /// </summary>
    public IBasicActivateItems Activator;

    public Curation.Data.Dataset Dataset;


    public DatasetConfigurationUICommon()
    {
        //Compiler = new CohortCompiler(null);
    }

   
}