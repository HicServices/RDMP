// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data.Cohort;

namespace Rdmp.Core.Providers.Nodes;

/// <summary>
///     The RDMP is designed to store sensitive clinical datasets and make them available in research ready (anonymous)
///     form.  This usually requires governance approval from the data
///     provider.  This node lets you create periods of governance for your datasets (See GovernancePeriodUI).
/// </summary>
public class AllGovernanceNode : SingletonNode, IOrderable
{
    public int Order
    {
        get => -5000;
        set { }
    }

    public AllGovernanceNode() : base("Governance")
    {
    }
}