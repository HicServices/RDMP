using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Rdmp.Core.Models
{
    [Table("ColumnInfo")]
    public class ColumnInfo
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [MaxLength(500)]
        public string Name { get; set; }

        [Required]
        public int TableInfo_ID { get; set; }

        [Column("Data_type")]
        public string DataType { get; set; }
        public bool IsPrimaryKey { get; set; }
        public string Collation { get; set; }

        [ForeignKey("TableInfo_ID")]
        public virtual TableInfo TableInfo { get; set; }

        public virtual ICollection<CatalogueItem> CatalogueItems { get; set; }

        public override string ToString() => Name;

    }

}
