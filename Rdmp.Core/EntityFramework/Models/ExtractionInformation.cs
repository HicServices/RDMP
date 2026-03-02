using FAnsi.Discovery.QuerySyntax;
using FAnsi.Implementations.MicrosoftSQL;
using Rdmp.Core.EntityFramework.Helpers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rdmp.Core.EntityFramework.Models
{
    [Table("ExtractionInformation")]
    public class ExtractionInformation: DatabaseObject
    {
        [Key]
        public override int ID { get; set; }

        [NotMapped]
        public override RDMPDbContext CatalogueDbContext { get; set; }

        [Required]
        public int CatalogueItem_ID { get; set; }

        [Required]
        public string SelectSQL { get; set; }

        public string Alias { get; set; }
        public int Order { get; set; }
        public string ExtractionCategory { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool HashOnDataRelease { get; set; }

        public bool IsExtractionIdentifier { get; set; }

        [ForeignKey("CatalogueItem_ID")]
        public virtual CatalogueItem CatalogueItem { get; set; }

        public override string ToString() => SelectSQL;
        public bool IsProperTransform()
        {
            if (string.IsNullOrWhiteSpace(SelectSQL))
                return false;

            if (CatalogueItem.ColumnInfo == null)
                return false;

            if (HashOnDataRelease)
                return true;

            //if the select sql is different from the column underlying it then it is a proper transform (not just a copy paste)
            return !SelectSQL.Equals(CatalogueItem.ColumnInfo.Name);
        }

        public string GetRuntimeName()
        {
            var helper = CatalogueItem.ColumnInfo == null ? MicrosoftQuerySyntaxHelper.Instance : CatalogueItem.ColumnInfo.GetQuerySyntaxHelper();
            if (!string.IsNullOrWhiteSpace(Alias))
                return helper.GetRuntimeName(Alias); //.GetRuntimeName(); RDMPQuerySyntaxHelper.GetRuntimeName(this);

            return !string.IsNullOrWhiteSpace(SelectSQL) ? helper.GetRuntimeName(SelectSQL) : CatalogueItem.ColumnInfo.GetRuntimeName();
        }

        public IQuerySyntaxHelper GetQuerySyntaxHelper() => CatalogueItem.ColumnInfo?.GetQuerySyntaxHelper();

    }
}
