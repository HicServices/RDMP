// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using SixLabors.ImageSharp;
using System.Linq;
using FAnsi.Discovery;
using MapsDirectlyToDatabaseTable.Versioning;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Databases;
using Rdmp.Core.Icons.IconOverlays;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Icons.IconProvision.StateBasedIconProviders;
using Rdmp.Core.Repositories.Construction;
using ReusableLibraryCode;
using ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    public class ExecuteCommandCreateNewExternalDatabaseServer : BasicCommandExecution, IAtomicCommand
    {
        private readonly PermissableDefaults _defaultToSet;
        private ExternalDatabaseServerStateBasedIconProvider _databaseIconProvider;
        private IconOverlayProvider _overlayProvider;
        private IPatcher _patcher;
        private DiscoveredDatabase _database;

        public ExternalDatabaseServer ServerCreatedIfAny { get; private set; }


        [UseWithObjectConstructor]
        public ExecuteCommandCreateNewExternalDatabaseServer(IBasicActivateItems activator, PermissableDefaults defaultToSet,DiscoveredDatabase toCreate) 
            : this(activator,defaultToSet == PermissableDefaults.None ? null : defaultToSet.ToTier2DatabaseType(),defaultToSet)
        {
            _database = toCreate;
        }

        public ExecuteCommandCreateNewExternalDatabaseServer(IBasicActivateItems activator, IPatcher patcher, PermissableDefaults defaultToSet) : base(activator)
        {
            _patcher = patcher;
            _defaultToSet = defaultToSet;

            _overlayProvider = new IconOverlayProvider();
            _databaseIconProvider = new ExternalDatabaseServerStateBasedIconProvider(_overlayProvider);

            //do we already have a default server for this?
            var existingDefault = BasicActivator.ServerDefaults.GetDefaultFor(_defaultToSet);

            if (existingDefault != null)
                SetImpossible("There is already an existing " + _defaultToSet + " database");
        }

        public override string GetCommandName()
        {
            if (_defaultToSet != PermissableDefaults.None)
                return string.Format("Create New {0} Server...", UsefulStuff.PascalCaseStringToHumanReadable(_defaultToSet.ToString().Replace("_ID", "").Replace("Live", "").Replace("ANO", "Anonymisation")));

            if (_patcher != null)
                return string.Format("Create New {0} Server...", _patcher.Name);

            return base.GetCommandName();
        }

        public override string GetCommandHelp()
        {
            switch (_defaultToSet)
            {
                case PermissableDefaults.LiveLoggingServer_ID:
                    return "Creates a database for auditing all flows of data (data load, extraction etc) including tables for errors, progress tables/record count loaded etc";
                case PermissableDefaults.IdentifierDumpServer_ID:
                    return "Creates a database for storing the values of intercepted columns that are discarded during data load because they contain identifiable data";
                case PermissableDefaults.DQE:
                    return "Creates a database for storing the results of data quality engine runs on your datasets over time.";
                case PermissableDefaults.WebServiceQueryCachingServer_ID:
                    break;
                case PermissableDefaults.RAWDataLoadServer:
                    return "Defines which database server should be used for the RAW data in the RAW=>STAGING=>LIVE model of the data load engine";
                case PermissableDefaults.ANOStore:
                    return "Creates a new anonymisation database which contains mappings of identifiable values to anonymous representations";
                case PermissableDefaults.CohortIdentificationQueryCachingServer_ID:
                    return "Creates a new Query Cache database which contains the indexed results of executed subqueries in a CohortIdentificationConfiguration";
            }

            return "Defines a new server that can be accessed by RDMP";
        }

        public override void Execute()
        {
            base.Execute();

            //user wants to create a new server e.g. a new Logging server
            if (_patcher == null)
                ServerCreatedIfAny = new ExternalDatabaseServer(BasicActivator.RepositoryLocator.CatalogueRepository, "New ExternalDatabaseServer " + Guid.NewGuid(), _patcher);
            else
            {
                //create the new server
                ServerCreatedIfAny = BasicActivator.CreateNewPlatformDatabase(
                    BasicActivator.RepositoryLocator.CatalogueRepository,
                    _defaultToSet,
                    _patcher,
                    _database);
            }

            //user cancelled creating a server
            if (ServerCreatedIfAny == null)
                return;

            Publish(ServerCreatedIfAny);
            Activate(ServerCreatedIfAny);
        }

        public override Image<Argb32> GetImage(IIconProvider iconProvider)
        {
            if (_patcher != null)
            {
                var basicIcon = _databaseIconProvider.GetIconForAssembly(_patcher.GetDbAssembly());
                return _overlayProvider.GetOverlay(basicIcon, OverlayKind.Add);
            }

            return iconProvider.GetImage(RDMPConcept.ExternalDatabaseServer, OverlayKind.Add);
        }
    }
}