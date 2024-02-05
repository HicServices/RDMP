using Rdmp.Core.Curation.Data;

namespace ReactUI.Server.Models
{
    public class CatalogueModel
    {
        public string Name { get; set; }
        public int ID { get; set; }
        public string Description { get; set; }

        public CatalogueModel() { }
        public CatalogueModel(int id, string name, string description)
        {
            Name = name;
            ID = id;
            Description = description;
        }

        public CatalogueModel(Catalogue catalogue)
        {
            Name = catalogue.Name;
            ID = catalogue.ID;
            Description = catalogue.Description;
        }


    }
}
