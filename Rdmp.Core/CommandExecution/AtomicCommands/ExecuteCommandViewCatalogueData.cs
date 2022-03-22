// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataViewing;
using Rdmp.Core.Icons.IconProvision;
using ReusableLibraryCode.Icons.IconProvision;
using System.Drawing;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    /// <summary>
    /// View data referenced by a <see cref="Catalogue"/>.  This may be all extractable columns from the underlying table; a subset; or aggregate of multiple tables
    /// </summary>
    public class ExecuteCommandViewCatalogueData : BasicCommandExecution
    {
        private readonly Catalogue _catalogue;
        private readonly int numberToFetch;

        public ExecuteCommandViewCatalogueData(IBasicActivateItems activator, Catalogue catalogue,int numberToFetch):base(activator)
        {
            this._catalogue = catalogue;
            this.numberToFetch = numberToFetch;
        }

        public override void Execute()
        {
            base.Execute();
            var collection = new ViewCatalogueDataCollection(_catalogue);

            if(numberToFetch > 0)
                collection.TopX = numberToFetch;

            BasicActivator.ShowData(collection);
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.SQL);
        }
    }
}
