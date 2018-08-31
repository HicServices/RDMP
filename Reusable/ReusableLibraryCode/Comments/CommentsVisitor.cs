using NuDoq;

namespace ReusableLibraryCode.Comments
{
    class CommentsVisitor:Visitor
    {
        private readonly CommentStore _store;

        public CommentsVisitor(CommentStore store)
        {
            _store = store;
        }

        public override void VisitClass(Class type)
        {
            if (type.Info != null && !string.IsNullOrWhiteSpace(type.ToText()))
                _store.Add(type.Info.Name, type.ToText());

            base.VisitClass(type);
        }

        public override void VisitInterface(Interface type)
        {
            if (type.Info != null && !string.IsNullOrWhiteSpace(type.ToText()))
                _store.Add(type.Info.Name, type.ToText());

            base.VisitInterface(type);
        }
    }
}
