using System;
using CatalogueLibrary.Data.Aggregation;

namespace CatalogueWebService.Modules.Data
{
    public class AggregateData
    {
        public int ID { get; set; }
        public int CatalogueID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime Created { get; set; }

        public AggregateData(AggregateConfiguration config)
        {
            ID = config.ID;
            CatalogueID = config.Catalogue_ID;
            Name = config.Name;
            Description = config.Description;
            Created = config.dtCreated;
        }
    }
}