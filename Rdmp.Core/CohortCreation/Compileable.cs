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

public abstract class Compileable : ICompileable
{
    protected readonly CohortCompiler _compiler;
    private CompilationState _state;

    public CohortAggregateContainer ParentContainerIfAny { get; set; }
    public bool? IsFirstInContainer { get; set; }

    public string Log { get; set; }

    protected Compileable(CohortCompiler compiler)
    {
        _compiler = compiler;
    }

    public override string ToString()
    {
        return Child.ToString();
    }

    public string GetStateDescription()
    {
        return State.ToString();
    }

    public abstract string GetCatalogueName();

    public CancellationToken CancellationToken { set; get; }
    public CancellationTokenSource CancellationTokenSource { get; set; }

    public CompilationState State
    {
        set
        {
            _state = value;
            var h = StateChanged;
            h?.Invoke(this, EventArgs.Empty);
        }
        get => _state;
    }

    public virtual int Order
    {
        get => ((IOrderable)Child).Order;
        set => ((IOrderable)Child).Order = value;
    }

    public event EventHandler StateChanged;
    public Exception CrashMessage { get; set; }

    public int FinalRowCount { set; get; }

    public int? CumulativeRowCount { set; get; }

    public abstract IMapsDirectlyToDatabaseTable Child { get; }
    public int Timeout { get; set; }
    public abstract IDataAccessPoint[] GetDataAccessPoints();


    public Stopwatch Stopwatch { get; set; }

    public TimeSpan? ElapsedTime => Stopwatch?.Elapsed;

    public abstract bool IsEnabled();

    public string GetCachedQueryUseCount()
    {
        return _compiler.GetCachedQueryUseCount(this);
    }

    public void SetKnownContainer(CohortAggregateContainer parent, bool isFirstInContainer)
    {
        ParentContainerIfAny = parent;
        IsFirstInContainer = isFirstInContainer;
    }
}