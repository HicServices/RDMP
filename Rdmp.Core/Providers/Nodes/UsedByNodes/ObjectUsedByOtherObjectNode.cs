// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;

namespace Rdmp.Core.Providers.Nodes.UsedByNodes
{
    public class ObjectUsedByOtherObjectNode<T, T2> : Node, IObjectUsedByOtherObjectNode<T,T2> 
        where T:class 
        where T2:class 
    {
        public T User { get; set; }
        public T2 ObjectBeingUsed { get; private set; }

        public bool IsEmptyNode
        {
            get { return ObjectBeingUsed == null; }
        }

        public ObjectUsedByOtherObjectNode(T user, T2 objectBeingUsed)
        {
            User = user;
            ObjectBeingUsed = objectBeingUsed;
        }

        public object MasqueradingAs()
        {
            return ObjectBeingUsed;
        }
        public override string ToString()
        {
            if (IsEmptyNode)
                return "???";

            return ObjectBeingUsed.ToString();
        }

        #region Equality
        protected bool Equals(ObjectUsedByOtherObjectNode<T, T2> other)
        {
            return EqualityComparer<T>.Default.Equals(User, other.User) && EqualityComparer<T2>.Default.Equals(ObjectBeingUsed, other.ObjectBeingUsed);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ObjectUsedByOtherObjectNode<T, T2>) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (EqualityComparer<T>.Default.GetHashCode(User)*397) ^ EqualityComparer<T2>.Default.GetHashCode(ObjectBeingUsed);
            }
        }
        #endregion
    }
}
