using System;
using System.Collections.Generic;
using System.Data.Common;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Data.Pipelines
{
    /// <summary>
    /// Each PipelineComponent can have 0 or more PipelineComponentArguments, these function exactly like the relationship between ProcessTask and ProcessTaskArgument and
    /// reflect a [DemandsInitialization] property on a class of type IDataFlowComponent which is built and populated by reflection from the PipelineComponent (serialization)
    /// </summary>
    public class PipelineComponentArgument : Argument, IPipelineComponentArgument
    {
        #region Database Properties

        private int _pipelineComponentID;

        public int PipelineComponent_ID
        {
            get { return _pipelineComponentID; }
            set { SetField(ref  _pipelineComponentID, value); }
        }

        #endregion

        public PipelineComponentArgument(ICatalogueRepository repository, PipelineComponent parent)
        {
            repository.InsertAndHydrate(this, new Dictionary<string, object>() { 
                {"PipelineComponent_ID",parent.ID},
                {"Name", "Parameter" + Guid.NewGuid()},
                {"Type", typeof (string).ToString()} });
        }

        public PipelineComponentArgument(ICatalogueRepository repository, DbDataReader r)
            : base(repository, r)
        {
           PipelineComponent_ID = int.Parse(r["PipelineComponent_ID"].ToString());
           Type = r["Type"].ToString();
           Name = r["Name"].ToString();
           Value = r["Value"] as string;
           Description = r["Description"] as string;
        }

        
        public override string ToString()
        {
            return Name;
        }


      
    }
}
