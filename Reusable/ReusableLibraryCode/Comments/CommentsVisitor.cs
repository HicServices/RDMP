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
