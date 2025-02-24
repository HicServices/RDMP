// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;

namespace Rdmp.UI.TestsAndSetup.ServicePropogation;

/// <summary>
/// Only use if you know what you are doing.  What you are doing is announcing that you cannot function on a single root database object alone (e.g. Project / ExtractionConfiguration etc).
/// and that you require a combination of objects and/or custom settings to be persisted/refreshed.  If you can manage with only one object (which you really should be able to) then use
/// RDMPSingleDatabaseObjectControl instead which is much easier to implement
/// 
/// <para>IObjectCollectionControls are controls driven by 0 or more database objects and optional persistence string (stored in an IPersistableObjectCollection).  The lifecycle of the control
/// is that it is Activated (probably by an IActivateItems control class) with a fully hydrated IPersistableObjectCollection.  This collection should be pretty immutable and will be saved
/// into the persistence text file when the application is exited (via PersistableObjectCollectionDockContent)</para>
/// 
/// </summary>
public interface IObjectCollectionControl : IRDMPControl, ILifetimeSubscriber, INamedTab
{
    /// <summary>
    /// Provides a fully hydrated collection either created by a user action or by deserializing a persistence string in PersistableObjectCollectionDockContent.  Either way the
    /// collection will be fully hydrated.
    /// </summary>
    /// <param name="activator"></param>
    /// <param name="collection"></param>
    void SetCollection(IActivateItems activator, IPersistableObjectCollection collection);

    /// <summary>
    /// Used to serialize the control for later use e.g. on application exit, you must only return your collection, the rest is handled by the IPersistableObjectCollection itself or
    /// the PersistableObjectCollectionDockContent
    /// </summary>
    /// <returns></returns>
    IPersistableObjectCollection GetCollection();
}