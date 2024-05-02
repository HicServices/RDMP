// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Diagnostics;
using System.Threading;
using Rdmp.Core.CohortCreation.Execution;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.CohortCreation;

/// <summary>
///     A cohort identification container (AggregateContainer) or sub query (AggregateConfiguration) that is running in a
///     CohortCompiler and will be
///     given the results of the execution (CohortIdentificationTaskExecution).
/// </summary>
public interface ICompileable : IOrderable
{
    IMapsDirectlyToDatabaseTable Child { get; }
    int Timeout { get; set; }

    CancellationToken CancellationToken { get; set; }
    CancellationTokenSource CancellationTokenSource { get; set; }
    CompilationState State { set; get; }

    event EventHandler StateChanged;
    Exception CrashMessage { get; set; }

    string Log { get; set; }

    int FinalRowCount { get; set; }
    int? CumulativeRowCount { get; set; }

    IDataAccessPoint[] GetDataAccessPoints();

    Stopwatch Stopwatch { get; set; }
    TimeSpan? ElapsedTime { get; }

    bool IsEnabled();

    string GetCachedQueryUseCount();

    void SetKnownContainer(CohortAggregateContainer parent, bool isFirstInContainer);
}