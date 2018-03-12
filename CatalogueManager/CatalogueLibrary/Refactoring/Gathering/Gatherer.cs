using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using ReusableLibraryCode;

namespace CatalogueLibrary.Refactoring.Gathering
{
    /// <summary>
    /// Gathers dependencies of a given object in a more advanced/selective way than simply using methods of IHasDependencies
    /// </summary>
    public class Gatherer
    {
        private readonly IRDMPPlatformRepositoryServiceLocator _repositoryLocator;

        public Gatherer(IRDMPPlatformRepositoryServiceLocator repositoryLocator)
        {
            _repositoryLocator = repositoryLocator;
        }

        public GatheredObject[] GatherDependencies(ColumnInfo c)
        {
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
                }
            }

            //ExtractableDataset columns
            
            //deployed extraction filters

            foreach (Catalogue catalogue in catalogues)
            {
                //Aggregates

                //Aggregate dimensions

                //Aggregate filters

                //
            }

            return foundSoFar.ToArray();
        }

    }
}
