using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.DataExport.Data
{
    static class ProjectSpecificCatalogueManager
    {

        public static bool CanMakeCatalogueProjectSpecific(IDataExportRepository dqeRepo, ICatalogue catalogue, IProject project, List<int> projectIdsToIgnore)
        {
            var status = catalogue.GetExtractabilityStatus(dqeRepo);

            if (!status.IsExtractable)
                //throw new Exception("Catalogue must first be made Extractable");
                return false;

            var ei = catalogue.GetAllExtractionInformation(ExtractionCategory.Any);
            if (!ei.Any())
                //SetImpossible("Catalogue has no extractable columns");
                return false;
            if (ei.Count(e => e.IsExtractionIdentifier) < 1)
                //SetImpossible("Catalogue must have at least 1 IsExtractionIdentifier column");
                return false;
            var edss = dqeRepo.GetAllObjectsWithParent<ExtractableDataSet>(catalogue);
            if (edss.Any(e => e.Project_ID == project.ID))
            {
                //already project specific
                return true;
            }
            foreach (var eds in edss)
            {
                var alreadyInConfiguration = eds.ExtractionConfigurations.FirstOrDefault(ec => ec.Project_ID != project.ID && !projectIdsToIgnore.Contains(ec.Project_ID));
                //already used in a project that we're not manipulating
                if (alreadyInConfiguration != null) return false;
            }

            return true;
        }

        public static void MakeCatalogueProjectSpecific(IDataExportRepository dqeRepo, ICatalogue catalogue, IProject project)
        {
            var eds = dqeRepo.GetAllObjectsWithParent<ExtractableDataSet>(catalogue).Where(eds => eds.Project_ID is null).SingleOrDefault();
            if (eds is null)
            {
                eds = new ExtractableDataSet(dqeRepo, catalogue, false);
            }
            eds.Project_ID = project.ID;
            foreach (var ei in catalogue.GetAllExtractionInformation(ExtractionCategory.Any).Where(ei => ei.ExtractionCategory is ExtractionCategory.Core))
            {
                ei.ExtractionCategory = ExtractionCategory.ProjectSpecific;
                ei.SaveToDatabase();
            }
            eds.SaveToDatabase();
        }

        public static bool CanMakeCatalogueNonProjectSpecific(IDataExportRepository dqeRepo, ICatalogue catalogue, ExtractableDataSet extractableDataSet)
        {
            var selectedDatasetsForEDS = dqeRepo.GetAllObjects<SelectedDataSets>().Where(sds => sds.ExtractableDataSet_ID == extractableDataSet.ID);
            if (selectedDatasetsForEDS.Any()) //used in an extraction
            {
                //check if catalgue is project specific to any other projects
                var otherEDs = dqeRepo.GetAllObjectsWithParent<ExtractableDataSet>(catalogue).Where(eds => eds.ID != extractableDataSet.ID && eds.Project_ID != null);
                if (otherEDs.Any())
                {
                    //it's used in this projects extractions and used in another project specific extraction;
                    return false;
                }
            }
            return true;
        }

        public static void MakeCatalogueNonProjectSpecific(IDataExportRepository dqeRepo, ICatalogue catalogue, ExtractableDataSet extractableDataSet)
        {
            var existingNullEntry = dqeRepo.GetAllObjects<ExtractableDataSet>().Where(eds => eds.Catalogue_ID == catalogue.ID && eds.Project_ID == null).FirstOrDefault();
            if (existingNullEntry is not null)
            {
                var selectedDatasets = dqeRepo.GetAllObjects<SelectedDataSets>().Where(sds => sds.ExtractableDataSet_ID == extractableDataSet.ID);
                foreach(var sds in selectedDatasets)
                {
                    sds.ExtractableDataSet_ID = existingNullEntry.ID;
                    sds.SaveToDatabase();
                }
                var cumulativeExtractionResults = dqeRepo.GetAllObjects<CumulativeExtractionResults>().Where(cer => cer.ExtractableDataSet_ID == extractableDataSet.ID);
                foreach (var cer in cumulativeExtractionResults)
                {
                    cer.ExtractableDataSet_ID = existingNullEntry.ID;
                    cer.SaveToDatabase();
                }
                extractableDataSet.DeleteInDatabase();
            }
            else
            {
                extractableDataSet.Project_ID = null;
                extractableDataSet.SaveToDatabase();
            }
            foreach (var ei in catalogue.GetAllExtractionInformation(ExtractionCategory.ProjectSpecific))
            {
                ei.ExtractionCategory = ExtractionCategory.Core;
                ei.SaveToDatabase();
            }
        }
    }
}

