using Rdmp.Core.EntityFramework.Helpers;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Rdmp.Core.EntityFramework.Models
{
    [Table("ColumnInfo")]
    public class ColumnInfo: DatabaseObject
    {
        [Key]
        public override  int ID { get; set; }

        [NotMapped]
        public override RDMPDbContext CatalogueDbContext { get; set; }
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


        [ForeignKey("Dataset_ID")]
        public virtual Dataset Dataset{ get; set; }

        public override string ToString() => Name;

    }

}
