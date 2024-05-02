// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.Core.Providers.Nodes;

/// <summary>
///     The saved cohort (list of patient identifiers) which will be linked with the datasets in the associated
///     <see cref="ExtractionConfiguration" />
/// </summary>
public class LinkedCohortNode : Node, IMasqueradeAs, IDeletableWithCustomMessage
{
    public IExtractionConfiguration Configuration { get; }
    public IExtractableCohort Cohort { get; }

    public LinkedCohortNode(IExtractionConfiguration configuration, IExtractableCohort cohort)
    {
        Configuration = configuration;
        Cohort = cohort;
    }

    public override string ToString()
    {
        return Cohort.ToString();
    }

    public object MasqueradingAs()
    {
        return Cohort;
    }

    protected bool Equals(LinkedCohortNode other)
    {
        return Equals(Configuration, other.Configuration) && Equals(Cohort, other.Cohort);
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((LinkedCohortNode)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Configuration, Cohort);
    }

    public void DeleteInDatabase()
    {
        Configuration.Cohort_ID = null;
        Configuration.SaveToDatabase();
    }

    public string GetDeleteMessage()
    {
        return $"remove cohort from ExtractionConfiguration '{Configuration}'";
    }

    /// <inheritdoc />
    public string GetDeleteVerb()
    {
        return "Remove";
    }
}