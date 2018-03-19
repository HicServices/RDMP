using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Data.DataTables;

namespace Sharing.Dependency.Gathering
{
    /// <summary>
    /// Gathers dependencies of a given object in a more advanced/selective way than simply using methods of IHasDependencies
    /// </summary>
    public class Gatherer
    {
        private readonly IRDMPPlatformRepositoryServiceLocator _repositoryLocator;
        
        private Dictionary<int, ExtractionConfiguration> _allExtractionConfigurations;
        private ExtractableColumn[] _allExtractionColumns;

        public Gatherer(IRDMPPlatformRepositoryServiceLocator repositoryLocator)
        {
            _repositoryLocator = repositoryLocator;
            
            _allExtractionConfigurations = _repositoryLocator.DataExportRepository.GetAllObjects<ExtractionConfiguration>().ToDictionary(k => k.ID, v => v);
            _allExtractionColumns = _repositoryLocator.DataExportRepository.GetAllObjects<ExtractableColumn>();
        }

        public GatheredObject GatherDependencies(ColumnInfo c)
        {
            var root = new GatheredObject(c);

            HashSet<GatheredObject> foundSoFar = new HashSet<GatheredObject>();

            HashSet<Catalogue> catalogues = new HashSet<Catalogue>();

            //the table it belongs to
            foundSoFar.Add(new GatheredObject(c.TableInfo));

            //the CatalogueItems (descriptive elements) that relate to it (including transforms)
            foreach (CatalogueItem ci in _repositoryLocator.CatalogueRepository.GetAllObjects<CatalogueItem>("WHERE ColumnInfo_ID = " + c.ID))
            {
                foundSoFar.Add(new GatheredObject(ci));

                //record the unique Catalogues for later evaluation
                if (catalogues.All(cata => cata.ID != ci.Catalogue_ID))
                    catalogues.Add(ci.Catalogue);

                //The extraction SQL / Category / Transformation 
                var ei = ci.ExtractionInformation;
                if (ei != null)
                {
                    foundSoFar.Add(new GatheredObject(ei));

                    //The filters belonging to extraction informations
                    foreach (ExtractionFilter extractionFilter in ei.ExtractionFilters)
                        foundSoFar.Add(new GatheredObject(extractionFilter));

                    //The columns as they have been extracted
                    var extractionInformationDeployments = _allExtractionColumns.Where(
                        col =>
                            col.CatalogueExtractionInformation_ID.HasValue &&
                            col.CatalogueExtractionInformation_ID.Value == ei.ID).ToArray();

                    //ExtractableDataset columns
                    foreach (ExtractableColumn ec in extractionInformationDeployments)
                    {
                        var config = _allExtractionConfigurations[ec.ExtractionConfiguration_ID];
                        foundSoFar.Add(new GatheredObject(ec) {IsReleased = config.IsReleased});
                    }

                    _repositoryLocator.DataExportRepository.GetAllObjects<DeployedExtractionFilter>();
                }
            }

            
            //deployed extraction filters

            foreach (Catalogue catalogue in catalogues)
            {
                //Aggregates

                //Aggregate dimensions

                //Aggregate filters

                //
            }

            root.Dependencies.AddRange(foundSoFar);
            return root;
        }

    }
}
