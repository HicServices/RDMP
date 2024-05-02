// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.Curation.Data.Cohort;

/// <summary>
///     Handles combining 2 <see cref="CohortIdentificationConfiguration" /> into a single new combined config.  Also
///     handles splitting 1 into 2 (i.e. the reverse).
/// </summary>
public class CohortIdentificationConfigurationMerger
{
    private readonly ICatalogueRepository _repository;

    public CohortIdentificationConfigurationMerger(ICatalogueRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    ///     Clones and combines two or more <see cref="CohortIdentificationConfiguration" /> into a single new cic.
    /// </summary>
    /// <param name="cics"></param>
    /// <param name="operation"></param>
    /// <returns>
    ///     The new merged CohortIdentificationConfiguration which contains all the provided <paramref name="cics" />
    /// </returns>
    public CohortIdentificationConfiguration Merge(CohortIdentificationConfiguration[] cics, SetOperation operation)
    {
        if (cics.Length <= 1)
            throw new ArgumentException("You must select at least 2 cics to merge", nameof(cics));

        //clone them
        var cicClones = new CohortIdentificationConfiguration[cics.Length];
        try
        {
            for (var i = 0; i < cics.Length; i++)
                cicClones[i] = cics[i].CreateClone(ThrowImmediatelyCheckNotifier.Quiet);
        }
        catch (Exception ex)
        {
            throw new Exception("Error during pre merge cloning stage, no merge will be attempted", ex);
        }

        using (_repository.BeginNewTransaction())
        {
            // Create a new master configuration
            var cicMaster = new CohortIdentificationConfiguration(_repository,
                $"Merged cics (IDs {string.Join(",", cics.Select(c => c.ID))})");

            // With a single top level container with the provided operation
            cicMaster.CreateRootContainerIfNotExists();
            var rootContainer = cicMaster.RootCohortAggregateContainer;
            rootContainer.Operation = operation;
            rootContainer.SaveToDatabase();

            //Grab the root container of each of the input cics
            foreach (var cic in cicClones)
            {
                var container = cic.RootCohortAggregateContainer;

                //clear them to avoid dual parentage
                cic.RootCohortAggregateContainer_ID = null;
                cic.SaveToDatabase();

                //add to the new master cic root container
                rootContainer.AddChild(container);

                // Make the new name of all the AggregateConfigurations match the new master cic
                foreach (var child in container.GetAllAggregateConfigurationsRecursively())
                    EnsureNamingConvention(cicMaster, child);

                // Delete the old now empty clones
                cic.DeleteInDatabase();
            }

            //finish transaction
            _repository.EndTransaction(true);

            return cicMaster;
        }
    }

    /// <summary>
    ///     Clone and import one or more <see cref="CohortIdentificationConfiguration" /> into the target
    ///     <paramref name="into" />
    /// </summary>
    /// <param name="cics"></param>
    /// <param name="into">The container into which you want to add the <paramref name="cics" /></param>
    public void Import(CohortIdentificationConfiguration[] cics, CohortAggregateContainer into)
    {
        var cicInto = into.GetCohortIdentificationConfiguration() ??
                      throw new ArgumentException($"Cannot import into orphan container '{into}'", nameof(into));

        //clone them
        var cicClones = new CohortIdentificationConfiguration[cics.Length];
        try
        {
            for (var i = 0; i < cics.Length; i++)
                cicClones[i] = cics[i].CreateClone(ThrowImmediatelyCheckNotifier.Quiet);
        }
        catch (Exception ex)
        {
            throw new Exception("Error during pre import cloning stage, no import will be attempted", ex);
        }


        using (_repository.BeginNewTransaction())
        {
            //Grab the root container of each of the input cics
            foreach (var cic in cicClones)
            {
                var container = cic.RootCohortAggregateContainer;

                //clear them to avoid dual parentage
                cic.RootCohortAggregateContainer_ID = null;
                cic.SaveToDatabase();

                //add them into the target SET operation container you are importing into
                into.AddChild(container);

                // Make the new name of all the AggregateConfigurations match the owner of import into container
                foreach (var child in container.GetAllAggregateConfigurationsRecursively())
                    EnsureNamingConvention(cicInto, child);

                // Delete the old now empty clones
                cic.DeleteInDatabase();
            }

            //finish transaction
            _repository.EndTransaction(true);
        }
    }


    private static void EnsureNamingConvention(CohortIdentificationConfiguration cic, AggregateConfiguration ac)
    {
        //clear any old cic_x prefixes
        ac.Name = Regex.Replace(ac.Name, $@"^({CohortIdentificationConfiguration.CICPrefix}\d+_?)+", "");
        ac.SaveToDatabase();

        //and add the new correct one
        cic.EnsureNamingConvention(ac);
    }

    /// <summary>
    ///     Splits the root container of a <see cref="CohortIdentificationConfiguration" /> into multiple new cic.
    /// </summary>
    /// <param name="rootContainer"></param>
    /// <returns>All new configurations unmerged out of the <paramref name="rootContainer" /></returns>
    public CohortIdentificationConfiguration[] UnMerge(CohortAggregateContainer rootContainer)
    {
        if (!rootContainer.IsRootContainer())
            throw new ArgumentException("Container must be a root container to be unmerged", nameof(rootContainer));

        if (rootContainer.GetAggregateConfigurations().Any())
            throw new ArgumentException("Container must contain only sub-containers (i.e. no aggregates)",
                nameof(rootContainer));

        if (rootContainer.GetSubContainers().Length <= 1)
            throw new ArgumentException("Container must contain 2+ sub-containers to be unmerged",
                nameof(rootContainer));

        var cic = rootContainer.GetCohortIdentificationConfiguration();
        var toReturn = new List<CohortIdentificationConfiguration>();

        try
        {
            // clone the input cic
            cic = cic.CreateClone(ThrowImmediatelyCheckNotifier.Quiet);

            // grab the new clone root container
            rootContainer = cic.RootCohortAggregateContainer;
        }
        catch (Exception ex)
        {
            throw new Exception("Error during pre merge cloning stage, no UnMerge will be attempted", ex);
        }

        using (_repository.BeginNewTransaction())
        {
            // For each of these
            foreach (var subContainer in rootContainer.GetSubContainers().OrderBy(c => c.Order))
            {
                // create a new config
                var newCic = new CohortIdentificationConfiguration(_repository,
                    $"Un Merged {subContainer.Name} ({subContainer.ID}) ");

                //take the container we are splitting out
                subContainer.MakeIntoAnOrphan();

                //make it the root container of the new cic
                newCic.RootCohortAggregateContainer_ID = subContainer.ID;
                newCic.SaveToDatabase();

                // Make the new name of all the AggregateConfigurations match the new cic
                foreach (var child in subContainer.GetAllAggregateConfigurationsRecursively())
                    EnsureNamingConvention(newCic, child);

                toReturn.Add(newCic);
            }

            //Now delete the original clone that we unmerged the containers out of
            cic.DeleteInDatabase();

            //finish transaction
            _repository.EndTransaction(true);
        }

        return toReturn.ToArray();
    }
}