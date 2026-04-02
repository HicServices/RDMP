using Rdmp.Core.EntityFramework.Helpers;
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
    public class SelectedDataSet: DatabaseObject
    {

        [Key]
        public override int ID { get; set; }


        public int ExtractionConfiguration_ID { get; set; }
    }
}
