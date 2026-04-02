using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.EntityFramework.Helpers;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Revertable;
using Rdmp.Core.Providers;
using Rdmp.Core.ReusableLibraryCode;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.EntityFramework.Models.DataExport
{
    [Table("Project")]
    public class Project : DatabaseObject, IProject
    {
        [Key]
        public override int ID { get; set; }
        public string MasterTicket { get; set; }
        public string ExtractionDirectory { get; set; }
        public int? ProjectNumber { get; set; }

        public IExtractionConfiguration[] ExtractionConfigurations => throw new NotImplementedException();

        public IProjectCohortIdentificationConfigurationAssociation[] ProjectCohortIdentificationConfigurationAssociations => throw new NotImplementedException();

        public string Name { get; set; }
        public string Folder { get; set; }

        public override string ToString() => Name;

        public bool Exists()
        {
            throw new NotImplementedException();
        }

        public Curation.Data.ExtractionInformation[] GetAllProjectCatalogueColumns(ExtractionCategory any)
        {
            throw new NotImplementedException();
        }

        public Curation.Data.ExtractionInformation[] GetAllProjectCatalogueColumns(ICoreChildProvider childProvider, ExtractionCategory any)
        {
            throw new NotImplementedException();
        }

        public ICatalogue[] GetAllProjectCatalogues()
        {
            throw new NotImplementedException();
        }

        public IHasDependencies[] GetObjectsDependingOnThis()
        {
            throw new NotImplementedException();
        }

        public IHasDependencies[] GetObjectsThisDependsOn()
        {
            throw new NotImplementedException();
        }

        public RevertableObjectReport HasLocalChanges()
        {
            throw new NotImplementedException();
        }

        public void RevertToDatabaseState()
        {
            throw new NotImplementedException();
        }

        public void SaveToDatabase()
        {
            throw new NotImplementedException();
        }
    }
}
