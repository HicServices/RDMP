// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CatalogueLibrary;
using CatalogueLibrary.DataFlowPipeline;
using DataLoadEngine.Job;
using DataLoadEngine.LoadExecution.Components;
using DataLoadEngine.LoadExecution.Delegates;

namespace DataLoadEngine.LoadExecution
{
    /// <summary>
    /// See SingleJobExecution
    /// </summary>
    public interface IDataLoadExecution
    {
        List<IDataLoadComponent> Components { get; }
        ExitCodeType Run(IDataLoadJob job, GracefulCancellationToken executionCancellationToken);
    }

    
}