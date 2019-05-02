// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MapsDirectlyToDatabaseTableUI;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Databases;
using Rdmp.UI.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    public class ExecuteCommandSetQueryCachingDatabase : BasicUICommandExecution, IAtomicCommand
    {
        private readonly CohortIdentificationConfiguration _cic;
        private ExternalDatabaseServer[] _caches;

        public ExecuteCommandSetQueryCachingDatabase(IActivateItems activator,CohortIdentificationConfiguration cic) : base(activator)
        {
            _cic = cic;

            _caches = Activator.RepositoryLocator.CatalogueRepository.GetAllObjects<ExternalDatabaseServer>()
                .Where(s => s.WasCreatedBy(new QueryCachingPatcher())).ToArray();

            if(!_caches.Any())
                SetImpossible("There are no Query Caching databases set up");
        }

        public override void Execute()
        {
            base.Execute();

            var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(_caches,true,false);
            
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (dialog.Selected == null)
                    _cic.QueryCachingServer_ID = null;
                else
                    _cic.QueryCachingServer_ID = dialog.Selected.ID;

                _cic.SaveToDatabase();
                Publish(_cic);
            }
        }

        public override string GetCommandName()
        {
            return _cic.QueryCachingServer_ID == null ? "Set Query Cache":"Change Query Cache";
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.ExternalDatabaseServer,OverlayKind.Link);
        }
    }
}