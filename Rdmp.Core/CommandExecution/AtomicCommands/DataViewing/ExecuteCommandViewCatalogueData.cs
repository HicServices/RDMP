// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataViewing;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Repositories.Construction;
using ReusableLibraryCode.Icons.IconProvision;
using System.Drawing;
using System.IO;

namespace Rdmp.Core.CommandExecution.AtomicCommands.DataViewing
{
    /// <summary>
    /// View data referenced by a <see cref="Catalogue"/>.  This may be all extractable columns from the underlying table; a subset; or aggregate of multiple tables
    /// </summary>
    public class ExecuteCommandViewCatalogueData : ExecuteCommandViewDataBase
    {
        private readonly Catalogue _catalogue;
        private readonly int numberToFetch;

        [UseWithObjectConstructor]
        public ExecuteCommandViewCatalogueData(IBasicActivateItems activator,

            [DemandsInitialization("The dataset to read data from")]
            Catalogue catalogue,

            [DemandsInitialization("The number of records to fetch or <= 0 to fetch all",DefaultValue = -1)]
            int numberToFetch = -1,

            [DemandsInitialization(ToFileDescription)]
            FileInfo toFile = null) :base(activator,toFile)
        {
            this._catalogue = catalogue;
            this.numberToFetch = numberToFetch;
        }
        protected override IViewSQLAndResultsCollection GetCollection()
        {
            return new ViewCatalogueDataCollection(_catalogue)
            {
                TopX = numberToFetch > 0 ? numberToFetch : null
            };
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.SQL);
        }
    }
}
