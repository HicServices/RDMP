using System.Collections.Generic;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Data.Dashboarding
{
    public interface IPersistableObjectCollection
    {
        PersistStringHelper Helper { get; }

        List<IMapsDirectlyToDatabaseTable> DatabaseObjects { get; set; }
        
        string SaveExtraText();
        void LoadExtraText(string s);
    }
}