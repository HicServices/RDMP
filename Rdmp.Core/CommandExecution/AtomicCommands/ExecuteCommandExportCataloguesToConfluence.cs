using Newtonsoft.Json;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Dataset.Confluence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    /// <summary>
    /// Uses provided Confluence credentials to populate a tree of pages about the public catalogues in RDMP
    /// </summary>
    public class ExecuteCommandExportCataloguesToConfluence : BasicCommandExecution, IAtomicCommand
    {

        private readonly IBasicActivateItems _activator;
        private readonly string _subdomain;
        private readonly int _spaceId;
        private readonly string _apiKey;
        private readonly string _owner;
        private readonly string _description;
        private readonly HttpClient _client = new();
        private readonly Dictionary<int, string> _cataloguePageLookups = [];
        private string rootParentId = null;
        private int rootParentVersion = 0;
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

        private class ConfluencePageResponse
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

        private static ConfluencePageResponse ResponseToConfluenceResponseObject(HttpResponseMessage response)
        {
            var content = Task.Run(async () => await response.Content.ReadAsStringAsync()).Result;
            return JsonConvert.DeserializeObject<ConfluencePageResponse>(content);
        }

        private static ConfluencePostErrors PostResponseToConfluenceErrorsObject(HttpResponseMessage response)
        {
            var content = Task.Run(async () => await response.Content.ReadAsStringAsync()).Result;
            return JsonConvert.DeserializeObject<ConfluencePostErrors>(content);
        }

        private ConfluenceGetResults GetConfluencePage(string uri, string title)
        {
            var result = Task.Run(async () => await _client.GetAsync($"{uri}?title={title}&space-id={_spaceId}")).Result;
            var content = Task.Run(async () => await result.Content.ReadAsStringAsync()).Result;
            return JsonConvert.DeserializeObject<ConfluenceGetResults>(content);
        }


        private void UpdateContainerPage(ConfluencePageBuilder builder, string uri)
        {
            var rootPageHTML = builder.BuildContainerPage(_cataloguePageLookups);
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
            var response = Task.Run(async () => await _client.PutAsJsonAsync($"{uri}/{rootParentId}", putRequest)).Result;
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var putResponseContent = ResponseToConfluenceResponseObject(response);
                rootParentId = putResponseContent.id;
                rootParentVersion = putResponseContent.version.number;
            }
            else
            {
                throw new Exception("Unable to update container page");
            }
        }

        private bool PageAlreadyExists(ConfluencePostErrors errors)
        {
            return errors.errors.Count == 1 && errors.errors[0].title.Contains("page with this title already exists");
        }

        private void CreateContainerPage(ConfluencePageBuilder builder, string uri)
        {
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
                var confluencePostResponseObject = ResponseToConfluenceResponseObject(response);
                rootParentId = confluencePostResponseObject.id;
                rootParentVersion = confluencePostResponseObject.version.number;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var confluenceErrorsResponse = PostResponseToConfluenceErrorsObject(response);
                if (PageAlreadyExists(confluenceErrorsResponse))
                {
                    var foundPages = GetConfluencePage(uri, request.title);
                    if (foundPages.results.Count != 1)
                    {
                        throw new Exception($"Multiple pages in pace {_spaceId} with name {request.title}");
                    }
                    rootParentId = foundPages.results[0].id;
                    rootParentVersion = foundPages.results[0].version.number;
                    UpdateContainerPage(builder, uri);
                }
                else
                {
                    throw new Exception($"Unexpected Error when creating container page - {confluenceErrorsResponse.errors[0].title}");
                }
            }
            else
            {
                throw new Exception($"Unable to create root page  - {response.StatusCode}");
            }
        }

        private void CreateCataloguePage(Catalogue catalogue, ConfluencePageBuilder builder, string uri)
        {
            var cataloguePageHTML = builder.BuildHTMLForCatalogue(catalogue);
            var request = new ConfluencePagePostRequest()
            {
                spaceId = _spaceId.ToString(),
                title = catalogue.Name,
                parentId = rootParentId,
                body = new ConfluencePageBody() { value = cataloguePageHTML }
            };
            var response = Task.Run(async () => await _client.PostAsJsonAsync(uri, request)).Result;
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var responseContent = ResponseToConfluenceResponseObject(response);
                _cataloguePageLookups.TryAdd(catalogue.ID, responseContent._links.webui);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var responseContent = PostResponseToConfluenceErrorsObject(response);
                if (PageAlreadyExists(responseContent))
                {
                    var getResults = GetConfluencePage(uri, request.title);
                    if (getResults.results.Count != 1)
                    {
                        throw new Exception($"Multiple pages in pace {_spaceId} with name {request.title}");
                    }
                    var id = getResults.results[0].id;
                    var version = getResults.results[0].version.number;

                    var cataloguePutRequest = new ConfluencePagePutRequest()
                    {
                        id = int.Parse(id),
                        title = request.title,
                        status = "current",
                        body = new ConfluencePageBody() { value = cataloguePageHTML },
                        version = new ConfluencePageVersion()
                        {
                            number = version + 1
                        }
                    };
                    response = Task.Run(async () => await _client.PutAsJsonAsync($"{uri}/{id}", cataloguePutRequest)).Result;
                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        throw new Exception($"Unable to PUT update for catalogue '{catalogue.Name}' - {response.StatusCode}");
                    }
                }
            }
            else
            {
                throw new Exception($"Unknown status code when working with catalogue '{catalogue.Name}' - {response.StatusCode}");
            }
        }

        public override void Execute()
        {
            base.Execute();
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", _apiKey);

            var catalogues = _activator.RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>()
                .Where(c => !c.IsDeprecated && !c.IsInternalDataset && !c.IsProjectSpecific(_activator.RepositoryLocator.DataExportRepository))
                .ToList();
            var builder = new ConfluencePageBuilder(catalogues, _owner, _description, _subdomain);
            var uri = $"https://{_subdomain}.atlassian.net/wiki/api/v2/pages";

            CreateContainerPage(builder, uri);
            if (rootParentId != null)
            {
                foreach (var catalogue in catalogues)
                {
                    CreateCataloguePage(catalogue, builder, uri);
                }
                UpdateContainerPage(builder, uri);//re-add the links for the catalogue pages to the home page

            }
            else
            {
                throw new Exception($"Unable to find root id in response");

            }
        }

    }
}
