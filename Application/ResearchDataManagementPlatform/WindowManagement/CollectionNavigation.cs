// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi.Discovery;
using Rdmp.Core.CommandExecution;
using System.Collections.Generic;
using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace ResearchDataManagementPlatform.WindowManagement
{
    /// <summary>
    /// Records the fact that the user visited a specific object in a tree collection
    /// </summary>
    public class CollectionNavigation: INavigation
    {
        public IMapsDirectlyToDatabaseTable Object { get; }

        public bool IsAlive
        {
            get
            {
                if(Object is IMightNotExist o)
                    return o.Exists();

                return true;
            }
        } 

        public CollectionNavigation(IMapsDirectlyToDatabaseTable Object)
        {
            this.Object = Object;
        }

        public void Activate(ActivateItems activateItems)
        {
            activateItems.RequestItemEmphasis(this,new EmphasiseRequest(Object,0));
        }

        public void Close()
        {
            
        }
        public override string ToString()
        {
            return Object.ToString();
        }

        public override bool Equals(object obj)
        {
            return obj is CollectionNavigation other &&
                   Object.Equals(other.Object);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return 162302186 + EqualityComparer<IMapsDirectlyToDatabaseTable>.Default.GetHashCode(Object);
            }
        }
    }
}
