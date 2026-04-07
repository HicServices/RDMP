using Rdmp.Core.EntityFramework.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Terminal.Gui.Trees;

namespace Rdmp.Core.EntityFramework.Models.DataExport
{
    [Table("ProjectCohortIdentificationConfigurationAssociation")]
    public class ProjectCohortIdentificationConfigurationAssociation: DatabaseObject
    {
        [Key]
        public override int ID { get; set; }

        public int Project_ID { get; set; }
        public int CohortIdentificationConfiguration_ID { get; set; }
    }
}
