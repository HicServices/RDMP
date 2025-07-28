using NPOI.OpenXmlFormats.Spreadsheet;
using Org.BouncyCastle.Crypto.Signers;
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
                return false;

            var ei = catalogue.GetAllExtractionInformation(ExtractionCategory.Any);
            if (!ei.Any())
                return false;
            if (ei.Count(e => e.IsExtractionIdentifier) < 1)
                return false;
            var edss = dqeRepo.GetAllObjectsWithParent<ExtractableDataSet>(catalogue);
            if (edss.Any(e => e.Projects.Select(p => p.ID).Contains(project.ID)))
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

        public static ExtractableDataSet MakeCatalogueProjectSpecific(IDataExportRepository dqeRepo, ICatalogue catalogue, IProject project)
        {
            var eds = dqeRepo.GetAllObjectsWithParent<ExtractableDataSet>(catalogue).SingleOrDefault();
            if (eds is null)
            {
                eds = new ExtractableDataSet(dqeRepo, catalogue, false);
                eds.SaveToDatabase();
            }

            var edsp = new ExtractableDataSetProject(dqeRepo, eds, project);
            edsp.SaveToDatabase();
            foreach (var ei in catalogue.GetAllExtractionInformation(ExtractionCategory.Any).Where(ei => ei.ExtractionCategory is ExtractionCategory.Core))
            {
                ei.ExtractionCategory = ExtractionCategory.ProjectSpecific;
                ei.SaveToDatabase();
            }
            return eds;
        }

        public static bool CanMakeCatalogueNonProjectSpecific(IDataExportRepository dqeRepo, ICatalogue catalogue, ExtractableDataSet extractableDataSet, IProject project)
        {
            bool usedInExtraction = project.ExtractionConfigurations.Where(ec => ec.SelectedDataSets.Select(sds => sds.ExtractableDataSet).Contains(extractableDataSet)).Any();
            if (!usedInExtraction) return true;
            if (usedInExtraction && extractableDataSet.Projects.Count == 1) return true;
            return false;
        }

        public static void MakeCatalogueNonProjectSpecific(IDataExportRepository dqeRepo, ICatalogue catalogue, ExtractableDataSet extractableDataSet, Project project)
        {
            if (project is null) return;
            if (extractableDataSet is null) return;
            if (catalogue is null) return;

            var p = dqeRepo.GetAllObjects<ExtractableDataSetProject>().Where(edsp => edsp.ExtractableDataSet_ID == extractableDataSet.ID && edsp.Project_ID == project.ID).FirstOrDefault();
            if (p is not null)
            {
                p.DeleteInDatabase();
            }
            extractableDataSet.Projects.Remove(project);

            foreach (var ei in catalogue.GetAllExtractionInformation(ExtractionCategory.ProjectSpecific))
            {
                ei.ExtractionCategory = ExtractionCategory.Core;
                ei.SaveToDatabase();
            }
        }
    }
}
