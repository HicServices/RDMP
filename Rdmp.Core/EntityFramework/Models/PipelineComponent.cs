using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.EntityFramework.Helpers;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.EntityFramework.Models
{
    [Table("PipelineComponent")]
    public class PipelineComponent: DatabaseObject, IPipelineComponent
    {
        [Key]
        public override int ID { get; set; }
        public int Order { get; set; }

        public int Pipeline_ID { get; set; }

        public string Name { get; set; }

        public override string ToString() => Name;
        public string Class { get; set; }

        [ForeignKey("Pipeline_ID")]
        public virtual Pipeline Pipeline { get; set; }

        public virtual List<PipelineComponentArgument> Arguments { get; set; } = new();

        public IEnumerable<IPipelineComponentArgument> PipelineComponentArguments => throw new NotImplementedException();

        public Curation.Data.Pipelines.PipelineComponent Clone(Curation.Data.Pipelines.Pipeline intoTargetPipeline)
        {
            throw new NotImplementedException();
        }

        public IArgument[] CreateArgumentsForClassIfNotExists<T>()
        {
            throw new NotImplementedException();
        }

        public IArgument[] CreateArgumentsForClassIfNotExists(Type underlyingComponentType)
        {
            throw new NotImplementedException();
        }

        public IArgument CreateNewArgument()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IArgument> GetAllArguments()
        {
            throw new NotImplementedException();
        }

        public Type GetClassAsSystemType() => MEF.GetType(Class);

        public string GetClassNameLastPart()
        {
            throw new NotImplementedException();
        }

        public string GetClassNameWhoArgumentsAreFor()
        {
            throw new NotImplementedException();
        }

        public IHasDependencies[] GetObjectsDependingOnThis()
        {
            throw new NotImplementedException();
        }

        public IHasDependencies[] GetObjectsThisDependsOn()
        {
            throw new NotImplementedException();
        }

        public void SaveToDatabase()
        {
            throw new NotImplementedException();
        }
    }
}
