﻿// Copyright (c) The University of Dundee 2018-2023
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.
using FAnsi.Naming;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Curation.Data;

/// <summary>
/// This class stores the redacted CHIs that are found during data load of via catalogue mutilation
/// </summary>
public interface IRedactedCHI :IMapsDirectlyToDatabaseTable
{

    ICatalogueRepository CatalogueRepository { get; }

    string PotentialCHI { get; }
    string CHIContext{ get; }

    string CHILocation { get; }
}