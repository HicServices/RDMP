// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using FAnsi.Discovery;
using Rdmp.Core.Curation.ANOEngineering;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.EntityNaming;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Engine.Mutilators;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Modules.Mutilators.Dilution;

/// <summary>
///     load component responsible for diluting data.  For example data might be diluted from date of birth to a bit flag
///     indicating only whether it is known or
///     not (alternatively it might round the date to the first of the month etc).  This mutilation occurs after migration
///     from RAW to STAGING (at which point
///     the undiluted values will have been stored in the IdentifierDump).  The mutilation might change the data type of
///     the column (e.g. from date to bit in the
///     above example) based on the user specified IDilutionOperation.
///     <para>
///         This operation MUST only appear in AdjustStaging.  It works in concert with
///         <see cref="PreLoadDiscardedColumn" />s.  Create a PreLoadDiscardedColumn
///         with Destination=Dilution, this operation can then be used to mutilate the value (for example cutting off the
///         ends of postcodes).  The pristene (un-mutilated)
///         value will be stored in the IdentifierDump along with all the other dumped columns but the LIVE will also
///         contain the mutilated value
///     </para>
///     <para>
///         Checking for this component is quite good and should detect incompatible Types (where LIVE column does not
///         match the IDilutionOperation), missing columns
///         / dump server configuration etc.
///     </para>
/// </summary>
public class Dilution : IPluginMutilateDataTables
{
    [DemandsInitialization(
        "Column which is to be diluted, must have Destination=Dilution - i.e. not Destination=Oblivion or StoreInIdentifierDump",
        Mandatory = true)]
    public PreLoadDiscardedColumn ColumnToDilute { get; set; }

    [DemandsInitialization("Dilution Operation to be performed on the column", DemandType.Unspecified, null,
        typeof(IDilutionOperation), Mandatory = true)]
    public Type Operation { get; set; }

    [DemandsInitialization(
        "The number of seconds to wait before timing out when executing the Operation.  This will be running an UPDATE on every record in STAGING so should be quite high depending on how many records your load loads at once (e.g. 5000)",
        DemandType.Unspecified, 5000)]
    public int Timeout { get; set; }

    public void Check(ICheckNotifier notifier)
    {
        if (ColumnToDilute == null)
        {
            notifier.OnCheckPerformed(new CheckEventArgs("ColumnToDilute is null", CheckResult.Fail));
            return;
        }

        if (ColumnToDilute.Destination != DiscardedColumnDestination.Dilute)
            notifier.OnCheckPerformed(new CheckEventArgs(
                $"ColumnToDilute '{ColumnToDilute.GetRuntimeName()}' is not marked as DiscardedColumnDestination.Dilute",
                CheckResult.Fail));

        //Stamp out the type
        IDilutionOperation instance = null;
        try
        {
            var factory = new DilutionOperationFactory(ColumnToDilute);
            instance = factory.Create(Operation);
        }
        catch (Exception e)
        {
            notifier.OnCheckPerformed(
                new CheckEventArgs(
                    $"Could not create DilutionOperation of Type {Operation} using DilutionOperationFactory, see inner Exception for details",
                    CheckResult.Fail, e));
        }

        if (_loadStage != LoadStage.AdjustStaging)
            notifier.OnCheckPerformed(new CheckEventArgs(
                $"Dilution can ONLY occur in load stage AdjustStaging, it is currently configured as load stage: {_loadStage}",
                CheckResult.Fail));

        instance?.Check(notifier);
    }


    public void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventsListener)
    {
    }

    private DiscoveredDatabase _dbInfo;
    private LoadStage _loadStage;

    public void Initialize(DiscoveredDatabase dbInfo, LoadStage loadStage)
    {
        _dbInfo = dbInfo;
        _loadStage = loadStage;
    }

    public ExitCodeType Mutilate(IDataLoadJob job)
    {
        var namer = job.Configuration.DatabaseNamer;

        using var con = _dbInfo.Server.GetConnection();
        con.Open();
        UsefulStuff.ExecuteBatchNonQuery(GetMutilationSql(namer), con, timeout: Timeout);

        return ExitCodeType.Success;
    }

    private string GetMutilationSql(INameDatabasesAndTablesDuringLoads namer)
    {
        var factory = new DilutionOperationFactory(ColumnToDilute);
        return factory.Create(Operation).GetMutilationSql(namer);
    }
}