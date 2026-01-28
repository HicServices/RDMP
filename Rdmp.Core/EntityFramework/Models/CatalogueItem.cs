using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rdmp.Core.Models
{
    [Table("CatalogueItem")]
    public class CatalogueItem
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [MaxLength(500)]
        public string Name { get; set; }

        [Required]
        public int Catalogue_ID { get; set; }

        public int? ColumnInfo_ID { get; set; }

        public string Statistical_cons { get; set; }

        public string Research_relevance { get; set; }

        public string Description { get; set; }

        public string Topic { get; set; }

        public string Agg_method { get; set; }

        public string Limitations { get; set; }

        public string Comments { get; set; }
        [Column(TypeName = "nvarchar(max)")]
        public int? Periodicity { get; set; }

        // Navigation properties
        [ForeignKey("Catalogue_ID")]
        public virtual Catalogue Catalogue { get; set; }

        [ForeignKey("ColumnInfo_ID")]
        public virtual ColumnInfo ColumnInfo { get; set; }
    }
}
