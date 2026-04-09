using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.FilterImporting.Construction;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.EntityFramework.Helpers;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Revertable;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.EntityFramework.Models.DataExport
{
    [Table("SelectedDataSets")]
    public class SelectedDataSet : DatabaseObject, ISelectedDataSets
    {

        [Key]
        public override int ID { get; set; }


        public int ExtractionConfiguration_ID { get; set; }

        [ForeignKey("ExtractionConfiguration_ID")]
        public virtual ExtractionConfiguration ExtractionConfiguration { get; set; }

        public int ExtractableDataSet_ID { get; set; }

        public int RootFilterContainer_ID { get; set; }

        [ForeignKey("ExtractableDataSet_ID")]
        public ExtractableDataSet ExtractableDataSet { get; set; }
        [NotMapped]
        public IExtractionProgress ExtractionProgressIfAny => throw new NotImplementedException();
        [NotMapped]
        public ISelectedDataSetsForcedJoin[] SelectedDataSetsForcedJoins => throw new NotImplementedException();
        [NotMapped]
        public IContainer RootFilterContainer => throw new NotImplementedException();
        [NotMapped]
        IExtractionConfiguration ISelectedDataSets.ExtractionConfiguration => ExtractionConfiguration;
        [NotMapped]
        int? IRootFilterContainerHost.RootFilterContainer_ID { get => RootFilterContainer_ID; set => throw new NotImplementedException(); }

        [NotMapped]
        IExtractableDataSet ISelectedDataSets.ExtractableDataSet => ExtractableDataSet;

        public override string ToString()
        {
            return DataExportDbContext.ExtractableDataSets.FirstOrDefault(eds => eds.ID == ExtractableDataSet_ID)?.ToString();
        }


        public void CreateRootContainerIfNotExists()
        {
            throw new NotImplementedException();
        }

        public bool Exists()
        {
            throw new NotImplementedException();
        }

        public ICatalogue GetCatalogue()
        {
            throw new NotImplementedException();
        }

        public ICumulativeExtractionResults GetCumulativeExtractionResultsIfAny()
        {
            throw new NotImplementedException();
        }

        public IFilterFactory GetFilterFactory()
        {
            throw new NotImplementedException();
        }

        public RevertableObjectReport HasLocalChanges()
        {
            throw new NotImplementedException();
        }

        public void RevertToDatabaseState()
        {
            throw new NotImplementedException();
        }

        public void SaveToDatabase()
        {
            throw new NotImplementedException();
        }

        public bool ShouldBeReadOnly(string context, out string reason)
        {
            throw new NotImplementedException();
        }

        //public override string ToString() => ExtractionConfiguration

    }
}
