using Rdmp.Core.EntityFramework.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rdmp.Core.EntityFramework.Models
{
    [Table("TableInfo")]
    public class TableInfo
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [MaxLength(500)]
        public string Name { get; set; }

        public string Database { get; set; }
        public string Server { get; set; }
        public string DatabaseType { get; set; }
        //public bool IsLookup { get; set; }

        public virtual ICollection<ColumnInfo> ColumnInfos { get; set; }
    }
}
