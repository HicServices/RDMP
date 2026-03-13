using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.EntityFramework.Helpers;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Revertable;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.EntityFramework.Models
{
    [Table("Pipeline")]
    public class Pipeline: DatabaseObject, IPipeline
    {
        [Key]
        public override int ID { get; set; }

        [NotMapped]
        public override RDMPDbContext CatalogueDbContext { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        public int? SourcePipelineComponent_ID { get; set; }
        public int? DestinationPipelineComponent_ID { get; set; }

        [ForeignKey("SourcePipelineComponent_ID")]
        public virtual PipelineComponent Source { get; set; }

        [ForeignKey("DestinationPipelineComponent_ID")]
        public virtual PipelineComponent Destination { get; set; }

        public List<PipelineComponent> PipelineComponents => CatalogueDbContext.PipelineComponents.ToList().Where(component => component.Pipeline_ID == this.ID).ToList();

        IList<IPipelineComponent> IPipeline.PipelineComponents => PipelineComponents.Select( p => (IPipelineComponent)p).ToList();

        IPipelineComponent IPipeline.Destination => Destination;

        IPipelineComponent IPipeline.Source => Source;

        public void ClearAllInjections()
        {
            throw new NotImplementedException();
        }

        public Curation.Data.Pipelines.Pipeline Clone()
        {
            throw new NotImplementedException();
        }

        public bool Exists() => true;//todo?

        public RevertableObjectReport HasLocalChanges()
        {
            throw new NotImplementedException();
        }

        public void InjectKnown(IPipelineComponent[] instance)
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
    }
}
