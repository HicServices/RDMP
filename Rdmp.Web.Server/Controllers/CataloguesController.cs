using Microsoft.AspNetCore.Mvc;
using Rdmp.Core.Curation.Data;
using Rdmp.Web.Server.Models;

namespace Rdmp.Web.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CataloguesController : ControllerBase
    {
        private readonly ILogger<CataloguesController> _logger;

        public CataloguesController(ILogger<CataloguesController> logger)
        {
            _logger = logger;
        }
        [HttpGet(Name = "GetCatalogues")]
        public IEnumerable<object> Get()
        {
            var catalogues = RDMPInitialiser._startup.RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>();
            return catalogues.Select(x => new CatalogueModel(x.ID, x.Name, x.Description));
        }
    }
}
