using Rdmp.Core.Curation.Data;
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
    [Table("CohortIdentificationConfiguration")]

    public class CohortIdentificationConfiguration: DatabaseObject, IHasFolder
    {

        [Key]
        public override int ID { get; set; }

        public int? Version { get; set; }

        public string Name { get; set => SetField(ref field, value); }
        public string Description { get; set => SetField(ref field, value); }
        public string Folder { get; set => SetField(ref field, value); }
        public bool Frozen { get; set => SetField(ref field, value); }
        public string FrozenBy { get; set => SetField(ref field, value); }
        public DateTime? FrozenDate { get; set => SetField(ref field, value); }
        public bool IsTemplate { get; set => SetField(ref field, value); }

        public override string ToString()
        {
            return Name;
        }

        public List<CohortIdentificationConfiguration> GetVersions()
        {
            return new List<CohortIdentificationConfiguration>();//TODO
        }
    }
}
