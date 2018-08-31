using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Injection;
using ReusableLibraryCode;

namespace CatalogueLibrary.Data.Pipelines
{
    /// <inheritdoc cref="IPipeline"/>
    public class Pipeline : VersionedDatabaseEntity, IPipeline,IHasDependencies
    {
        #region Database Properties

        private string _name;
        private string _description;
        private int? _destinationPipelineComponentID;
        private int? _sourcePipelineComponentID;

        /// <inheritdoc/>
        public string Name
        {
            get { return _name; }
            set { SetField(ref  _name, value); }
        }

        /// <inheritdoc/>
        public string Description
        {
            get { return _description; }
            set { SetField(ref  _description, value); }
        }

        /// <inheritdoc/>
        public int? DestinationPipelineComponent_ID
        {
            get { return _destinationPipelineComponentID; }
            set { SetField(ref  _destinationPipelineComponentID, value); }
        }
        /// <inheritdoc/>
        public int? SourcePipelineComponent_ID
        {
            get { return _sourcePipelineComponentID; }
            set { SetField(ref  _sourcePipelineComponentID, value); }
        }

        #endregion

        #region Relationships
        /// <inheritdoc/>
        [NoMappingToDatabase]
        public IList<IPipelineComponent> PipelineComponents { get
        {
            return _knownPipelineComponents.Value;
        }}

        /// <inheritdoc/>
        [NoMappingToDatabase]
        public IPipelineComponent Destination {
            get
            {
                return DestinationPipelineComponent_ID == null
                    ? null
                    :_knownPipelineComponents.Value.Single(c=>c.ID == DestinationPipelineComponent_ID.Value);
            }
        }
        /// <inheritdoc/>
        [NoMappingToDatabase]
        public IPipelineComponent Source
        {
            get
            {
                return SourcePipelineComponent_ID == null
                    ? null
                    :_knownPipelineComponents.Value.Single(c=>c.ID == SourcePipelineComponent_ID.Value);
            }
        }

        #endregion

        public Pipeline(ICatalogueRepository repository, string name = null)
        {
            repository.InsertAndHydrate(this, new Dictionary<string, object>
            {
                {"Name", name ?? "NewPipeline " + Guid.NewGuid()}
            });
            
            ClearAllInjections();
        }

        internal Pipeline(ICatalogueRepository repository, DbDataReader r)
            : base(repository, r)
        {
            Name = r["Name"].ToString();

            object o=  r["DestinationPipelineComponent_ID"];
            if (o == DBNull.Value)
                DestinationPipelineComponent_ID = null;
            else
                DestinationPipelineComponent_ID = Convert.ToInt32(o);

            o = r["SourcePipelineComponent_ID"];
            if (o == DBNull.Value)
                SourcePipelineComponent_ID = null;
            else
                SourcePipelineComponent_ID = Convert.ToInt32(o);

            Description = r["Description"] as string;

            ClearAllInjections();
        }
        
        public override string ToString()
        {
            return Name;
        }

        public Pipeline Clone()
        {
            var clonePipe = new Pipeline((ICatalogueRepository)Repository, Name + "(Clone)");
            clonePipe.Description = Description;

            var originalSource = Source;
            if (originalSource != null)
            {
                var cloneSource = originalSource.Clone(clonePipe);
                clonePipe.SourcePipelineComponent_ID = cloneSource.ID;
            }
            var originalDestination = Destination;
            if (originalDestination != null)
            {
                var cloneDestination = originalDestination.Clone(clonePipe);
                clonePipe.DestinationPipelineComponent_ID = cloneDestination.ID;
            }

            clonePipe.SaveToDatabase();

            foreach (IPipelineComponent component in PipelineComponents)
            {
                //if the component is one of the ones we already cloned
                if (Equals(originalSource, component) || Equals(originalDestination, component))
                    continue;

                component.Clone(clonePipe);
            }


            return clonePipe;
        }
        /// <inheritdoc/>
        public IHasDependencies[] GetObjectsThisDependsOn()
        {
            return new IHasDependencies[0];
        }
        /// <inheritdoc/>
        public IHasDependencies[] GetObjectsDependingOnThis()
        {
            return PipelineComponents.Cast<IHasDependencies>().ToArray();
        }

        private Lazy<IList<IPipelineComponent>> _knownPipelineComponents;
        /// <inheritdoc/>
        public void InjectKnown(IPipelineComponent[] instance)
        {
            _knownPipelineComponents = new Lazy<IList<IPipelineComponent>>(()=>instance.OrderBy(p=>p.Order).ToList());
        }
        /// <inheritdoc/>
        public void ClearAllInjections()
        {
            _knownPipelineComponents = new Lazy<IList<IPipelineComponent>>(FetchPipelineComponents);
        }

        private IList<IPipelineComponent> FetchPipelineComponents()
        {
            return Repository.GetAllObjectsWithParent<PipelineComponent>(this)
                .Cast<IPipelineComponent>()
                .OrderBy(p => p.Order)
                .ToList();
        }
    }
}
