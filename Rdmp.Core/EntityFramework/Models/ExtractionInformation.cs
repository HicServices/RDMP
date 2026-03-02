using Rdmp.Core.EntityFramework.Helpers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rdmp.Core.EntityFramework.Models
{
    [Table("ExtractionInformation")]
    public class ExtractionInformation: DatabaseObject
    {

        [Required]
        public int CatalogueItem_ID { get; set; }

        [Required]
        public string SelectSQL { get; set; }

        public string Alias { get; set; }
        public int Order { get; set; }
        public string ExtractionCategory { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool HashOnDataRelease { get; set; }

        [ForeignKey("CatalogueItem_ID")]
        public virtual CatalogueItem CatalogueItem { get; set; }

        public override string ToString() => SelectSQL;
    }
}
