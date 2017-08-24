using System.Collections.Generic;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;

namespace CatalogueLibrary
{
    public class ForwardEngineerCatalogue
    {
        private readonly TableInfo _tableInfo;
        private readonly ColumnInfo[] _columnInfos;
        private readonly bool _markAllExtractable;
        private readonly CatalogueRepository _repository;

        public ForwardEngineerCatalogue(TableInfo tableInfo, ColumnInfo[] columnInfos, bool markAllExtractable = false)
        {
            _repository = (CatalogueRepository)tableInfo.Repository;
            _tableInfo = tableInfo;
            _columnInfos = columnInfos;
            _markAllExtractable = markAllExtractable;
        }

        public void ExecuteForwardEngineering(out Catalogue catalogue, out CatalogueItem[] items, out ExtractionInformation[] extractionInformations)
        {
            ExecuteForwardEngineering(null, out catalogue, out items, out extractionInformations);
        }

        public void ExecuteForwardEngineering()
        {
            Catalogue whoCaresCata;
            CatalogueItem[] whoCaresItems;
            ExtractionInformation[] whoCaresInformations;

            ExecuteForwardEngineering(null,out whoCaresCata,out whoCaresItems,out whoCaresInformations);
        }

        public void ExecuteForwardEngineering(Catalogue intoExistingCatalogue)
        {
            Catalogue whoCaresCata;
            CatalogueItem[] whoCaresItems;
            ExtractionInformation[] whoCaresInformations;

            ExecuteForwardEngineering(intoExistingCatalogue, out whoCaresCata, out whoCaresItems, out whoCaresInformations);
        }

        public void ExecuteForwardEngineering(Catalogue intoExistingCatalogue,out Catalogue catalogue, out CatalogueItem[] catalogueItems, out ExtractionInformation[] extractionInformations)
        {
            var repo = (CatalogueRepository)_tableInfo.Repository;

            //if user did not specify an existing catalogue to supplement 
            if (intoExistingCatalogue == null)
                //create a new (empty) catalogue and treat that as the new target
                intoExistingCatalogue = new Catalogue(repo, _tableInfo.GetRuntimeName());

            catalogue = intoExistingCatalogue;
            List<CatalogueItem> catalogueItemsCreated = new List<CatalogueItem>();
            List<ExtractionInformation> extractionInformationsCreated = new List<ExtractionInformation>();

            int order = 0;

            //for each column we will add a new one to the 
            foreach (ColumnInfo col in _columnInfos)
            {
                order++;
                
                //create it with the same name
                CatalogueItem cataItem = new CatalogueItem(repo, intoExistingCatalogue, col.Name.Substring(col.Name.LastIndexOf(".") + 1).Trim('[', ']', '`'));
                catalogueItemsCreated.Add(cataItem);

                if (_markAllExtractable)
                {
                    var newExtractionInfo = new ExtractionInformation(repo, cataItem, col, col.Name);
                    newExtractionInfo.Order = order;
                    newExtractionInfo.SaveToDatabase();
                    extractionInformationsCreated.Add(newExtractionInfo);
                }
                else
                {
                    cataItem.ColumnInfo_ID =  col.ID;
                    cataItem.SaveToDatabase();
                }
            }

            extractionInformations = extractionInformationsCreated.ToArray();
            catalogueItems = catalogueItemsCreated.ToArray();
        }
    }
}