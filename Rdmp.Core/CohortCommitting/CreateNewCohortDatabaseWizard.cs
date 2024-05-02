// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.CohortCommitting.Pipeline.Destinations.IdentifierAllocation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using TypeGuesser;

namespace Rdmp.Core.CohortCommitting;

/// <summary>
///     Creates an ExternalCohortTable database implementation.  The implementation will be based on your live
///     IsExtractionIdentifier columns
///     (PrivateIdentifierPrototype) and a release identifier allocation strategy (
///     <see cref="IAllocateReleaseIdentifiers" />)
///     e.g. varchar(10) private patient identifier gets mapped to a new GUID.
///     <para>
///         This implementation is intended to be a basic solution only and lacks advanced features such having the same
///         release identifier for the same primary
///         key in subsequent versions of the same cohort (generally you want 1 - m private identifiers because you don't
///         want people to be able to link patients
///         across project extracts they are working on).
///     </para>
///     <para>See UserManual.md for more information on how to tailor the resulting database to fit your needs.</para>
/// </summary>
public class CreateNewCohortDatabaseWizard
{
    private bool AllowNullReleaseIdentifiers { get; }
    private readonly ICatalogueRepository _catalogueRepository;
    private readonly IDataExportRepository _dataExportRepository;
    private readonly DiscoveredDatabase _targetDatabase;

    private const string ReleaseIdentifierFieldName = "ReleaseId";
    private const string DefinitionTableForeignKeyField = "cohortDefinition_id";


    public CreateNewCohortDatabaseWizard(DiscoveredDatabase targetDatabase, ICatalogueRepository catalogueRepository,
        IDataExportRepository dataExportRepository, bool allowNullReleaseIdentifiers)
    {
        AllowNullReleaseIdentifiers = allowNullReleaseIdentifiers;
        _catalogueRepository = catalogueRepository;
        _dataExportRepository = dataExportRepository;
        _targetDatabase = targetDatabase;
    }

    public PrivateIdentifierPrototype[] GetPrivateIdentifierCandidates()
    {
        //get the extraction identifiers
        var extractionInformations = _catalogueRepository.GetAllObjects<ExtractionInformation>()
            .Where(ei => ei.IsExtractionIdentifier);

        //name + datatype, ideally we want to find 30 fields called 'PatientIndex' in 30 datasets all as char(10) fields but more likely we will get a slew of different spellings and dodgy datatypes (varchar(max) etc)
        var toReturn = new List<PrivateIdentifierPrototype>();

        //for each extraction identifier get the name of the column and give the associated data type
        foreach (var extractionInformation in extractionInformations)
        {
            //do not process ExtractionInformations when the ColumnInfo is COLUMNINFO_MISSING
            if (extractionInformation.ColumnInfo == null || !extractionInformation.ColumnInfo.Exists())
                continue;

            var match = toReturn.SingleOrDefault(prototype => prototype.IsCompatible(extractionInformation));

            if (match != null)
                match.MatchingExtractionInformations.Add(extractionInformation);
            else
                toReturn.Add(new PrivateIdentifierPrototype(extractionInformation));
        }

        return toReturn.ToArray();
    }

    public ExternalCohortTable CreateDatabase(PrivateIdentifierPrototype privateIdentifierPrototype,
        ICheckNotifier notifier)
    {
        var tt = _targetDatabase.Server.GetQuerySyntaxHelper().TypeTranslater;


        if (tt.GetLengthIfString(privateIdentifierPrototype.DataType) == int.MaxValue)
            throw new Exception(
                "Private identifier datatype cannot be varchar(max) style as this prevents Primary Key creation on the table");

        if (!_targetDatabase.Exists())
        {
            notifier.OnCheckPerformed(new CheckEventArgs(
                $"Did not find database {_targetDatabase} on server so creating it", CheckResult.Success));
            _targetDatabase.Create();
        }

        try
        {
            var definitionTable = _targetDatabase.CreateTable("CohortDefinition", new[]
            {
                new DatabaseColumnRequest("id", new DatabaseTypeRequest(typeof(int)))
                    { AllowNulls = false, IsAutoIncrement = true, IsPrimaryKey = true },
                new DatabaseColumnRequest("projectNumber", new DatabaseTypeRequest(typeof(int))) { AllowNulls = false },
                new DatabaseColumnRequest("version", new DatabaseTypeRequest(typeof(int))) { AllowNulls = false },
                new DatabaseColumnRequest("description", new DatabaseTypeRequest(typeof(string), 3000))
                    { AllowNulls = false },
                new DatabaseColumnRequest("dtCreated", new DatabaseTypeRequest(typeof(DateTime)))
                    { AllowNulls = false, Default = MandatoryScalarFunctions.GetTodaysDate }
            });


            var idColumn = definitionTable.DiscoverColumn("id");
            var foreignKey =
                new DatabaseColumnRequest(DefinitionTableForeignKeyField, new DatabaseTypeRequest(typeof(int)), false)
                    { IsPrimaryKey = true };

            // Look up the collations of all the private identifier columns
            var collations = privateIdentifierPrototype.MatchingExtractionInformations
                .Select(e => e.ColumnInfo?.Collation)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .ToArray();

            var cohortTable = _targetDatabase.CreateTable("Cohort", new[]
                {
                    new DatabaseColumnRequest(privateIdentifierPrototype.RuntimeName,
                        privateIdentifierPrototype.DataType, false)
                    {
                        IsPrimaryKey = true,

                        // if there is a single collation amongst private identifier prototype references we must use that collation
                        // when creating the private column so that the DBMS can link them no bother
                        Collation = collations.Length == 1 ? collations[0] : null
                    },
                    new DatabaseColumnRequest(ReleaseIdentifierFieldName, new DatabaseTypeRequest(typeof(string), 300))
                        { AllowNulls = AllowNullReleaseIdentifiers },
                    foreignKey
                }
                ,
                //foreign key between id and cohortDefinition_id
                new Dictionary<DatabaseColumnRequest, DiscoveredColumn> { { foreignKey, idColumn } }, true);


            notifier.OnCheckPerformed(new CheckEventArgs("About to create pointer to the source", CheckResult.Success));
            var pointer = new ExternalCohortTable(_dataExportRepository, "TestExternalCohort",
                _targetDatabase.Server.DatabaseType)
            {
                DatabaseType = _targetDatabase.Server.DatabaseType,
                Server = _targetDatabase.Server.Name,
                Database = _targetDatabase.GetRuntimeName(),
                Username = _targetDatabase.Server.ExplicitUsernameIfAny,
                Password = _targetDatabase.Server.ExplicitPasswordIfAny,
                Name = _targetDatabase.GetRuntimeName(),
                TableName = cohortTable.GetRuntimeName(),
                PrivateIdentifierField = privateIdentifierPrototype.RuntimeName,
                ReleaseIdentifierField = ReleaseIdentifierFieldName,
                DefinitionTableForeignKeyField = DefinitionTableForeignKeyField,
                DefinitionTableName = definitionTable.GetRuntimeName()
            };

            pointer.SaveToDatabase();

            notifier.OnCheckPerformed(new CheckEventArgs(
                "successfully created reference to cohort source in data export manager", CheckResult.Success));

            notifier.OnCheckPerformed(new CheckEventArgs("About to run post creation checks", CheckResult.Success));
            pointer.Check(notifier);

            notifier.OnCheckPerformed(new CheckEventArgs("Finished", CheckResult.Success));

            return pointer;
        }
        catch (Exception e)
        {
            notifier.OnCheckPerformed(
                new CheckEventArgs("Entire setup failed with exception (double click to find out why)",
                    CheckResult.Fail, e));
            return null;
        }
    }
}