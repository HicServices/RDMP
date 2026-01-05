// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using FAnsi.Discovery;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Modules.Mutilators;

/// <summary>
/// Widens columns on the specified table to be varchar(max).  This is useful for delaying truncation errors till later in the load.  For example if you
/// apply a TableVarcharMaxer as a Mounting operation before loading data into it with an IAttacher you can have all the data loaded successfully into RAW where
/// it can be interrogated with SQL to find out what the truncated fields are and what to do about them.
/// 
/// <para>Remember that RAW and STAGING are created based on the LIVE table schema (but that RAW has no column constraints like pks or not null fields).  This
/// component lets you further relax the structure of RAW to have varchar(max) column datatypes.  The load will still crash when it comes to migration to
/// STAGING or merging with LIVE because the datatypes are not valid according to LIVE but you will have an easier time debugging than trying to look through
/// a flat file for problematic values.</para>
/// </summary>
public class TableVarcharMaxer : MatchingTablesMutilator
{
    [DemandsInitialization(
        "By default (false) the mutilator will only expand columns with an SQL Type containing 'char', if set to true then this will do all columns including decimals, dates etc",
        defaultValue: false)]
    public bool AllDataTypes { get; set; }

    [DemandsInitialization("The type that all matching columns will be converted into", defaultValue: "varchar(max)",
        typeOf: null, mandatory: true)]
    public string DestinationType { get; set; }

    public TableVarcharMaxer() : base(LoadStage.Mounting, LoadStage.AdjustRaw)
    {
    }

    public override void Check(ICheckNotifier notifier)
    {
        base.Check(notifier);

        if (DbInfo != null)
            try
            {
                DbInfo.Server.GetQuerySyntaxHelper().TypeTranslater.GetCSharpTypeForSQLDBType(DestinationType);
                notifier.OnCheckPerformed(new CheckEventArgs("DestinationType is supported", CheckResult.Success));
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs($"DestinationType ({DestinationType}) is not supported",
                    CheckResult.Warning, e));
            }
    }

    protected override void MutilateTable(IDataLoadEventListener job, ITableInfo tableInfo, DiscoveredTable table)
    {
        foreach (var col in table.DiscoverColumns())
            if (AllDataTypes || col.DataType.GetLengthIfString() >= 1)
                try
                {
                    col.DataType.AlterTypeTo(DestinationType);
                    job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                        $"converted column {col} to {DestinationType}"));
                }
                catch (Exception e)
                {
                    job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error,
                        $"Failed to convert column {col} of data type {col.DataType} to destination type {DestinationType}",
                        e));
                }
    }
}