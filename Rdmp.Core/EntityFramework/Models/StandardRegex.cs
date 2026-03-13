using Rdmp.Core.EntityFramework.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.EntityFramework.Models
{
    [Table("StandardRegex")]
    public class StandardRegex: DatabaseObject
    {
        public override int ID { get; set; }
        public string ConceptName { get; set; }
        public string Regex { get; set; }
        public string Description { get; set; }
        public override string ToString() => ConceptName;

    }
}
