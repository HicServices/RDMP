using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Dataset.Confluence;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    public class ExecuteCommandExportCataloguesToConfluence : BasicCommandExecution, IAtomicCommand
    {

        private readonly  IBasicActivateItems _activator;
        private readonly string _subdomain;
        private readonly  int _spaceId;
        private readonly string _apiKey;
        private readonly string _owner;
        private readonly string _description;
        private readonly HttpClient _client = new();

        private class ConfluencePageBody
        {
            public string representation { get; set; } = "storage";
            public string value { get; set; }
        }
        private class ConfluencePagePostRequest
        {
            public string spaceId { get; set; }
            public string status { get; set; } = "current";
            public string title { get; set; } = "Catalogues";
            public string parentId { get; set; }
            public ConfluencePageBody body { get; set; }

        }

        private class ConfluencePagePostResponse
        {
            public string id { get; set; }
        }

        public ExecuteCommandExportCataloguesToConfluence(IBasicActivateItems activator, string subdomain, int spaceId, string apiKey, string owner, string description)
        {
            _activator = activator;
            _subdomain = subdomain;
            _spaceId = spaceId;
            _apiKey = apiKey;
            _owner = owner;
            _description = description;
        }

        public override  void Execute()
        {
            //todo needs to do updates also
            base.Execute();
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", _apiKey);

            var catalogues = _activator.RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>()
                .Where(c => !c.IsColdStorageDataset && !c.IsDeprecated && !c.IsInternalDataset && !c.IsProjectSpecific(_activator.RepositoryLocator.DataExportRepository))
                .ToList();
            var builder = new ConfluencePageBuilder(catalogues,_owner,_description);
            var rootPageHTML = builder.BuildRootPage();
            var uri = $"https://{_subdomain}.atlassian.net/wiki/api/v2/pages";
            var request = new ConfluencePagePostRequest()
            {
                spaceId = _spaceId.ToString(),
                title = $"{_owner} Catalogues",
                body = new ConfluencePageBody() { value = rootPageHTML }
            };
            HttpResponseMessage response = Task.Run(async () => await _client.PostAsJsonAsync(uri, request)).Result;
            if(response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var content = Task.Run(async () => await response.Content.ReadAsStringAsync()).Result;
                var responseContent = JsonConvert.DeserializeObject<ConfluencePagePostResponse>(content);
                string rootParentId = responseContent.id;
                if (rootParentId != null)
                {
                    foreach (var catalogue in catalogues)
                    {
                        var cataloguePageHTML = builder.BuildHTMLForCatalogue(catalogue);
                        request = new ConfluencePagePostRequest()
                        {
                            spaceId = _spaceId.ToString(),
                            title = catalogue.Name,
                            parentId = rootParentId,
                            body = new ConfluencePageBody() { value = cataloguePageHTML }
                        };
                        response = Task.Run(async () => await _client.PostAsJsonAsync(uri, request)).Result;

                    }
                }
                else
                {
                    Console.WriteLine($"Unable to find root id in response - {response.Content.ToString()}");

                }
            }
            else
            {
                Console.WriteLine($"Unable to create root page  - {response.StatusCode}");
            }
        }

    }
}
