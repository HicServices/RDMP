using CatalogueLibrary.Data;
using ReusableUIComponents.Copying;

namespace RDMPObjectVisualisation.Copying.Commands
{
    public class ManyCataloguesCommand : ICommand
    {
        public Catalogue[] Catalogues { get; set; }

        public ManyCataloguesCommand(Catalogue[] catalogues)
        {
            Catalogues = catalogues;
        }

        public string GetSqlString()
        {
            return null;
        }
    }
}