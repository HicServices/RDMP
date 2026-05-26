using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cache;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.EntityFramework.Helpers;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Revertable;
using Rdmp.Core.ReusableLibraryCode.Checks;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.EntityFramework.Models
{
    [Table("LoadProgress")]
    public class LoadProgress : DatabaseObject, ILoadProgress
    {
        [Key]
        public override int ID { get; set; }
        public DateTime? OriginDate { get; set; }
        public DateTime? DataLoadProgress { get; set ; }
        public int LoadMetadata_ID { get ; set ; }

        [ForeignKey("LoadMetadata_ID")]
        public virtual LoadMetadata LoadMetadata { get; set; }

        public ICacheProgress CacheProgress => throw new NotImplementedException();

        public bool IsDisabled { get; set ; }

        public int DefaultNumberOfDaysToLoadEachTime { get; set; }

        public string Name { get ; set ; }

        ILoadMetadata ILoadProgress.LoadMetadata => (ILoadMetadata)LoadMetadata;

        public override string ToString()
        {
            return Name;
        }

        public void Check(ICheckNotifier notifier)
        {
            throw new NotImplementedException();
        }

        public bool Exists()
        {
            throw new NotImplementedException();
        }

        public RevertableObjectReport HasLocalChanges()
        {
            throw new NotImplementedException();
        }

        public void RevertToDatabaseState()
        {
            throw new NotImplementedException();
        }

    }
}
