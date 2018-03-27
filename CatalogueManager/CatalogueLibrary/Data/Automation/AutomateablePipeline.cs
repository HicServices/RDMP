using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace CatalogueLibrary.Data.Automation
{
    /// <summary>
    /// A user custom automated activity.  This involves the user having a plugin which has automation components and building a pipeline in which an automation service serves up
    /// jobs and the automation destination component executes those jobs.  What the pipeline will actually do when executed is entirely up to the functionality in the plugin.
    /// 
    /// If you have an automation server running the automation executable and a free AutomationServiceSlot in your Catalogue database then the pipeline will be assembled and
    /// executed until the source returns null (no more jobs).  If you are a plugin writer you should make sure to put some kind of threshold on the source so that it doesn't 
    /// serve up 1000 async jobs at once.
    /// </summary>
    public class AutomateablePipeline:DatabaseEntity,IHasDependencies
    {
        private string _pipelineName;

        #region Database Properties
        private int _automationServiceSlotID;
        private int _pipelineID;

        public int AutomationServiceSlot_ID
        {
            get { return _automationServiceSlotID; }
            set { SetField(ref _automationServiceSlotID , value); }
        }

        public int Pipeline_ID
        {
            get { return _pipelineID; }
            set { SetField(ref _pipelineID , value); }
        }
        #endregion

        #region Relationships
        [NoMappingToDatabase]
        public AutomationServiceSlot AutomationServiceSlot
        {
            get{return Repository.GetObjectByID<AutomationServiceSlot>(AutomationServiceSlot_ID);}
        }

        [NoMappingToDatabase]
        public Pipeline Pipeline
        {
            get { return Repository.GetObjectByID<Pipeline>(Pipeline_ID); }
        }
        #endregion

        internal AutomateablePipeline(ICatalogueRepository repository, DbDataReader r): base(repository, r)
        {
            AutomationServiceSlot_ID = Convert.ToInt32(r["AutomationServiceSlot_ID"]);
            Pipeline_ID = Convert.ToInt32(r["Pipeline_ID"]);
        }

        public AutomateablePipeline(ICatalogueRepository repository, AutomationServiceSlot allocatedToSlot, Pipeline pipeline = null)
        {
            if(pipeline == null)
                pipeline = new Pipeline(repository,"New Automated Pipeline " + Guid.NewGuid());

            repository.InsertAndHydrate(this,new Dictionary<string, object>()
            {
                {"AutomationServiceSlot_ID",allocatedToSlot.ID},
                {"Pipeline_ID",pipeline.ID}
            });
        }

        public override string ToString()
        {
            CachePipelineNameIfRequired();

            return _pipelineName;
        }

        private void CachePipelineNameIfRequired()
        {
            if (_pipelineName == null)
                _pipelineName = Pipeline.Name;
        }

        public IHasDependencies[] GetObjectsThisDependsOn()
        {
            return new IHasDependencies[] { AutomationServiceSlot };
        }

        public IHasDependencies[] GetObjectsDependingOnThis()
        {

            return new IHasDependencies[] {Pipeline};
        }
    }
}
