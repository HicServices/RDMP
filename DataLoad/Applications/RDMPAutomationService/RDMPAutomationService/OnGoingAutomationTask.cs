using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Automation;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using RDMPAutomationService.Interfaces;

namespace RDMPAutomationService
{
    /// <summary>
    /// Flow object (T) for Automation Pipelines.  Represents a discrete System.Threading.Tasks.Task created for an IAutomateable (Task will simply run the 
    /// IAutomateable.RunTask method).  Also includes classes for monitoring/updating the job status (Running, Crashed etc) as well as what AutomationJobType it is
    /// etc.  See Automation.cd
    /// </summary>
    public class OnGoingAutomationTask
    {
        public Task Task { get; private set; }
        public AutomationJob Job { get; private set; }
        public CancellationTokenSource CancellationTokenSource{ get; private set; }
        public AutomationJobType JobType { get; private set; }
        public IRepository Repository { get; private set; }

        /// <summary>
        /// Packaged up method (IAutomateable) which will be hosted and started (asynchronously) run by an AutomationDestination while at the same time having it's execution state audited in the 
        /// database (via AutomationJobType). 
        /// </summary>
        /// <param name="slot">The slot locked by the RDMPAutomationLoop which is hosting you.  You should have been told this at some point, possibly in an Initialize() method if you are an IPluginAutomationSource (about the only people who should be constructing these anyway)</param>
        /// <param name="type">An enum indicating the classification of Task underway, if you are unsure pass UserCustomPipeline</param>
        /// <param name="description">Very important! a description of what your IAutomateable is supposed to be doing (including any arguments it might have e.g. which Catalogues it's messing with etc).  This description will be documented in the name of a new AutomationJob under the AutomationServiceSlot </param>
        /// <param name="automateable">A class containing a method which will achieve the stated goal (description), the IAutomateable will have it's RunTask method called asynchronously by the AutomationDestination in parallell with all the other running jobs.  You should make sure to update the AutomationJob regularly during the execution logic of your RunTask method and Delete it at the end if it completed successfully </param>
        public OnGoingAutomationTask(AutomationJob job, IAutomateable automateable)
        {
            Job = job;
            CancellationTokenSource = new CancellationTokenSource();
            Task = new Task(() =>

            {
                try
                {
                    automateable.RunTask(this);
                }
                catch (Exception e)
                {
                    
                    job.SetLastKnownStatus(AutomationJobStatus.Crashed);
                    new AutomationServiceException((ICatalogueRepository)Repository, e);
                }
            }
                
                
                );
            Repository = job.Repository;
        }
    }
}
