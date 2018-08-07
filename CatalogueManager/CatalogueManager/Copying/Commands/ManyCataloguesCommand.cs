using CatalogueLibrary.Data;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.Copying.Commands
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