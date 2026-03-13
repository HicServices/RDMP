using Rdmp.Core.Curation.Data.DataLoad;
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
    [Table("PipelineComponentArgument")]
    public class PipelineComponentArgument: DatabaseObject, IArgument
    {
        [Key]
        public override int ID { get; set; }

        [Column("PipelineComponent_ID")]
        public int PipelineComponent_ID { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }

        [ForeignKey("PipelineComponent_ID")]
        public virtual PipelineComponent PipelineComponent { get; set; }

        public Type GetConcreteSystemType()
        {
            throw new NotImplementedException();
        }

        public Type GetSystemType()
        {
            throw new NotImplementedException();
        }

        public object GetValueAsSystemType()
        {
            throw new NotImplementedException();
        }

        public void SaveToDatabase()
        {
            throw new NotImplementedException();
        }

        public void SetType(Type t)
        {
            throw new NotImplementedException();
        }

        public void SetValue(object o)
        {
            throw new NotImplementedException();
        }
    }
}
