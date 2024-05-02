// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataLoad.Engine.Pipeline.Components.Anonymisation;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.DataLoad.Engine.Checks.Checkers;

/// <summary>
///     Wrapper class for checking Anonymisation configurations (if any) of a TableInfo.  The wrapped classes are
///     ANOTableInfoSynchronizer and IdentifierDumper.
/// </summary>
public class AnonymisationChecks : ICheckable
{
    private readonly TableInfo _tableInfo;

    public AnonymisationChecks(TableInfo tableInfo)
    {
        _tableInfo = tableInfo;
    }

    public void Check(ICheckNotifier notifier)
    {
        //check ANO stuff is synchronized
        notifier.OnCheckPerformed(new CheckEventArgs("Preparing to synchronize ANO configuration",
            CheckResult.Success));

        var synchronizer = new ANOTableInfoSynchronizer(_tableInfo);

        try
        {
            synchronizer.Synchronize(notifier);
        }
        catch (Exception e)
        {
            notifier.OnCheckPerformed(new CheckEventArgs(
                $"Synchronization of Anonymisation configurations of table {_tableInfo.GetRuntimeName()} failed with Exception",
                CheckResult.Fail, e));
        }

        if (_tableInfo.IdentifierDumpServer_ID != null)
        {
            var identifierDumper = new IdentifierDumper(_tableInfo);
            identifierDumper.Check(notifier);
        }
    }
}