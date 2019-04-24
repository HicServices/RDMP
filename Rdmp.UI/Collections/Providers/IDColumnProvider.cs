// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using BrightIdeasSoftware;
using MapsDirectlyToDatabaseTable;

namespace CatalogueManager.Collections.Providers
{
    /// <summary>
    /// Handles creating the ID column in a tree list view where the ID is populated for all models of Type IMapsDirectlyToDatabaseTable and null
    /// for all others
    /// </summary>
    public class IDColumnProvider
    {
        private readonly TreeListView _tree;

        public IDColumnProvider(TreeListView tree)
        {
            _tree = tree;
        }

        private object IDColumnAspectGetter(object rowObject)
        {
            var imaps = rowObject as IMapsDirectlyToDatabaseTable;

            if (imaps == null)
                return null;

            return imaps.ID;
        }

        public OLVColumn CreateColumn()
        {
            var toReturn = new OLVColumn();
            toReturn.Text = "ID";
            toReturn.IsVisible = false;
            toReturn.AspectGetter += IDColumnAspectGetter;
            toReturn.IsEditable = false;
            return toReturn;
        }
    }
}
