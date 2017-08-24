using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.QueryBuilding;
using CatalogueLibrary.Repositories;
using CatalogueWebService.Cache;
using CatalogueWebService.Modules.Data;
using DataQualityEngine;
using Nancy;
using Nancy.Responses;
using QueryCaching.Aggregation;
using RDMPStartup;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;

namespace CatalogueWebService.Modules
{
    public class CatalogueModule : NancyModule
    {
        private readonly ServerDefaults _defaults;
        private readonly ICacheProvider<ChartData> _cache;
        private CatalogueRepository _catalogueRepository;

        public CatalogueModule(IRDMPPlatformRepositoryServiceLocator finder, ICacheProvider<ChartData> cache)
        {
            _catalogueRepository = finder.CatalogueRepository;

            _cache = cache;
            _defaults = new ServerDefaults(_catalogueRepository);

            Get["/catalogues"] = parameters =>
            {
                var catalogues = _catalogueRepository.GetAllCatalogues();
                var catalogueData = catalogues.Select(catalogue => new CatalogueData
                {
                    ID = catalogue.ID, Name = catalogue.Name
                }).ToList();

                return Response.AsJson(catalogueData);
            };

            Get["/catalogues/{id}"] = parameters =>
            {
                int catalogueId = parameters.id;
                try
                {
                    Catalogue catalogue = _catalogueRepository.GetObjectByID<Catalogue>(catalogueId);
                    return Response.AsJson(new CatalogueData(catalogue, new DatasetTimespanCalculator()));
                }
                catch (Exception)
                {
                    return CatalogueNotFound(catalogueId);
                }
            };

            Get["/catalogues/{id}/supportingdocuments/"] = parameters =>
            {
                int catalogueId = parameters.id;
                try
                {
                    var docs =
                        _catalogueRepository.GetObjectByID<Catalogue>(catalogueId)
                            .GetAllSupportingDocuments(FetchOptions.ExtractableLocals);
                    return Response.AsJson(docs.Select(d => new SupportingDocumentData(d)).ToList());
                }
                catch (Exception)
                {
                    return CatalogueNotFound(catalogueId);
                }
            };


            Get["/catalogues/{id}/supportingdocuments/{supportingDocumentId}"] = parameters =>
            {
                try
                {
                    int catalogueId = parameters.id;
                    int supportingDocumentId = parameters.supportingDocumentId;


                    var doc = _catalogueRepository.GetObjectByID<SupportingDocument>(supportingDocumentId);

                    if(doc.Catalogue_ID != catalogueId)
                        throw new NotSupportedException("User asked for supporting document which belonged to a different Catalogue");

                    if (!doc.IsReleasable())
                        throw new NotSupportedException("User asked for an illegal file!");

                    FileInfo toCopy = new FileInfo(doc.URL.LocalPath);
                    var fs = toCopy.OpenRead();
                    var streamResponse = new StreamResponse(() => fs, MimeTypes.GetMimeType(toCopy.Name));

                    return streamResponse.AsAttachment(toCopy.Name);
                }
                catch (Exception ex)
                {
                    return Response.AsJson(new ErrorData(){Message = ex.Message });
                }
            };

            Get["/catalogues/{id}/aggregates"] = parameters =>
            {
                int catalogueId = parameters.id;
                try
                {
                    var aggregateData = GetAvailableAggregates(catalogueId).Select(configuration => new AggregateData(configuration)).ToList();          
                    return Response.AsJson(aggregateData);
                }
                catch (Exception)
                {
                    return CatalogueNotFound(catalogueId);
                }
            };
            
            Get["/catalogues/{id}/aggregates/{aggregateId}"] = parameters =>
            {
                try
                {
                    var aggregateConfiguration = FindAggregateInCatalogue(parameters.aggregateId, parameters.id);
                    return Response.AsJson(new AggregateData(aggregateConfiguration));
                }
                catch (Exception e)
                {
                    return NotFoundResponse(e.Message);
                }
            };

            Get["/catalogues/{id}/aggregates/{aggregateId}/data"] = parameters =>
            {
                // This doesn't work, looks like CatalogueModule object is created on each request? Find another way to integrate cache.
                if (_cache.Contains(Request.Path))
                    return Response.AsJson(_cache.Get(Request.Path));

                try
                {
                    AggregateConfiguration aggregateConfiguration = FindAggregateInCatalogue(parameters.aggregateId, parameters.id);
                    var chartData = CompileAggregateData(aggregateConfiguration);
                    
                    _cache.Set(Request.Path, chartData);
                    
                    return Response.AsJson(chartData);
                }
                catch (Exception e)
                {
                    return NotFoundResponse(e.Message);
                }
            };
        }

        private IEnumerable<AggregateConfiguration> GetAvailableAggregates(int catalogueId)
        {
            var server = _defaults.GetDefaultFor(ServerDefaults.PermissableDefaults.WebServiceQueryCachingServer_ID);
            var manager = new CachedAggregateConfigurationResultsManager(server);

            return
                _catalogueRepository.GetObjectByID<Catalogue>(catalogueId)
                .AggregateConfigurations
                    .Where(
                        //It is extractable still!
                        c => c.IsExtractable
                             &&
                             //And it has cached answers available
                             manager.GetLatestResultsTableUnsafe(c, AggregateOperation.ExtractableAggregateResults) != null);
        }
        private ChartData CompileAggregateData(AggregateConfiguration aggregateConfiguration)
        {
            if(!aggregateConfiguration.IsExtractable)
                throw new NotSupportedException("Aggregate is not IsExtractable");

            var server = _defaults.GetDefaultFor(ServerDefaults.PermissableDefaults.WebServiceQueryCachingServer_ID);
            var manager = new CachedAggregateConfigurationResultsManager(server);

            IHasFullyQualifiedNameToo tableData = manager.GetLatestResultsTableUnsafe(aggregateConfiguration, AggregateOperation.ExtractableAggregateResults);
            

            using(var con = DataAccessPortal.GetInstance().ExpectServer(server,DataAccessContext.DataExport).GetConnection())
            {
                con.Open();

                var cmd = DatabaseCommandHelper.GetCommand("Select * from " + tableData, con);
                var dt = new DataTable();

                DatabaseCommandHelper.GetDataAdapter(cmd).Fill(dt);

                if (dt.Columns.Count < 2)
                    throw new NotSupportedException("Aggregates must have 2 columns at least");

                var numSeries = dt.Columns.Count - 1;
                var chartData = new ChartData(numSeries);
                foreach (DataRow row in dt.Rows)
                {
                    chartData.AddX(row[0].ToString());

                    for (var seriesNum = 0; seriesNum < numSeries; ++seriesNum)
                        chartData.AddY(seriesNum, row[seriesNum + 1].ToString());
                }

                var columnNum = 0;
                foreach (DataColumn column in dt.Columns)
                {
                    if (columnNum > 0)
                        chartData.Series[columnNum - 1].Label = column.ColumnName;

                    ++columnNum;
                }

                return chartData;
            }
        }

        private Response NotFoundResponse(string message)
        {
            return Response.AsJson(new ErrorData
            {
                Message = message
            }, HttpStatusCode.NotFound);
        }

        private dynamic CatalogueNotFound(int catalogueId)
        {
            return Response.AsJson(new ErrorData
            {
                Message = "Could not find Catalogue " + catalogueId
            }, HttpStatusCode.NotFound);
        }

        public AggregateConfiguration FindAggregateInCatalogue(int aggregateId, int catalogueId)
        {
            Catalogue catalogue = _catalogueRepository.GetObjectByID<Catalogue>(catalogueId);
            if (catalogue == null)
                throw new Exception("Catalogue " + catalogueId + " not found");

            var aggregateConfigurations = catalogue.AggregateConfigurations.Where(a=>a.IsExtractable).ToList();

            if (!aggregateConfigurations.Any())
                throw new Exception("Catalogue " + catalogueId + " has no aggregate configurations");

            var aggregateConfiguration = aggregateConfigurations.FirstOrDefault(configuration => configuration.ID == aggregateId && configuration.IsExtractable);
            if (aggregateConfiguration == null)
                throw new Exception("Aggregate Configuration " + aggregateId + " not found for Catalogue " + catalogueId);

            return aggregateConfiguration;

        }
    }
}