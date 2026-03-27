using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.EntityFramework.Helpers;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Revertable;
using Rdmp.Core.ReusableLibraryCode.Checks;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Rdmp.Core.EntityFramework.Models
{
    [Table("ProcessTask")]
    public class ProcessTask : DatabaseObject, IProcessTask, ICheckable
    {
        [Key]
        public override int ID { get; set; }

        [Required]
        public int LoadMetadata_ID { get; set; }

        public string Path { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(50)")]
        public int ProcessTaskType { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(50)")]
        public int LoadStage { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public int Order { get; set; }

        [Required]
        public bool IsDisabled { get; set; }

        public string SerialisableConfiguration { get; set; }

        [ForeignKey("LoadMetadata_ID")]
        public virtual LoadMetadata LoadMetadata { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public virtual ICollection<ProcessTaskArgument> ProcessTaskArguments { get; set; }

        public int? RelatesSolelyToCatalogue_ID => throw new NotImplementedException();

        IEnumerable<Curation.Data.DataLoad.ProcessTaskArgument> IProcessTask.ProcessTaskArguments => throw new NotImplementedException();

        LoadStage IProcessTask.LoadStage => throw new NotImplementedException();

        ProcessTaskType IProcessTask.ProcessTaskType => throw new NotImplementedException();


        public bool IsPluginType() => (ProcessTaskType)ProcessTaskType == Curation.Data.DataLoad.ProcessTaskType.Attacher ||
                                  (ProcessTaskType)ProcessTaskType == Curation.Data.DataLoad.ProcessTaskType.MutilateDataTable ||
                                  (ProcessTaskType)ProcessTaskType == Curation.Data.DataLoad.ProcessTaskType.DataProvider;
        public IArgument[] CreateArgumentsForClassIfNotExists(Type t) =>
       ArgumentFactory.CreateArgumentsForClassIfNotExistsGeneric(
               t,

               //tell it how to create new instances of us related to parent
               this,

               //what arguments already exist
               GetAllArguments().ToArray())

           //convert the result back from generic to specific (us)
           .ToArray();

        /// <inheritdoc/>
        public IArgument[] CreateArgumentsForClassIfNotExists<T>() => CreateArgumentsForClassIfNotExists(typeof(T));

        public IArgument CreateNewArgument()
        {
            throw new NotImplementedException();
        }

        public bool Exists()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IArgument> GetAllArguments() => CatalogueDbContext.ProcessTaskArguments.Where(pta => pta.ProcessTask_ID == ID).AsEnumerable();

        public string GetClassNameWhoArgumentsAreFor()
        {
            return Path;
        }

        public RevertableObjectReport HasLocalChanges()
        {
            throw new NotImplementedException();
        }

        public void RevertToDatabaseState()
        {
            throw new NotImplementedException();
        }

        public void SaveToDatabase()
        {
            throw new NotImplementedException();
        }

        public void Check(ICheckNotifier notifier)
        {
            throw new NotImplementedException();
        }
    }
}
