// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Text.RegularExpressions;
using NuDoq;

namespace ReusableLibraryCode.Comments
{
    class CommentsVisitor:Visitor
    {
        Regex rMultiSpaces = new Regex(@"  +");

        private readonly CommentStore _store;

        public CommentsVisitor(CommentStore store)
        {
            _store = store;
        }

        public override void VisitClass(Class type)
        {
            if (type.Info != null && !string.IsNullOrWhiteSpace(type.ToText()))
                Add(type.Info.Name, type.ToText());

            base.VisitClass(type);
        }

        public override void VisitInterface(Interface type)
        {
            if (type.Info != null && !string.IsNullOrWhiteSpace(type.ToText()))
                Add(type.Info.Name, type.ToText());
            
            base.VisitInterface(type);
        }

        private void Add(string name, string description)
        {
            var adjusted = rMultiSpaces.Replace(description, " ");
            _store.Add(name, adjusted);
        }

        public override void VisitProperty(Property property)
        {
            base.VisitProperty(property);

            if (property.Kind == MemberKinds.Property && property.Info != null && property.Info.DeclaringType != null)
                Add(property.Info.DeclaringType.Name + "." + property.Info.Name, property.ToText());
        }
    }
}
