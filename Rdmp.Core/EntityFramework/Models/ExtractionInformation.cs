using FAnsi;
using FAnsi.Discovery.QuerySyntax;
using FAnsi.Implementations.MicrosoftSQL;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.EntityFramework.Helpers;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.QueryBuilding.SyntaxChecking;
using Rdmp.Core.ReusableLibraryCode.Checks;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rdmp.Core.EntityFramework.Models
{
    [Table("ExtractionInformation")]
    public class ExtractionInformation: DatabaseObject, IColumn
    {
        [Key]
        public override int ID { get; set; }

        [NotMapped]
        public override RDMPDbContext CatalogueDbContext { get; set; }

        [Required]
        public int CatalogueItem_ID { get; set; }

        [Required]
        public string SelectSQL { get; set => SetField(ref field, value); }

        public string Alias { get; set => SetField(ref field, value); }
        public int Order { get; set => SetField(ref field, value); }
        public string ExtractionCategory { get; set => SetField(ref field, value); }
        public bool IsPrimaryKey { get; set => SetField(ref field, value); }
        public bool HashOnDataRelease { get; set => SetField(ref field, value); }

        public bool IsExtractionIdentifier { get; set => SetField(ref field, value); }

        [NotMapped]
        public ColumnInfo ColumnInfo => CatalogueItem?.ColumnInfo;

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

        public void Check(ICheckNotifier notifier)
        {
            new ColumnSyntaxChecker(this).Check(notifier);
        }

        public string GetRuntimeName()
        {
            var helper = CatalogueItem.ColumnInfo == null ? MicrosoftQuerySyntaxHelper.Instance : CatalogueItem.ColumnInfo.GetQuerySyntaxHelper();
            if (!string.IsNullOrWhiteSpace(Alias))
                return helper.GetRuntimeName(Alias); //.GetRuntimeName(); RDMPQuerySyntaxHelper.GetRuntimeName(this);

            return !string.IsNullOrWhiteSpace(SelectSQL) ? helper.GetRuntimeName(SelectSQL) : CatalogueItem.ColumnInfo.GetRuntimeName();
        }

        public IQuerySyntaxHelper GetQuerySyntaxHelper() => CatalogueItem.ColumnInfo?.GetQuerySyntaxHelper();

        public ExtractionCategory GetExtractionCategory() => (ExtractionCategory)Enum.Parse(typeof(ExtractionCategory), ExtractionCategory);

    }
}
