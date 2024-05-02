// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi.Discovery;
using Rdmp.Core.CohortCommitting;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
///     Creates a new database in which to store final cohort lists of a given private linkage identifier name and format
///     (e.g. CHI).
/// </summary>
internal sealed class ExecuteCommandCreateNewCohortStore : BasicCommandExecution
{
    private readonly IBasicActivateItems activator;
    private readonly DiscoveredDatabase databaseToCreate;
    private readonly bool allowNullReleaseIdentifiers;
    private readonly string privateFieldName;
    private readonly string privateFieldDataType;

    /// <summary>
    ///     The cohort store created
    /// </summary>
    internal ExternalCohortTable Created;


    public ExecuteCommandCreateNewCohortStore(IBasicActivateItems activator,
        [DemandsInitialization("The database to create")]
        DiscoveredDatabase databaseToCreate,
        [DemandsInitialization(
            "True to allow null values in the release identifier field.  Set to true if you want to do your own custom release identifier allocation later e.g. via a stored proc")]
        bool allowNullReleaseIdentifiers,
        [DemandsInitialization("Name of the private identifier field in your datasets e.g. chi")]
        string privateFieldName,
        [DemandsInitialization(
            "Sql datatype (of your DBMS) that the private identifier field should have e.g. varchar(10)")]
        string privateFieldDataType) : base(activator)
    {
        this.activator = activator;
        this.databaseToCreate = databaseToCreate;
        this.allowNullReleaseIdentifiers = allowNullReleaseIdentifiers;
        this.privateFieldName = privateFieldName;
        this.privateFieldDataType = privateFieldDataType;
    }

    public override void Execute()
    {
        base.Execute();

        //Create cohort store database
        var wizard = new CreateNewCohortDatabaseWizard(databaseToCreate,
            activator.RepositoryLocator.CatalogueRepository, activator.RepositoryLocator.DataExportRepository,
            allowNullReleaseIdentifiers);
        Created = wizard.CreateDatabase(new PrivateIdentifierPrototype(privateFieldName, privateFieldDataType),
            ThrowImmediatelyCheckNotifier.Quiet);

        Publish(Created);
    }
}