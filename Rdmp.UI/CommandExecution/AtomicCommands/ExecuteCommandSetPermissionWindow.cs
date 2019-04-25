// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using System.Linq;
using Rdmp.Core.CatalogueLibrary.Data;
using Rdmp.Core.CatalogueLibrary.Data.Cache;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.UI.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandSetPermissionWindow : BasicUICommandExecution,IAtomicCommandWithTarget
    {
        private readonly CacheProgress _cacheProgress;
        private PermissionWindow _window;
        
        public ExecuteCommandSetPermissionWindow(IActivateItems activator, CacheProgress cacheProgress) : base(activator)
        {
            _cacheProgress = cacheProgress;
            _window = null;

            if(!activator.CoreChildProvider.AllPermissionWindows.Any())
                SetImpossible("There are no PermissionWindows created yet");
        }

        public override string GetCommandHelp()
        {
            return "Restrict caching execution to the given time period";
        }

        public override void Execute()
        {
            base.Execute();

            if(_window == null)
                _window = SelectOne<PermissionWindow>(Activator.RepositoryLocator.CatalogueRepository);

            if(_window == null)
                return;

            _cacheProgress.PermissionWindow_ID = _window.ID;
            _cacheProgress.SaveToDatabase();

            Publish(_cacheProgress);
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.PermissionWindow, OverlayKind.Link);
        }

        public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            var window = target as PermissionWindow;
            if (window != null)
                _window = window;

            return this;
        }
    }
}