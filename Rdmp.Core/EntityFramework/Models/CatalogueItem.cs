using Rdmp.Core.EntityFramework.Helpers;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rdmp.Core.EntityFramework.Models
{
    [Table("CatalogueItem")]
    public class CatalogueItem: DatabaseObject
    {


        [Required]
        [MaxLength(500)]
        public string Name { get; set => SetField(ref field, value); }

        [Required]
        public int Catalogue_ID { get; set => SetField(ref field, value); }

        public int? ColumnInfo_ID { get; set => SetField(ref field, value); }

        public string Statistical_cons { get; set => SetField(ref field, value); }

        public string Research_relevance { get; set => SetField(ref field, value); }

        public string Description { get; set => SetField(ref field, value); }

        public string Topic { get; set => SetField(ref field, value); }

        public string Agg_method { get; set => SetField(ref field, value); }

        public string Limitations { get; set => SetField(ref field, value); }

        public string Comments { get; set => SetField(ref field, value); }
        [Column(TypeName = "nvarchar(max)")]
        public int? Periodicity { get; set => SetField(ref field, value); }

        // Navigation properties
        [ForeignKey("Catalogue_ID")]
        public virtual Catalogue Catalogue { get; set; }

        [ForeignKey("ColumnInfo_ID")]
        public virtual ColumnInfo ColumnInfo { get; set; }

        public virtual ExtractionInformation ExtractionInformation{ get; set; }
        public override string ToString() => Name;

    }
}
