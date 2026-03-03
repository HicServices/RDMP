using Rdmp.Core.Curation.Data;
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
    public class LookupCompositeJoinInfo : DatabaseObject, ISupplementalJoin
    {
        [Key]
        public override int ID { get; set; }
        public int OriginalColumnInfo_ID { get; set; }
        public int ForeignKey_ID { get; set; }
        public int PrimaryKey_ID { get; set; }
        public string Collation { get; set; }


        [ForeignKey("OriginalColumnInfo_ID")]
        public virtual ColumnInfo OriginalColumnInfo { get; set; }
        [ForeignKey("ForeignKey_ID")]

        public virtual ColumnInfo ForeignKey { get; set; }

        [ForeignKey("PrimaryKey_ID")]

        public virtual ColumnInfo PrimaryKey { get; set; }
    }
}
