using Rdmp.Core.Repositories;
using ReusableLibraryCode.Checks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rdmp.Core.Curation.Data.Cohort
{
    /// <summary>
    /// Handles combining 2 <see cref="CohortIdentificationConfiguration"/> into a single new combined config.  Also handles splitting 1 into 2 (i.e. the reverse).
    /// </summary>
    public class CohortIdentificationConfigurationMerger
    {
        private readonly CatalogueRepository _repository;

        public CohortIdentificationConfigurationMerger(CatalogueRepository repository)
        {
            this._repository = repository;
        }

        /// <summary>
        /// Clones and combines two <see cref="CohortIdentificationConfiguration"/> into a single new cic.
        /// </summary>
        /// <param name="cic1"></param>
        /// <param name="cic2"></param>
        /// <param name="operation"></param>
        public void Merge(CohortIdentificationConfiguration cic1, CohortIdentificationConfiguration cic2, SetOperation operation)
        {
            //clone them
            try
            {
                cic1 = cic1.CreateClone(new ThrowImmediatelyCheckNotifier());
                cic2 = cic2.CreateClone(new ThrowImmediatelyCheckNotifier());
            }
            catch(Exception ex)
            {
                throw new Exception("Error during pre merge cloning stage, no merge will be attempted",ex);
            }
            

            using(var trans = _repository.BeginNewTransactedConnection())
            {
                // Create a new master configuration
                var cicMaster = new CohortIdentificationConfiguration(_repository,$"Merged cic ({cic1.ID } with {cic2.ID})" );
                
                // With a single top level container with the provided operation
                cicMaster.CreateRootContainerIfNotExists();
                var rootContainer = cicMaster.RootCohortAggregateContainer;
                rootContainer.Operation = operation;
                rootContainer.SaveToDatabase();
                
                //Grab the root container of each of the input cics
                var cont1 = cic1.RootCohortAggregateContainer;
                var cont2 = cic2.RootCohortAggregateContainer;

                //clear them to avoid dual parentage
                cic1.RootCohortAggregateContainer_ID = null;
                cic1.SaveToDatabase();

                cic2.RootCohortAggregateContainer_ID = null;
                cic2.SaveToDatabase();

                //add both to the new master cic root container
                rootContainer.AddChild(cont1);
                rootContainer.AddChild(cont2);

                // Make the new name of all the AggregateConfigurations match the new master cic
                foreach(var child in cont1.GetAllAggregateConfigurationsRecursively().Union(cont2.GetAllAggregateConfigurationsRecursively()))
                    cicMaster.EnsureNamingConvention(child);
                
                // Delete the old now empty clones
                cic1.DeleteInDatabase();
                cic2.DeleteInDatabase();

                //finish transaction
                _repository.EndTransactedConnection(true);
            }
        }

        /// <summary>
        /// Splits the root container of a <see cref="CohortIdentificationConfiguration"/> into multiple new cic.
        /// </summary>
        /// <param name="rootContainer"></param>
        public void UnMerge(CohortAggregateContainer rootContainer)
        {
            if(!rootContainer.IsRootContainer())
                throw new ArgumentException("Container must be a root container to be unmerged",nameof(rootContainer));
            
            if(rootContainer.GetAggregateConfigurations().Any())
                throw new ArgumentException("Container must contain only sub-containers (i.e. no aggregates)",nameof(rootContainer));

            if(rootContainer.GetSubContainers().Length <= 1)
                throw new ArgumentException("Container must contain 2+ sub-containers to be unmerged",nameof(rootContainer));

            var cic = rootContainer.GetCohortIdentificationConfiguration();
                        
            try
            {
                // clone the input cic 
                cic = cic.CreateClone(new ThrowImmediatelyCheckNotifier());

                // grab the new clone root container
                rootContainer = cic.RootCohortAggregateContainer;
            }
            catch(Exception ex)
            {
                throw new Exception("Error during pre merge cloning stage, no UnMerge will be attempted",ex);
            }
                        
            using(var trans = _repository.BeginNewTransactedConnection())
            {
                // For each of these
                foreach(var subContainer in rootContainer.GetSubContainers())
                {
                    // create a new config
                    var newCic = new CohortIdentificationConfiguration(_repository,$"Un Merged { subContainer.Name } ({subContainer.ID }) ");
                    
                    //take the container we are splitting out
                    subContainer.MakeIntoAnOrphan();

                    //make it the root container of the new cic
                    newCic.RootCohortAggregateContainer_ID = subContainer.ID;
                    newCic.SaveToDatabase();
                                        
                    // Make the new name of all the AggregateConfigurations match the new cic
                    foreach(var child in subContainer.GetAllAggregateConfigurationsRecursively())
                        newCic.EnsureNamingConvention(child);
                }

                //Now delete the original clone that we unmerged the containers out of
                cic.DeleteInDatabase();

                //finish transaction
                _repository.EndTransactedConnection(true);
            }
        }

    }
}
