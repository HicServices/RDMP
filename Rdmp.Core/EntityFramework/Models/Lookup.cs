using Rdmp.Core.Curation.Data;
using Rdmp.Core.EntityFramework.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.EntityFramework.Models
{
    public class Lookup : DatabaseObject, IJoin
    {

        public int PrimaryKey_ID { get; set; }
        public int ForeignKey_ID { get; set; }
        public int Description_ID { get; set; }

        public string Collation { get; set; }
        public int ExtractionJoinType { get; set; } //left,right,inner

        public virtual ColumnInfo PrimaryKey { get; set; }
        public virtual ColumnInfo ForeignKey { get; set; }
        public virtual ColumnInfo DescriptionKey { get; set; }

        [Key]
        public override int ID { get; set; }
        public override RDMPDbContext CatalogueDbContext { get; set; }

        public override string ToString() => $"{ForeignKey?.Name} = {PrimaryKey?.Name}";

        public IEnumerable<ISupplementalJoin> GetSupplementalJoins() => CatalogueDbContext.LookupCompositeJoinInfos.Where(ji => ji.OriginalColumnInfo_ID == this.ID).ToList();

        public ExtractionJoinType GetInvertedJoinType()
        {
            throw new NotImplementedException();
        }

        public string GetCustomJoinSql()
        {
            throw new NotImplementedException();
        }
    }
}
