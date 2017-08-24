using CatalogueLibrary.Data;

namespace CatalogueWebService.Modules.Data
{
    public class CatalogueItemData
    {
        public int Id { get; set; }
        public int CatalogueId { get; set; }
        public string Name { get; set; }
        public string StatisticalCons { get; set; }
        public string ResearchRelevance { get; set; }
        public string Description { get; set; }
        public string Topic { get; set; }
        public string AggMethod { get; set; }
        public string Limitations { get; set; }
        public string Comments { get; set; }
        public Catalogue.CataloguePeriodicity Periodicity { get; set; }

        public CatalogueItemData(CatalogueItem item)
        {
            Id = item.ID;
            CatalogueId = item.Catalogue_ID;
            Name = item.Name;
            StatisticalCons = item.Statistical_cons;
            ResearchRelevance = item.Research_relevance;
            Description = item.Description;
            Topic = item.Topic;
            AggMethod = item.Agg_method;
            Limitations = item.Limitations;
            Comments = item.Comments;
            Periodicity = item.Periodicity;
        }
    }
}