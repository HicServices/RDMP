using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.EntityFramework.Helpers;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Revertable;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Rdmp.Core.EntityFramework.Models.DataExport
{
    [Table("ExtractableDataSet")]
    public class ExtractableDataSet: DatabaseObject, IExtractableDataSet
    {

        [Key]
        public override int ID { get; set; }

        public int Catalogue_ID { get; set; }
        public bool DisableExtraction { get; set; }
        public int? Project_ID { get; set; }

        [ForeignKey("Catalogue_ID")]
        public virtual Catalogue Catalogue { get; set; }


        [ForeignKey("Project_ID")]
        public virtual Project Project{ get; set; }

        [NotMapped]
        public List<IProject> Projects { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        [NotMapped]
        public bool IsCatalogueDeprecated => false;//throw new NotImplementedException();

        [NotMapped]
        ICatalogue IExtractableDataSet.Catalogue => Catalogue;

        public bool Exists()
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

        public override string ToString()
        {
            if (Catalogue == null)
                return $"DELETED CATALOGUE {Catalogue_ID}";

            //only bother refreshing Catalogue details if we will be able to get a legit catalogue name
            return Catalogue.IsDeprecated ? $"DEPRECATED CATALOGUE {Catalogue.Name}" : Catalogue.Name;
        }
    }
}
