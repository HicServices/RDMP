using System;
using System.Collections.Generic;
using System.Data.Common;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace CatalogueLibrary.Data.Pipelines
{
    /// <summary>
    /// Each PipelineComponent can have 0 or more PipelineComponentArguments, these function exactly like the relationship between ProcessTask and ProcessTaskArgument and
    /// reflect a [DemandsInitialization] property on a class of type IDataFlowComponent which is built and populated by reflection from the PipelineComponent (serialization)
    /// 
    /// <para>See Pipeline and PipelineComponent for more information about this</para>
    /// </summary>
    public class PipelineComponentArgument : Argument, IPipelineComponentArgument
    {
        #region Database Properties

        private int _pipelineComponentID;

        /// <inheritdoc/>
        public int PipelineComponent_ID
        {
            get { return _pipelineComponentID; }
            set { SetField(ref  _pipelineComponentID, value); }
        }

        #endregion

        #region Relationship Properties

        /// <inheritdoc cref="PipelineComponent_ID"/>
        [NoMappingToDatabase]
        public IHasDependencies PipelineComponent { get
        {
            return Repository.GetObjectByID<PipelineComponent>(PipelineComponent_ID);
        }}

        #endregion

        /// <summary>
        /// Creates a new argument storage object for one of the arguments in <see cref="PipelineComponent"/>.  
        /// 
        /// <para>You should probably call <see cref="PipelineComponent.CreateArgumentsForClassIfNotExists"/> intead</para>
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="parent"></param>
        public PipelineComponentArgument(ICatalogueRepository repository, PipelineComponent parent)
        {
            repository.InsertAndHydrate(this, new Dictionary<string, object>() { 
                {"PipelineComponent_ID",parent.ID},
                {"Name", "Parameter" + Guid.NewGuid()},
                {"Type", typeof (string).ToString()} });
        }

        internal PipelineComponentArgument(ICatalogueRepository repository, DbDataReader r)
            : base(repository, r)
        {
           PipelineComponent_ID = int.Parse(r["PipelineComponent_ID"].ToString());
           Type = r["Type"].ToString();
           Name = r["Name"].ToString();
           Value = r["Value"] as string;
           Description = r["Description"] as string;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }

        /// <inheritdoc/>
        public IHasDependencies[] GetObjectsThisDependsOn()
        {
            return new[] {PipelineComponent};
        }
        /// <inheritdoc/>
        public IHasDependencies[] GetObjectsDependingOnThis()
        {
            return new IHasDependencies[0];
        }
    }
}
