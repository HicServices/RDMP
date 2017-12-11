using CatalogueLibrary.Data;

namespace CatalogueLibrary.Nodes
{
    /// <summary>
    /// Container tree node for all the documentation bits of a Catalogue including SupportingDocuments and SupportingSqlTables 
    /// </summary>
    public class DocumentationNode
    {
        public Catalogue Catalogue { get; set; }
        public SupportingDocument[] SupportingDocuments { get; set; }
        public SupportingSQLTable[] SupportingSQLTables { get; set; }

        public DocumentationNode(Catalogue catalogue, SupportingDocument[] supportingDocuments, SupportingSQLTable[] supportingSQLTables)
        {
            Catalogue = catalogue;
            SupportingDocuments = supportingDocuments;
            SupportingSQLTables = supportingSQLTables;
        }

        public override string ToString()
        {
            return "Documentation";
        }

        protected bool Equals(DocumentationNode other)
        {
            return Equals(Catalogue, other.Catalogue);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (DocumentationNode)) return false;
            return Equals((DocumentationNode) obj);
        }

        public override int GetHashCode()
        {
            return (Catalogue != null ? Catalogue.GetHashCode() : 0);
        }
    }
}