using Microsoft.Identity.Client;
using Rdmp.Core.Curation.Data.Dashboarding;
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
    [Table("DatabaseControl")]
    public class DashboardControl: DatabaseObject
    {
        [Key]
        public override int ID { get; set; }
        public int DashboardLayout_ID { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string PersistenceString { get; set; }
        public string ControlType { get; set; }

        [ForeignKey("DashboardLayout_ID")]
        public virtual DashboardLayout ParentLayout { get; set; }

        public DashboardObjectUse[] ObjectsUsed => Array.Empty<DashboardObjectUse>();//TODO
    }
}
