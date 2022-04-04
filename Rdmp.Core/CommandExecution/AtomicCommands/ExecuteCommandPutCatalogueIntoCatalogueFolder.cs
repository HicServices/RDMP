// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.Curation.Data;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    public class ExecuteCommandPutCatalogueIntoCatalogueFolder: BasicCommandExecution
    {
        private readonly Catalogue[] _catalogues;
        private readonly string _targetModel;

        public ExecuteCommandPutCatalogueIntoCatalogueFolder(IBasicActivateItems activator, CatalogueCombineable cmd, string targetModel)
            :this(activator,new []{cmd.Catalogue},targetModel)
        {
            
        }
        public ExecuteCommandPutCatalogueIntoCatalogueFolder(IBasicActivateItems activator, ManyCataloguesCombineable cmd, string targetModel)
            : this(activator, cmd.Catalogues, targetModel)
        {
            
        }

        private ExecuteCommandPutCatalogueIntoCatalogueFolder(IBasicActivateItems activator, Catalogue[] catalogues, string targetModel) : base(activator)
        {
            _targetModel = targetModel;
            _catalogues = catalogues;
        }
        public override void Execute()
        {
            base.Execute();

            foreach (Catalogue c in _catalogues)
            {
                c.Folder = _targetModel;
                c.SaveToDatabase();
            }

            //Catalogue folder has changed so publish the change (but only change the last Catalogue so we don't end up subing a million global refreshes changes)
            Publish(_catalogues.Last());
        }
    }
}