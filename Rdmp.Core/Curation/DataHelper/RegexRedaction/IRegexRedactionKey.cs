﻿// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.Curation.DataHelper.RegexRedaction;

/// <summary>
/// Stores the PKs for a redaction 
/// </summary>
public interface IRegexRedactionKey: IMapsDirectlyToDatabaseTable
{
    ICatalogueRepository CatalogueRepository { get; }
    int RegexRedaction_ID { get; protected set; }
    int ColumnInfo_ID { get; protected set; }
    string Value { get; protected set; }
}
