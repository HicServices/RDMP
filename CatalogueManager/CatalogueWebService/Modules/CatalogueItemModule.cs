using System.Collections.Generic;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using CatalogueWebService.Modules.Data;
using Nancy;
using RDMPStartup;

namespace CatalogueWebService.Modules
{
    public class CatalogueItemModule : NancyModule
    {
        public CatalogueItemModule(IRDMPPlatformRepositoryServiceLocator locator)
        {
            Get["/catalogues/{id}/items"] = parameters =>
            {

                Catalogue catalogue = locator.CatalogueRepository.GetObjectByID<Catalogue>(parameters.id);

                if (catalogue == null)
                    return Response.AsJson(new ErrorData
                    {
                        Message = "Catalogue " + parameters.id + " not found"
                    }, HttpStatusCode.NotFound);

                var catalogueItems = catalogue.CatalogueItems;
                var itemDataList = new List<CatalogueItemData>();

                foreach (var item in catalogueItems)
                    itemDataList.Add(new CatalogueItemData(item));

                return Response.AsJson(itemDataList);
            };
        }
    }
}