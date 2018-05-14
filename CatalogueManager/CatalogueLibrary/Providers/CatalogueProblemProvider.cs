using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Nodes.LoadMetadataNodes;

namespace CatalogueLibrary.Providers
{
    /// <summary>
    /// Identifies all problems with all objects found in the Catalogue database.  This only includes problems that are fast to detect at runtime.
    /// </summary>
    public class CatalogueProblemProvider : IProblemProvider
    {
        private ICoreChildProvider _childProvider;
        private HashSet<int> _orphanCatalogueItems = new HashSet<int>();

        public void RefreshProblems(ICoreChildProvider childProvider)
        {
            _childProvider = childProvider;

            //Take all the catalogue items which DONT have an associated ColumnInfo (should hopefully be quite rare)
            var orphans = _childProvider.AllCatalogueItems.Where(ci => ci.ColumnInfo_ID == null);
            
            //now identify those which have an ExtractionInformation (that's a problem! they are extractable but orphaned)
            _orphanCatalogueItems = new HashSet<int>(
                orphans.Where(o => _childProvider.AllExtractionInformations.Any(ei => ei.CatalogueItem_ID == o.ID))

                //store just the ID for performance
                .Select(i=>i.ID));
            
        }

        public bool HasProblem(object o)
        {
            return !string.IsNullOrWhiteSpace(DescribeProblem(o));
        }

        public string DescribeProblem(object o)
        {
            if (o is CatalogueItem)
                return DescribeProblem((CatalogueItem) o);

            if (o is HICProjectDirectoryNode)
                return DescribeProblem((HICProjectDirectoryNode) o);

            if (o is ExtractionInformation)
                return DescribeProblem((ExtractionInformation) o);

            return null;
        }

        public string DescribeProblem(ExtractionInformation extractionInformation)
        {
            //Get the Catalogue that this ExtractionInformation is descended from
            var descendancy = _childProvider.GetDescendancyListIfAnyFor(extractionInformation);
            if (descendancy != null)
            {
                var catalogue = descendancy.Parents.OfType<Catalogue>().SingleOrDefault();
                if (catalogue != null)
                {
                    //if we know the Catalogue extractability
                    
                    //ExtractionCategory.ProjectSpecific should match the Catalogue extractability.IsProjectSpecific
                    //otherwise it's a Problem

                    if (catalogue.IsProjectSpecific(null))
                    {
                        if(extractionInformation.ExtractionCategory != ExtractionCategory.ProjectSpecific)
                            return "Catalogue " + catalogue + " is Project Specific Catalogue so all ExtractionCategory should be " + ExtractionCategory.ProjectSpecific;
                    }
                    else if( extractionInformation.ExtractionCategory == ExtractionCategory.ProjectSpecific)
                        return "ExtractionCategory is only valid when the Catalogue ('"+catalogue+"') is also ProjectSpecific";
                }
            }

            return null;
        }

        public string DescribeProblem(HICProjectDirectoryNode hicProjectDirectoryNode)
        {
            if (hicProjectDirectoryNode.IsEmpty)
                return "No Project Directory has been specified for the load";

            return null;
        }

        public string DescribeProblem(CatalogueItem catalogueItem)
        {
            if (_orphanCatalogueItems.Contains(catalogueItem.ID))
                return "CatalogueItem is extractable but has no associated ColumnInfo";

            return null;
        }
    }
}
