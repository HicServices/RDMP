// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort.Joinables;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Annotations;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.Curation.Data.Cohort;

/// <summary>
/// Cohort identification is achieved by identifying Sets of patients and performing set operations on them e.g. you might identify "all patients who have been prescribed Diazepam"
/// and then EXCEPT "patients who have been prescribed Diazepam before 2000".  This is gives you DISTINCT patients who were FIRST prescribed Diazepam AFTER 2000.  A CohortAggregateContainer
/// is a collection of sets (actually implemented as an AggregateConfiguration) (and optionally subcontainers) which are all separated with the given SetOperation.
/// 
/// <para>There are three SET operations:</para>
/// <para>UNION - Match all patients in any of the child containers/aggregates</para>
/// <para>INTERSECT - Match patients only if they appear in ALL child containers/aggregates</para>
/// <para>EXCEPT - Take patients in the first child container/aggregate and discard any appearing in subsequent child containers/aggregates</para>
/// 
/// </summary>
public class CohortAggregateContainer : DatabaseEntity, IOrderable, INamed, IDisableable, IMightBeReadOnly
{
    #region Database Properties

    private SetOperation _operation;
    private string _name;
    private int _order;
    private bool _isDisabled;

    /// <summary>
    /// Describes how patient identifier sets identified by children (subcontainers and <see cref="AggregateConfiguration"/>s) in this container are combined using
    /// SQL operations (UNION / INTERSECT / EXCEPT).
    /// </summary>
    public SetOperation Operation
    {
        get => _operation;
        set => SetField(ref _operation, value);
    }

    /// <inheritdoc/>
    /// <remarks>Starts out as simply the name of the <see cref="Operation"/> but can be changed by the user e.g. 'EXCEPT - Study Exclusion Criteria
    /// <para>This property should always start with the <see cref="Operation"/> to avoid confusion</para>
    /// </remarks>
    [NotNull]
    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    /// <summary>
    /// The order within the parent <see cref="CohortAggregateContainer"/> (if it is not a Root level container / orphan).  Semantically this position is relevant only for
    /// the <see cref="SetOperation.EXCEPT"/> which takes the first set and throws out all subsequent sets.
    /// <remarks>Also affects the order of IncludeCumulativeTotals</remarks>
    /// </summary>
    public int Order
    {
        get => _order;
        set => SetField(ref _order, value);
    }

    /// <inheritdoc/>
    public bool IsDisabled
    {
        get => _isDisabled;
        set => SetField(ref _isDisabled, value);
    }

    #endregion


    public CohortAggregateContainer()
    {
    }

    internal CohortAggregateContainer(ICatalogueRepository repository, DbDataReader r)
        : base(repository, r)
    {
        Order = int.Parse(r["Order"].ToString());
        Enum.TryParse(r["Operation"].ToString(), out SetOperation op);
        Operation = op;
        Name = r["Name"].ToString();
        IsDisabled = Convert.ToBoolean(r["IsDisabled"]);
    }

    /// <summary>
    /// Creates a new container (which starts out as an orphan) with the given <see cref="SetOperation"/>.  You should either set a
    ///  <see cref="CohortIdentificationConfiguration.RootCohortAggregateContainer_ID"/> to this.<see cref="IMapsDirectlyToDatabaseTable.ID"/> to make this container the root container
    /// or use <see cref="AddChild(CohortAggregateContainer)"/>  on another container to make this a subcontainer of it.
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="operation"></param>
    public CohortAggregateContainer(ICatalogueRepository repository, SetOperation operation)
    {
        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "Operation", operation.ToString() },
            { "Order", 0 },
            { "Name", operation.ToString() }
        });
    }


    /// <summary>
    /// Gets all the subcontainers of the current container (if any)
    /// <para>You might want to instead use <seealso cref="GetOrderedContents"/></para>
    /// </summary>
    /// <returns></returns>
    public CohortAggregateContainer[] GetSubContainers() => CatalogueRepository.CohortContainerManager.GetChildren(this)
        .OfType<CohortAggregateContainer>().ToArray();

    /// <summary>
    /// Gets the parent container of the current container (if it is not a root / orphan container)
    /// </summary>
    /// <returns></returns>
    public CohortAggregateContainer GetParentContainerIfAny() =>
        CatalogueRepository.CohortContainerManager.GetParent(this);

    /// <summary>
    /// Returns all the cohort identifier set queries (See <see cref="AggregateConfiguration"/>) declared as immediate children of the container.  These exist in
    /// order defined by <see cref="IOrderable.Order"/> and can be interspersed with subcontainers (<see cref="GetSubContainers"/>).
    /// <para>You might want to instead use <seealso cref="GetOrderedContents"/></para>
    /// </summary>
    /// <returns></returns>
    public AggregateConfiguration[] GetAggregateConfigurations() => CatalogueRepository.CohortContainerManager
        .GetChildren(this).OfType<AggregateConfiguration>().ToArray();

    /// <summary>
    /// Makes the configuration a member of this container.
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="order"></param>
    public void AddChild(AggregateConfiguration configuration, int order)
    {
        CreateInsertionPointAtOrder(configuration, configuration.Order, true);
        CatalogueRepository.CohortContainerManager.Add(this, configuration, order);
        configuration.ReFetchOrder();
    }


    /// <summary>
    /// Removes the given <see cref="AggregateConfiguration"/> from this container if it is an immediate child.
    /// <para>Has no effect if if the <see cref="AggregateConfiguration"/> is not an immediate child</para>
    /// </summary>
    /// <param name="configuration"></param>
    public void RemoveChild(AggregateConfiguration configuration)
    {
        CatalogueRepository.CohortContainerManager.Remove(this, configuration);
    }


    /// <summary>
    /// Deletes all relationships in which this has a parent - kills all containers parents
    /// </summary>
    public void MakeIntoAnOrphan()
    {
        var parent = GetParentContainerIfAny();
        if (parent != null)
            CatalogueRepository.CohortContainerManager.Remove(parent, this);
    }


    /// <summary>
    /// Makes the other <see cref="CohortAggregateContainer"/> into a subcontainer of this container
    /// </summary>
    /// <param name="child"></param>
    public void AddChild(CohortAggregateContainer child)
    {
        if (child.IsRootContainer())
            throw new InvalidOperationException("Root containers cannot be added as subcontainers");

        CreateInsertionPointAtOrder(child, child.Order, true);
        CatalogueRepository.CohortContainerManager.Add(this, child);
    }

    /// <inheritdoc/>
    /// <remarks>Also deletes subcontainers to avoid leaving orphans in the database</remarks>
    public override void DeleteInDatabase()
    {
        MakeIntoAnOrphan();

        var children = GetSubContainers();

        //delete the children
        foreach (var subContainer in children)
            subContainer.DeleteInDatabase();

        //now delete this
        base.DeleteInDatabase();
    }

    /// <inheritdoc/>
    public override string ToString() => Name;

    public bool ShouldBeReadOnly(out string reason)
    {
        var cic = GetCohortIdentificationConfiguration();

        if (cic == null)
        {
            reason = null;
            return false;
        }

        return cic.ShouldBeReadOnly(out reason);
    }

    /// <summary>
    /// Returns true if this.Children contains the thing you are looking for - IMPORTANT: also returns true if we are the thing you are looking for
    /// </summary>
    /// <param name="potentialChild"></param>
    /// <returns></returns>
    public bool HasChild(CohortAggregateContainer potentialChild)
    {
        var foundChildThroughRecursion = false;

        //recurse into all children
        foreach (var c in GetSubContainers())
            if (c.HasChild(
                    potentialChild)) //ask children recursively the same question (see return statement for the question we are asking)
                foundChildThroughRecursion = true;

        //are we the one you are looking for or were any of our children
        return potentialChild.ID == ID || foundChildThroughRecursion;
    }

    /// <summary>
    /// Returns true if the supplied <seealso cref="AggregateConfiguration"/> is a child of this container or any of its subcontainers (recursively)
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public bool HasChild(AggregateConfiguration configuration)
    {
        var foundChildThroughRecursion = false;

        //recurse into all children
        foreach (var c in GetSubContainers())
            if (c.HasChild(
                    configuration)) //ask children recursively the same question (see return statement for the question we are asking)
                foundChildThroughRecursion = true;

        //are any of the configurations in this bucket the one you are looking for
        return
            GetAggregateConfigurations().Any(c => c.ID == configuration.ID) //yes
            || foundChildThroughRecursion; //no but a child had it
    }

    /// <summary>
    /// Returns all subcontainers and identifier sets (<see cref="AggregateConfiguration"/>) of this container in order (See <see cref="Order"/>)
    /// </summary>
    /// <returns></returns>
    public IOrderedEnumerable<IOrderable> GetOrderedContents()
    {
        return CatalogueRepository.CohortContainerManager.GetChildren(this).OrderBy(o => o.Order);
    }

    /// <summary>
    /// Returns all <see cref="AggregateConfiguration"/> identifier sets in this container or any subcontainers
    /// </summary>
    /// <returns></returns>
    public List<AggregateConfiguration> GetAllAggregateConfigurationsRecursively()
    {
        var toReturn = new List<AggregateConfiguration>();

        toReturn.AddRange(GetAggregateConfigurations());

        foreach (var subContainer in GetSubContainers())
            toReturn.AddRange(subContainer.GetAllAggregateConfigurationsRecursively());

        return toReturn;
    }


    /// <summary>
    /// Creates a new CohortAggregateContainer tree containing a clone container for each container in the original tree and a clone AggregateConfiguration for each in the original tree
    /// but with a rename in which AggregateConfigurations in the first tree are expected to start cic_X where X is the original cohort identification configuration ID, this will be replaced
    /// with the new clone's ID
    /// </summary>
    /// <param name="notifier"></param>
    /// <param name="original"></param>
    /// <param name="clone"></param>
    /// <param name="parentToCloneJoinablesDictionary"></param>
    public CohortAggregateContainer CloneEntireTreeRecursively(ICheckNotifier notifier,
        CohortIdentificationConfiguration original, CohortIdentificationConfiguration clone,
        Dictionary<JoinableCohortAggregateConfiguration, JoinableCohortAggregateConfiguration>
            parentToCloneJoinablesDictionary)
    {
        //what is in us?
        var contents = GetOrderedContents();

        //clone us with same order (in parents)
        var cloneContainer = new CohortAggregateContainer((ICatalogueRepository)Repository, Operation)
        {
            Name = Name,
            Order = Order
        };
        cloneContainer.SaveToDatabase();


        //for each thing in us
        foreach (var content in contents)
        {
            var order = content.Order;

            //its a config, clone the config and add it to the clone container
            if (content is AggregateConfiguration config)
            {
                var configClone = clone.ImportAggregateConfigurationAsIdentifierList(config, null, false);
                notifier.OnCheckPerformed(new CheckEventArgs(
                    $"Created clone dataset {configClone} with ID {configClone.ID}", CheckResult.Success));
                cloneContainer.AddChild(configClone, order);

                //if the original used any joinable patient index tables
                var usedJoins = config.PatientIndexJoinablesUsed;

                //our dictionary should have a record of it along with a clone patient index table we should hook our clone up to
                foreach (var j in usedJoins)
                {
                    //for some reason the CohortIdentificationConfiguration didn't properly clone the joinable permission or didn't add it to the dictionary
                    if (!parentToCloneJoinablesDictionary.TryGetValue(j.JoinableCohortAggregateConfiguration, out var
                            cloneJoinable))
                        throw new KeyNotFoundException(
                            $"Configuration {configClone} uses Patient Index Table {j.AggregateConfiguration} but our dictionary did not have the key, why was that joinable not cloned?");

                    //we do have a clone copy of the joinable permission, set the clone aggregate
                    var cloneJoinUse = cloneJoinable.AddUser(configClone);

                    cloneJoinUse.JoinType = j.JoinType;
                    cloneJoinUse.SaveToDatabase();

                    //Now! (brace yourself).  Some the filters in the AggregateConfiguration we just cloned might reference a table called ix2934 or whetever, this
                    //is the Joinable we need to do a replace to point them at the correct ix number (although if they are good users they will have aliased any
                    //patient index columns anyway)
                    if (configClone.RootFilterContainer_ID != null)
                        foreach (var clonedFilter in SqlQueryBuilderHelper.GetAllFiltersUsedInContainerTreeRecursively(
                                     configClone.RootFilterContainer))
                        {
                            var oldTableAlias = j.GetJoinTableAlias();
                            var newTableAlias = cloneJoinUse.GetJoinTableAlias();

                            clonedFilter.WhereSQL = clonedFilter.WhereSQL.Replace(oldTableAlias, newTableAlias);
                            clonedFilter.SaveToDatabase();
                        }
                }
            }

            //its another container (a subcontainer), recursively call the clone operation on it and add that subtree to the clone container
            if (content is CohortAggregateContainer container)
            {
                var cloneSubContainer =
                    container.CloneEntireTreeRecursively(notifier, original, clone, parentToCloneJoinablesDictionary);

                notifier.OnCheckPerformed(new CheckEventArgs(
                    $"Created clone container {cloneSubContainer} with ID {cloneSubContainer.ID}",
                    CheckResult.Success));
                cloneContainer.AddChild(cloneSubContainer);
            }
        }

        //return the clone we created
        return cloneContainer;
    }

    /// <summary>
    /// Returns the <see cref="CohortIdentificationConfiguration"/> that this container is a part of either as a root container or contained with in a subcontainer of
    /// the root container.
    /// <para>Returns null if the container is an orphan</para>
    /// </summary>
    /// <returns></returns>
    public CohortIdentificationConfiguration GetCohortIdentificationConfiguration()
    {
        var candidates = Repository.GetAllObjects<CohortIdentificationConfiguration>().ToArray();
        var container = this;

        //while there is a container
        while (container != null)
        {
            //see if it is a root container
            var toReturn = candidates.SingleOrDefault(c => c.RootCohortAggregateContainer_ID == container.ID);

            //it is a root container!
            if (toReturn != null)
                return toReturn;

            //it is not a root container, either the container is an orphan (very bad) or its parent is a root container (or its parent and so on)
            //either way get the parent
            container = container.GetParentContainerIfAny();
        }

        return null;
    }

    /// <summary>
    /// Moves all children containers/identifier lists (See <see cref="GetOrderedContents"/>) to make space for inserting a new one at the specified
    /// Order (See <see cref="Order"/>).
    /// </summary>
    /// <param name="makeRoomFor"></param>
    /// <param name="order"></param>
    /// <param name="incrementOrderOfCollisions"></param>
    public void CreateInsertionPointAtOrder(IOrderable makeRoomFor, int order, bool incrementOrderOfCollisions)
    {
        var contents = GetOrderedContents().ToArray();

        //if there is nobody at that order then we are good
        if (contents.All(c => c.Order != order))
            return;

        foreach (var orderable in contents)
        {
            if (orderable.Order < order)
                orderable.Order--;
            else if (orderable.Order > order)
                orderable.Order++;
            else //collision on order
                orderable.Order += incrementOrderOfCollisions ? 1 : -1;
            ((ISaveable)orderable).SaveToDatabase();
        }
    }

    /// <summary>
    /// Returns a list of all the <see cref="CohortAggregateContainer"/> that are subcontainers of the this.  This includes all children and children
    /// of children etc recursively.
    /// </summary>
    /// <returns></returns>
    public List<CohortAggregateContainer> GetAllSubContainersRecursively()
    {
        var toReturn = new List<CohortAggregateContainer>();

        var subs = GetSubContainers();
        toReturn.AddRange(subs);

        foreach (var sub in subs)
            toReturn.AddRange(sub.GetAllSubContainersRecursively());

        return toReturn;
    }

    /// <summary>
    /// Returns true if this a cohort set and is the topmost (root) SET container of a <see cref="CohortIdentificationConfiguration"/>.
    /// </summary>
    /// <returns></returns>
    public bool IsRootContainer()
    {
        var cic = GetCohortIdentificationConfiguration();
        return cic?.RootCohortAggregateContainer_ID == ID;
    }

    /// <summary>
    /// Returns all containers that exist above the current container (up to the root container of the CohortIdentificationConfiguration)
    /// </summary>
    /// <returns></returns>
    public IEnumerable<CohortAggregateContainer> GetAllParentContainers()
    {
        var container = this;

        while (container != null)
        {
            container = container.GetParentContainerIfAny();
            if (container != null)
                yield return container;
        }
    }
}