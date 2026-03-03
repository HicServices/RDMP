using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using FAnsi.Naming;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.EntityFramework.Helpers;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Rdmp.Core.EntityFramework.Models
{
    [Table("ColumnInfo")]
    public class ColumnInfo: DatabaseObject, IResolveDuplication, IHasRuntimeName
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
        public int? DuplicateRecordResolutionOrder { get; set; }
        public bool DuplicateRecordResolutionIsAscending { get; set; }
        public string Format { get; set; }

        public int? ANOTable_ID { get; set; }

        [ForeignKey("TableInfo_ID")]
        public virtual TableInfo TableInfo { get; set; }

        [ForeignKey("ANOTable_ID")]
        public virtual ANOTable ANOTable { get; set; }

        public virtual ICollection<CatalogueItem> CatalogueItems { get; set; }


        [ForeignKey("Dataset_ID")]
        public virtual Dataset Dataset{ get; set; }

        public bool IgnoreInLoads { get; set; }

        public override string ToString() => Name;
        public string GetFullyQualifiedName() => Name;

        public string GetRuntimeName() => Name == null ? null : GetQuerySyntaxHelper().GetRuntimeName(Name);
        public string GetRuntimeName(LoadStage stage)
        {
            var finalName = GetRuntimeName();

            //if (stage <= LoadStage.AdjustRaw)
            //    //see if it has an ANO Transform on it
            //TODO think aboutano table
            //    if (ANOTable_ID != null && finalName.StartsWith("ANO"))
            //        return finalName["ANO".Length..];

            //any other stage will be the regular final name
            return finalName;
        }
        public IQuerySyntaxHelper GetQuerySyntaxHelper()
        {
            if(TableInfo is null) TableInfo = CatalogueDbContext.TableInfos.Find(TableInfo_ID);
            return TableInfo.GetQuerySyntaxHelper();
        }

        public string GetRuntimeDataType(LoadStage loadStage)
        {
            if (loadStage <= LoadStage.AdjustRaw)
            {
                //if it has an ANO transform
                if (ANOTable_ID != null)
                    return
                        ANOTable.GetRuntimeDataType(
                            loadStage); //get the datatype from the ANOTable because ColumnInfo is of mutable type depending on whether it has been anonymised yet

                //it doesn't have an ANOtransform but it might be the subject of dilution
                var discard = TableInfo.PreLoadDiscardedColumns.SingleOrDefault(c =>
                    c.GetRuntimeName().Equals(GetRuntimeName(), StringComparison.InvariantCultureIgnoreCase));

                //The column exists both in the live database and in the identifier dump.  This is because it goes through horrendous bitcrushing operations e.g. Load RAW with full
                //postcode varchar(8) and ship postcode off to identifier dump but also let it go through to live but only as the first 4 letters varchar(4).  so the datatype of the column
                //in RAW is varchar(8) but in Live is varchar(4)
                return discard != null ? discard.Data_type : DataType;
            }

            //The user is asking about a stage other than RAW so tell them about the final column type state
            return DataType;
        }

        public DiscoveredColumn Discover(DataAccessContext context)
        {
            var ti = TableInfo;
            var db = DataAccessPortal.ExpectDatabase(ti, context);
            return db.ExpectTable(ti.GetRuntimeName()).DiscoverColumn(GetRuntimeName());
        }

    }

}
