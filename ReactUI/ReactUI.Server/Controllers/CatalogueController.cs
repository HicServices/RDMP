using Microsoft.AspNetCore.Mvc;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories;
using ReactUI.Server.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ReactUI.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CatalogueController : ControllerBase
    {
        private readonly ILogger<CatalogueController> _logger;

        public CatalogueController(ILogger<CatalogueController> logger)
        {
            _logger = logger;
        }
        [HttpGet(Name = "GetCatalogueById")]
        public CatalogueModel GetCatalogueById(int id)
        {
            var catalogue = RDMPInitialiser._startup.RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>().Where(c => c.ID == id).First();
            if(catalogue is not null)
            {
                return new CatalogueModel(catalogue);
            }
            return null;
        }

        [HttpPatch(Name = "UpdateCatalogue")]
        public CatalogueModel UpdateCatalogue(int id, string body) {
            var catalogue = RDMPInitialiser._startup.RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>().Where(c => c.ID == id).First();
            var x = JsonSerializer.Deserialize<Dictionary<string, string>>(body);

            if (catalogue is not null)
            {
                catalogue.Description = x["description"] is not null ? x["description"] : catalogue.Description;
                catalogue.SaveToDatabase();
                return new CatalogueModel(catalogue);
            }
            return null;

        }
    }
}
