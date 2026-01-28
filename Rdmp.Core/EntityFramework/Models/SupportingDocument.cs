using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rdmp.Core.Models
{
    [Table("SupportingDocument")]
    public class SupportingDocument
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [MaxLength(500)]
        public string Name { get; set; }

        public int? Catalogue_ID { get; set; }
        public string URL { get; set; }
        public bool Extractable { get; set; }
        public bool IsGlobal { get; set; }

        [ForeignKey("Catalogue_ID")]
        public virtual Catalogue Catalogue { get; set; }
    }
}
