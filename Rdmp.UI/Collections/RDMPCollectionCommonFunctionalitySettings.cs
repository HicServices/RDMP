// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Providers;
using Rdmp.UI.ItemActivation;

namespace Rdmp.UI.Collections
{
    /// <summary>
    /// Initialization arguments for <see cref="RDMPCollectionCommonFunctionality"/>.  Use this class to control what system default behaviours
    /// are exhibited by a given <see cref="RDMPCollectionUI"/> (tree view) e.g. whether the user can pin objects.
    /// </summary>
    public class RDMPCollectionCommonFunctionalitySettings
    {
        /// <summary>
        /// True to add an extra column to the tree view which shows if / allows changing the favourite objects status of objects.
        ///  <para>Defaults to true</para>
        /// </summary>
        public bool AddFavouriteColumn { get; set; }

        /// <summary>
        /// True to add an extra column (not visible by default) to the tree view which the ID property of objects that are
        ///  <see cref="MapsDirectlyToDatabaseTable.IMapsDirectlyToDatabaseTable"/>
        ///  <para>Defaults to true</para>
        /// </summary>
        public bool AddIDColumn { get; set; }

        /// <summary>
        /// False to automatically set up tree hierarchy children based on the <see cref="ICoreChildProvider"/> in the
        /// <see cref="IActivateItems"/> at construction time.  True if you plan to handle object children yourself
        ///  <para>Defaults to false</para>
        /// </summary>
        public bool SuppressChildrenAdder { get; set; }

        /// <summary>
        /// False to perform the default object activation behaviour on double click.  True if you plan to handle it yourself with a custom action.
        /// 
        /// <para>Defaults to false</para>
        /// </summary>
        public bool SuppressActivate { get; set; }

        /// <summary>
        /// True to add an extra column (Checks) to the tree which lets the user run checks on ICheckable things
        /// </summary>
        public bool AddCheckColumn { get; set; }

        /// <summary>
        /// False to prevent the user sorting columns (including any new columns created by <see cref="RDMPCollectionCommonFunctionality"/>)
        /// 
        /// <para>Defaults to true</para>
        /// </summary>
        public bool AllowSorting { get; set; }

        public RDMPCollectionCommonFunctionalitySettings()
        {
            AddFavouriteColumn = true;
            AddIDColumn = true;
            SuppressChildrenAdder = false;
            SuppressActivate = false;
            AddCheckColumn = true;
            AllowSorting = true;
        }
    }
}