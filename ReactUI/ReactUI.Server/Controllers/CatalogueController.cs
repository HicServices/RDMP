using Microsoft.AspNetCore.Mvc;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories;

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

        [HttpGet(Name = "GetCatalogues")]
        public IEnumerable<object> Get()
        {
            var catalogues = RDMPInitialiser._startup.RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>();
            return catalogues.Select(x => new { x.Name, x.Description, x.ID });
        }
    }
}
