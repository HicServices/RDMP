// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.TestsAndSetup.ServicePropogation;

namespace Rdmp.UI.Refreshing;

internal class SelfDestructProtocol<T> : IRefreshBusSubscriber where T : DatabaseEntity
{
    private readonly IActivateItems _activator;
    public RDMPSingleDatabaseObjectControl<T> User { get; private set; }
    public T OriginalObject { get; set; }

    public SelfDestructProtocol(RDMPSingleDatabaseObjectControl<T> user, IActivateItems activator, T originalObject)
    {
        _activator = activator;
        User = user;
        OriginalObject = originalObject ??
                         throw new System.Exception(
                             $"Could not construct tab for a null object. Control was '{User?.GetType()}'");
    }

    public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
    {
        var descendancy = e.DeletedObjectDescendancy ??
                          _activator.CoreChildProvider.GetDescendancyListIfAnyFor(e.Object);

        //implementation of the anonymous callback
        var o = e.Object as T;

        //if the descendancy contained our object Type we should also consider a refresh
        if (o == null && descendancy != null)
            o = (T)descendancy.Parents.LastOrDefault(p => p is T);

        //don't respond to events raised by the user themself!
        if (sender == User)
            return;

        //if the original object does not exist anymore (could be a CASCADE event so we do have to check it every time regardless of what object type is refreshing)
        if (!OriginalObject.Exists()) //object no longer exists!
        {
            var parent = User.ParentForm;
            if (parent is { IsDisposed: false })
                parent.Close(); //self destruct because object was deleted

            return;
        }

        if (o != null && o.ID == OriginalObject.ID &&
            o.GetType() == OriginalObject.GetType()) //object was refreshed, probably an update to some fields in it
            User.SetDatabaseObject(_activator, o); //give it the new object
    }
}