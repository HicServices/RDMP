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
    public class Dataset : DatabaseObject, IHasFolder
    {
        [Key]
        public override int ID { get; set; }

        [NotMapped]
        public override RDMPDbContext CatalogueDbContext { get; set; }
        public string Folder { get; set => SetField(ref field, value); }
        public string Name { get; set => SetField(ref field, value); }

        public string DigitalObjectIdentifier { get; set => SetField(ref field, value); }

        public string Source { get; set => SetField(ref field, value); }

        public override string ToString() => Name;

    }
}
