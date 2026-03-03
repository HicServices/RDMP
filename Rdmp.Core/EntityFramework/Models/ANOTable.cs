using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.EntityFramework.Helpers;
using Rdmp.Core.ReusableLibraryCode.Checks;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.EntityFramework.Models
{
    public class ANOTable: DatabaseObject, ICheckable
    {
        [Key]
        public override int ID { get; set; }
        public string Name { get; set; }
        public int Server_ID { get; set; }
        public int NumberOfIntegersToUseInAnonymousRepresentation { get; set; }
        public int NumberOfCharactersToUseInAnonymousRepresentation { get; set; }
        public string Suffix { get; set; }

        public void Check(ICheckNotifier notifier)
        {
            throw new NotImplementedException();
        }
        public string GetRuntimeDataType(LoadStage stage)
        {
            return "TODO";
        }
    }
}
