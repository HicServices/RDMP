// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Windows.Forms;
using MapsDirectlyToDatabaseTable;

namespace Rdmp.UI.Refreshing
{
    /// <summary>
    /// <see cref="IRefreshBusSubscriber"/> for <see cref="Control"/> classes which want their lifetime to be linked to a single
    /// <see cref="IMapsDirectlyToDatabaseTable"/> object.  This grants notifications of publish events about the object and ensures
    /// your <see cref="Control"/> is closed when/if the object is deleted.
    ///
    /// <para>See <see cref="RefreshBus.EstablishLifetimeSubscription"/></para>
    /// </summary>
    public interface ILifetimeSubscriber:IContainerControl,IRefreshBusSubscriber
    {
    }
}