using Rdmp.Core.EntityFramework.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.EntityFramework.Models
{
    [Table("LoadMetadataCatalogueLinkage")]
    public class LoadMetadataCatalogueLinkage: DatabaseObject
    {
        [Key]
        public override int ID { get; set; }
    
        public int LoadMetadataID { get; set; }
        public int CatalogueID { get; set; }

        [ForeignKey("CatalogueID")]
        public virtual Catalogue Catalogue{ get; set; }

        [ForeignKey("LoadMetadataID")]
        public virtual LoadMetadata LoadMetadata{ get; set; }
    }
}
