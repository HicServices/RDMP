using Rdmp.Core.EntityFramework.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.EntityFramework.Models
{
    public class CohortIdentificationConfiguration: DatabaseObject
    {

        [Key]
        public override int ID { get; set; }
    }
}
