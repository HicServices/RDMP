﻿// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CommandExecution;

namespace Rdmp.Core.Dataset;

/// <summary>
/// Common methods used by Cohort Builder UI implementations.  Eliminates
/// code duplication and makes it possible to add new UI formats later
/// e.g. web/console etc
/// </summary>
public class DatasetConfigurationUICommon
{
     /// <summary>
    /// User interface layer for modal dialogs, showing Exceptions etc
    /// </summary>
    public IBasicActivateItems Activator;

    public Curation.Data.Dataset Dataset;


    public DatasetConfigurationUICommon()
    {
    }

   
}