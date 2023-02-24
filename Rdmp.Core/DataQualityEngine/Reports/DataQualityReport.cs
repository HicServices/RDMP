// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Threading;
using Rdmp.Core.Curation.Data;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataQualityEngine.Reports;

public abstract class DataQualityReport : IDataQualityReport
{
    protected ICatalogue _catalogue;
    public abstract void Check(ICheckNotifier notifier);

    public virtual bool CatalogueSupportsReport(ICatalogue c)
    {
        _catalogue = c;

        var checkNotifier = new ToMemoryCheckNotifier();
        Check(checkNotifier);

        return checkNotifier.GetWorst() <= CheckResult.Warning;

    }
    public abstract void GenerateReport(ICatalogue c, IDataLoadEventListener listener, CancellationToken cancellationToken);
}