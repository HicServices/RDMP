using Rdmp.Core.DataExport.Data;
using Rdmp.Core.EntityFramework.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Rdmp.Core.EntityFramework.Models.DataExport
{
    public class ExtractableCohort: DatabaseObject
    {
        [Key]
        public override int ID { get; set; }

        public int ExternalCohortTable_ID { get; set; }

        [ForeignKey("ExternalCohortTable_ID")]
        public virtual ExternalCohortTable ExternalCohortTable { get; set; }

        public IExternalCohortDefinitionData GetExternalData(int timeoutInSeconds = -1)
        {
            return ExternalCohortDefinitionData.Orphan;
        }

        public string GetPrivateIdentifier(bool runtimeName = false) => "TODO";

        public string WhereSQL() => "TODO";
    }
}
