// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.UI.TestsAndSetup.ServicePropogation;

namespace Rdmp.UI.DashboardTabs.Construction;

/// <summary>
/// Unlike IObjectCollectionControl (the base interface) these objects (which must have an ultimate base class UserControl) will be displayed at random locations on a DashboardLayoutUI.
/// Your control will have to be able to be constructed without parameters (blank constructor) and allow the user to make meaningful changes to what is displayed (select objects etc). All
/// state changes must be recorded in an IPersistableObjectCollection which will be used for persistence into the Catalogue database (do not put sensitive information into your persistence
/// string).
/// 
/// <para>The lifecycle for your control is:
/// 1. blank constructor called
/// 2. ConstructEmptyCollection called
/// 3. Collection hydrated out of database
/// 4. SetCollection called </para>
/// 
/// <para>When the user makes important changes on your control you can use the DashboardControl.SaveCollectionState method to persist the list of objects/persistence string on your collection</para>
/// 
/// <para> You should build your IPersistableObjectCollection to make use of the Helper for serialization.
/// You should build your IPersistableObjectCollection to handle missing/empty argument dictionaries (serialization has a null/empty persistence string)</para>
///
/// <para>Since you need a blank constructor anyway this shouldn't be too hard</para>
/// 
/// <para>Finally you should name the collection to match the UI control e.g.:</para>
/// 
/// <para>GoodBadCataloguePieChart
/// GoodBadCataloguePieChartObjectCollection</para>
/// 
/// </summary>
public interface IDashboardableControl : IObjectCollectionControl, INotifyMeOfEditState
{
    /// <summary>
    /// unlike regular Dashboardable controls
    /// </summary>
    /// <returns></returns>
    IPersistableObjectCollection ConstructEmptyCollection(DashboardControl databaseRecord);
}