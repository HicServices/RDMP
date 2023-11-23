using Microsoft.AspNetCore.Mvc;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories;
using ReactUI.Server.Models;

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

        //[HttpGet(Name = "GetCatalogues")]
        //public IEnumerable<object> Get()
        //{
        //    var catalogues = RDMPInitialiser._startup.RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>();
        //    return catalogues.Select(x => new CatalogueModel(x.ID,x.Name,x.Description));
        //}
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
    }
}
