using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Dataset.Confluence;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.CodeDom;
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

        private readonly IBasicActivateItems _activator;
        private readonly string _subdomain;
        private readonly int _spaceId;
        private readonly string _apiKey;
        private readonly string _owner;
        private readonly string _description;
        private readonly HttpClient _client = new();
        private Dictionary<int, string> _cataloguePageLookups = new();

        private class ConfluencePageBody
        {
            public string representation { get; set; } = "storage";
            public string value { get; set; }
        }

        private class ConfluencePageVersion
        {
            public int number { get; set; }
        }

        private class ConfluencePagePostRequest
        {
            public string spaceId { get; set; }
            public string status { get; set; } = "current";
            public string title { get; set; } = "Catalogues";
            public string parentId { get; set; }
            public ConfluencePageBody body { get; set; }

        }

        private class ConfluencePagePutRequest
        {
            public int id { get; set; }
            public string status { get; set; } = "current";
            public string title { get; set; } = "Catalogues";
            public ConfluencePageBody body { get; set; }
            public ConfluencePageVersion version { get; set; }

        }

        private class ConfluenceLinks
        {
            public string webui { get; set; }
        }

        private class ConfluencePostError
        {
            public int status { get; set; }
            public string code { get; set; }
            public string title { get; set; }
            public string detail { get; set; }
        }
        private class ConfluencePostErrors
        {
            public List<ConfluencePostError> errors { get; set; }
        }

        private class ConfluenceGetResults
        {
            public List<ConfluenceGetResult> results { get; set; }
        }
        private class ConfluenceGetResult
        {
            public ConfluencePageVersion version { get; set; }
            public string id { get; set; }
        }

        private class ConfluencePagePostResponse
        {
            public string id { get; set; }
            public ConfluenceLinks _links { get; set; }
            public ConfluencePageVersion version { get; set; }
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

        public override void Execute()
        {
            //todo needs to do updates also
            base.Execute();
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", _apiKey);

            var catalogues = _activator.RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>()
                .Where(c => !c.IsColdStorageDataset && !c.IsDeprecated && !c.IsInternalDataset && !c.IsProjectSpecific(_activator.RepositoryLocator.DataExportRepository))
                .ToList();
            var builder = new ConfluencePageBuilder(catalogues, _owner, _description, _subdomain);
            var uri = $"https://{_subdomain}.atlassian.net/wiki/api/v2/pages";
            string rootParentId = null;
            int rootParentVersion = 0;

            //container page
            var rootPageHTML = builder.BuildContainerPage(_cataloguePageLookups);
            var request = new ConfluencePagePostRequest()
            {
                spaceId = _spaceId.ToString(),
                title = $"{_owner} Catalogues",
                body = new ConfluencePageBody() { value = rootPageHTML }
            };
            HttpResponseMessage response = Task.Run(async () => await _client.PostAsJsonAsync(uri, request)).Result;
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var content = Task.Run(async () => await response.Content.ReadAsStringAsync()).Result;
                var responseContent = JsonConvert.DeserializeObject<ConfluencePagePostResponse>(content);
                rootParentId = responseContent.id;
                rootParentVersion = responseContent.version.number;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var content = Task.Run(async () => await response.Content.ReadAsStringAsync()).Result;
                var responseContent = JsonConvert.DeserializeObject<ConfluencePostErrors>(content);
                if (responseContent.errors.Count == 1 && responseContent.errors[0].title.Contains("page with this title already exists"))
                {
                    //page already exists - update it
                    //need to get version
                    var getResult = Task.Run(async () => await _client.GetAsync($"{uri}?title={request.title}&space-id={_spaceId}")).Result;
                    var getContent = Task.Run(async () => await getResult.Content.ReadAsStringAsync()).Result;
                    var getObj = JsonConvert.DeserializeObject<ConfluenceGetResults>(getContent);
                    if (getObj.results.Count != 1)
                    {
                        throw new Exception($"Multiple pages in pace {_spaceId} with name {request.title}");
                    }
                    rootParentId = getObj.results[0].id;
                    rootParentVersion = getObj.results[0].version.number;

                    var putRequest = new ConfluencePagePutRequest()
                    {
                        id = int.Parse(rootParentId),
                        title = $"{_owner} Catalogues",
                        status = "current",
                        body = new ConfluencePageBody() { value = rootPageHTML },
                        version = new ConfluencePageVersion()
                        {
                            number = rootParentVersion + 1
                        }
                    };
                    response = Task.Run(async () => await _client.PutAsJsonAsync($"{uri}/{rootParentId}", putRequest)).Result;
                    var putContent = Task.Run(async () => await response.Content.ReadAsStringAsync()).Result;
                    var putResponseContent = JsonConvert.DeserializeObject<ConfluencePagePostResponse>(putContent);
                    rootParentId = putResponseContent.id;
                    rootParentVersion = putResponseContent.version.number;
                }
            }
            else
            {
                Console.WriteLine($"Unable to create root page  - {response.StatusCode}");
                return;
            }
            //catalogue pages
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
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var content = Task.Run(async () => await response.Content.ReadAsStringAsync()).Result;
                        var responseContent = JsonConvert.DeserializeObject<ConfluencePagePostResponse>(content);
                        _cataloguePageLookups.TryAdd(catalogue.ID, responseContent._links.webui);
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        var content = Task.Run(async () => await response.Content.ReadAsStringAsync()).Result;
                        var responseContent = JsonConvert.DeserializeObject<ConfluencePostErrors>(content);
                        if (responseContent.errors.Count == 1 && responseContent.errors[0].title.Contains("page with this title already exists"))
                        {
                            //already exists, do a put
                            //page already exists - update it
                            //need to get version
                            var getResult = Task.Run(async () => await _client.GetAsync($"{uri}?title={request.title}&space-id={_spaceId}")).Result;
                            var getContent = Task.Run(async () => await getResult.Content.ReadAsStringAsync()).Result;
                            var getObj = JsonConvert.DeserializeObject<ConfluenceGetResults>(getContent);
                            if (getObj.results.Count != 1)
                            {
                                throw new Exception($"Multiple pages in pace {_spaceId} with name {request.title}");
                            }
                            var id = getObj.results[0].id;
                            var version = getObj.results[0].version.number;

                            var cataloguePutRequest = new ConfluencePagePutRequest()
                            {
                                id = int.Parse(id),
                                title = request.title,
                                status = "current",
                                body = new ConfluencePageBody() { value = rootPageHTML },
                                version = new ConfluencePageVersion()
                                {
                                    number = version + 1
                                }
                            };
                            response = Task.Run(async () => await _client.PutAsJsonAsync($"{uri}/{id}", cataloguePutRequest)).Result;
                            var putContent = Task.Run(async () => await response.Content.ReadAsStringAsync()).Result;
                            var putResponseContent = JsonConvert.DeserializeObject<ConfluencePagePostResponse>(putContent);
                        }
                    }
                    else
                    {
                        //really bad
                    }
                }
                //re-add the links for the catalogue pages to the home page
                rootPageHTML = builder.BuildContainerPage(_cataloguePageLookups);
                var putRequest = new ConfluencePagePutRequest()
                {
                    id = int.Parse(rootParentId),
                    title = $"{_owner} Catalogues",
                    status = "current",
                    body = new ConfluencePageBody() { value = rootPageHTML },
                    version = new ConfluencePageVersion()
                    {
                        number = rootParentVersion + 1
                    }
                };
                response = Task.Run(async () => await _client.PutAsJsonAsync($"{uri}/{rootParentId}", putRequest)).Result;
            }
            else
            {
                Console.WriteLine($"Unable to find root id in response - {response.Content.ToString()}");

            }
        }

    }
}
