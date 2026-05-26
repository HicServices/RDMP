using Rdmp.Core.EntityFramework.Helpers;
using Rdmp.Core.ReusableLibraryCode.Checks;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.EntityFramework.Models
{
    [Table("StandardRegex")]
    public class StandardRegex: DatabaseObject, ICheckable
    {
        public StandardRegex(RDMPDbContext catalogueDbContext)
        {
            CatalogueDbContext = catalogueDbContext;
        }

        public static string DataLoadEngineGlobalIgnorePattern { get; internal set; }
        public override int ID { get; set; }
        public string ConceptName { get; set; }
        public string Regex { get; set; }
        public string Description { get; set; }

        public void Check(ICheckNotifier notifier)
        {
            throw new NotImplementedException();
        }

        public override string ToString() => ConceptName;

    }
}
