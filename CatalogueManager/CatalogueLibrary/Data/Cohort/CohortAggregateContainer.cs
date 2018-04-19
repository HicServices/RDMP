using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort.Joinables;
using CatalogueLibrary.QueryBuilding;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;

namespace CatalogueLibrary.Data.Cohort
{
    /// <summary>
    /// Cohort identification is achieved by identifying Sets of patients and performing set operations on them e.g. you might identify "all patients who have been prescribed Diazepam"
    /// and then EXCEPT "patients who have been prescribed Diazepam before 2000".  This is gives you DISTINCT patients who were FIRST prescribed Diazepam AFTER 2000.  A CohortAggregateContainer
    /// is a collection of sets (actually implemented as an AggregateConfiguration) (and optionally subcontainers) which are all separated with the given SetOperation.
    /// </summary>
    public class CohortAggregateContainer : DatabaseEntity, IDeleteable, ISaveable, IOrderable,INamed
    {
        #region Database Properties

        private SetOperation _operation;
        private string _name;
        private int _order;

        /// <summary>
        /// Describes how patient identifier sets identified by children (subcontainers and <see cref="AggregateConfiguration"/>s) in this container are combined using
        /// SQL operations (UNION / INTERSECT / EXCEPT).
        /// </summary>
        public SetOperation Operation
        {
            get { return _operation; }
            set { SetField(ref  _operation, value); }
        }

        /// <inheritdoc/>
        /// <remarks>Starts out as simply the name of the <see cref="Operation"/> but can be changed by the user e.g. 'EXCEPT - Study Exclusion Criteria
        /// <para>This property should always start with the <see cref="Operation"/> to avoid confusion</para>
        /// </remarks>
        public string Name
        {
            get { return _name; }
            set { SetField(ref  _name, value); }
        }

        /// <summary>
        /// The order within the parent <see cref="CohortAggregateContainer"/> (if it is not a Root level container / orphan).  Symantically this position is relevant only for
        /// the <see cref="SetOperation.EXCEPT"/> which takes the first set and throws out all subsequent sets.
        /// <remarks>Also affects the order of IncludeCumulativeTotals</remarks>
        /// </summary>
        public int Order
        {
            get { return _order; }
            set { SetField(ref  _order, value); }
        }

        #endregion

        internal CohortAggregateContainer(ICatalogueRepository repository, DbDataReader r)
            : base(repository, r)
        {
            Order = int.Parse(r["Order"].ToString());
            SetOperation op;
            SetOperation.TryParse(r["Operation"].ToString(), out op);
            Operation = op;
            Name = r["Name"].ToString();
        }

        /// <summary>
        /// Creates a new container (which starts out as an oprhan) with the given <see cref="SetOperation"/>.  You should either set a
        ///  <see cref="CohortIdentificationConfiguration.RootCohortAggregateContainer_ID"/> to this.<see cref="ID"/> to make this container the root container
        /// or use <see cref="AddChild(CohortAggregateContainer)"/>  on another container to make this a subcontainer of it.
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="operation"></param>
        public CohortAggregateContainer(ICatalogueRepository repository, SetOperation operation)
        {
            repository.InsertAndHydrate(this,new Dictionary<string, object>
            {
                {"Operation", operation.ToString()},
                {"Order", 0},
                {"Name", operation.ToString()}
            });
        }

        /// <summary>
        /// Gets all the subcontainers of the current container (if any)
        /// <para>You might want to instead use <seealso cref="GetOrderedContents"/></para>
        /// </summary>
        /// <returns></returns>
        public CohortAggregateContainer[] GetSubContainers()
        {
            return Repository.SelectAllWhere<CohortAggregateContainer>("SELECT CohortAggregateContainer_ChildID FROM CohortAggregateSubContainer WHERE CohortAggregateContainer_ParentID=@CohortAggregateContainer_ParentID", 
                "CohortAggregateContainer_ChildID",
                new Dictionary<string, object>
                {
                    {"CohortAggregateContainer_ParentID", ID}
                }).ToArray();
        }

        /// <summary>
        /// Gets the parent container of the current container (if it is not a root / orphan container)
        /// </summary>
        /// <returns></returns>
        public CohortAggregateContainer GetParentContainerIfAny()
        {
            return Repository.SelectAllWhere<CohortAggregateContainer>("SELECT CohortAggregateContainer_ParentID FROM CohortAggregateSubContainer WHERE CohortAggregateContainer_ChildID=@CohortAggregateContainer_ChildID",
                "CohortAggregateContainer_ParentID",
                new Dictionary<string, object>
                {
                    {"CohortAggregateContainer_ChildID", ID}
                }).SingleOrDefault();
        }

        /// <summary>
        /// Returns all the cohort identifier set queries (See <see cref="AggregateConfiguration"/>) declared as immediate children of the container.  These exist in 
        /// order defined by <see cref="IOrderable.Order"/> and can be interspersed with subcontainers (<see cref="GetSubContainers"/>).
        /// <para>You might want to instead use <seealso cref="GetOrderedContents"/></para>
        /// </summary>
        /// <returns></returns>
        public AggregateConfiguration[] GetAggregateConfigurations()
        {
            return Repository.SelectAll<AggregateConfiguration>("SELECT AggregateConfiguration_ID FROM CohortAggregateContainer_AggregateConfiguration where CohortAggregateContainer_ID=" + ID).OrderBy(config=>config.Order).ToArray();
        }

        /// <summary>
        /// Makes the configuration a member of this container.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="order"></param>
        public void AddChild(AggregateConfiguration configuration, int order)
        {
            Repository.Insert(
                "INSERT INTO CohortAggregateContainer_AggregateConfiguration (CohortAggregateContainer_ID, AggregateConfiguration_ID, [Order]) VALUES (@CohortAggregateContainer_ID, @AggregateConfiguration_ID, @Order)",
                new Dictionary<string, object>
                {
                    {"CohortAggregateContainer_ID", ID},
                    {"AggregateConfiguration_ID", configuration.ID},
                    {"Order", order}
                });

            configuration.ReFetchOrder();
        }


        /// <summary>
        /// Removes the given <see cref="AggregateConfiguration"/> from this container if it is an immediate child.
        /// <para>Has no effect if if the <see cref="AggregateConfiguration"/> is not an immediate child</para>
        /// </summary>
        /// <param name="configuration"></param>
        public void RemoveChild(AggregateConfiguration configuration)
        {
            Repository.Delete("DELETE FROM CohortAggregateContainer_AggregateConfiguration WHERE CohortAggregateContainer_ID = @CohortAggregateContainer_ID AND AggregateConfiguration_ID = @AggregateConfiguration_ID", new Dictionary<string, object>
            {
                {"CohortAggregateContainer_ID", ID},
                {"AggregateConfiguration_ID", configuration.ID}
            });
        }


        /// <summary>
        /// If the configuration is part of any aggregate container anywhere this method will set the order to the newOrder int
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static void SetOrderIfExistsFor(AggregateConfiguration configuration, int newOrder)
        {
            ((CatalogueRepository)configuration.Repository).Update("UPDATE CohortAggregateContainer_AggregateConfiguration SET [Order] = " + newOrder + " WHERE AggregateConfiguration_ID = @AggregateConfiguration_ID", new Dictionary<string, object>
            {
                {"AggregateConfiguration_ID", configuration.ID}
            });
        }

        /// <summary>
        /// Deletes all relationships in which this has a parent - kills all containers parents
        /// </summary>
        public void MakeIntoAnOrphan()
        {
            Repository.Delete("DELETE FROM CohortAggregateSubContainer WHERE CohortAggregateContainer_ChildID = @CohortAggregateContainer_ChildID", new Dictionary<string, object>
            {
                {"CohortAggregateContainer_ChildID", ID}
            });
        }

        /// <summary>
        /// Makes the other <see cref="CohortAggregateContainer"/> into a subcontainer of this container
        /// </summary>
        /// <param name="child"></param>
        public void AddChild(CohortAggregateContainer child)
        {
            Repository.Insert("INSERT INTO CohortAggregateSubContainer(CohortAggregateContainer_ParentID,CohortAggregateContainer_ChildID) VALUES (@CohortAggregateContainer_ParentID, @CohortAggregateContainer_ChildID)", new Dictionary<string, object>
            {
                {"CohortAggregateContainer_ParentID", ID},
                {"CohortAggregateContainer_ChildID", child.ID}
            });
        }

        /// <inheritdoc/>
        /// <remarks>Also deletes subcontainers to avoid leaving orphans in the database</remarks>
        public override void DeleteInDatabase()
        {
            var children = GetSubContainers();

            //delete the children
            foreach (var subContainer in children)
                subContainer.DeleteInDatabase();

            //now delete this
            base.DeleteInDatabase();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Returns true if this.Children contains the thing you are looking for - IMPORTANT: also returns true if we are the thing you are looking for
        /// </summary>
        /// <param name="potentialChild"></param>
        /// <returns></returns>
        public bool HasChild(CohortAggregateContainer potentialChild)
        {
            bool foundChildThroughRecursion = false;

            //recurse into all children
            foreach(var c in GetSubContainers())
                if (c.HasChild(potentialChild))//ask children recursively the same question (see return statement for the question we are asking)
                    foundChildThroughRecursion = true;
            
            //are we the one you are looking for or were any of our children
            return potentialChild.ID == this.ID || foundChildThroughRecursion;
        }

        /// <summary>
        /// Returns true if the supplied <seealso cref="AggregateConfiguration"/> is a child of this container or any of it's subcontainers (recursively)
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public bool HasChild(AggregateConfiguration configuration)
        {
            bool foundChildThroughRecursion = false;

            //recurse into all children
            foreach (var c in GetSubContainers())
                if (c.HasChild(configuration))//ask children recursively the same question (see return statement for the question we are asking)
                    foundChildThroughRecursion = true;

            //are any of the configurations in this bucket the one you are looking for
            return
            GetAggregateConfigurations().Any(c => c.ID == configuration.ID)//yes
            || foundChildThroughRecursion;//no but a child had it
        }

        /// <summary>
        /// Returns all subcontainers and identifier sets (<see cref="AggregateConfiguration"/>) of this container in order (See <see cref="Order"/>)
        /// </summary>
        /// <returns></returns>
        public IOrderedEnumerable<IOrderable> GetOrderedContents()
        {
            List<IOrderable> toReturn = new List<IOrderable>();
            toReturn.AddRange(GetSubContainers());
            toReturn.AddRange(GetAggregateConfigurations());

            return toReturn.OrderBy(o => o.Order);
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
        public CohortAggregateContainer CloneEntireTreeRecursively(ICheckNotifier notifier, CohortIdentificationConfiguration original, CohortIdentificationConfiguration clone, Dictionary<JoinableCohortAggregateConfiguration, JoinableCohortAggregateConfiguration> parentToCloneJoinablesDictionary)
        {
            //what is in us?
            var contents = GetOrderedContents();

            //clone us with same order (in parents)
            var cloneContainer = new CohortAggregateContainer((ICatalogueRepository)Repository, Operation);
            cloneContainer.Order = Order;
            cloneContainer.SaveToDatabase();


            //for each thing in us
            foreach (IOrderable content in contents)
            {
                int order = content.Order;
                var config = content as AggregateConfiguration;
                var container = content as CohortAggregateContainer;

                //its a config, clone the config and add it to the clone container
                if(config != null)
                {
                    var configClone = clone.ImportAggregateConfigurationAsIdentifierList(config, null,false);
                    notifier.OnCheckPerformed(new CheckEventArgs("Created clone dataset " + configClone + " with ID " + configClone.ID,CheckResult.Success));
                    cloneContainer.AddChild(configClone,order);

                    //if the original used any joinable patient index tables
                    var usedJoins = config.PatientIndexJoinablesUsed;

                    //our dictionary should have a record of it along with a clone patient index table we should hook our clone up to
                    foreach (var j in usedJoins)
                    {
                        //for some reason the CohortIdentificationConfiguration didn't properly clone the joinable permission or didn't add it to the dictionary
                        if(!parentToCloneJoinablesDictionary.ContainsKey(j.JoinableCohortAggregateConfiguration))
                            throw new KeyNotFoundException("Configuration " + configClone + " uses Patient Index Table " + j.AggregateConfiguration + " but our dictionary did not have the key, why was that joinable not cloned?");

                        //we do have a clone copy of the joinable permission, set the clone aggregate
                        var cloneJoinable = parentToCloneJoinablesDictionary[j.JoinableCohortAggregateConfiguration];
                        var cloneJoinUse = cloneJoinable.AddUser(configClone);

                        cloneJoinUse.JoinType = j.JoinType;
                        cloneJoinUse.SaveToDatabase();

                        //Now! (brace yourself).  Some the filters in the AggregateConfiguration we just cloned might reference a table called ix2934 or whetever, this
                        //is the Joinable we need to do a replace to point them at the correct ix number (although if they are good users they will have aliased any 
                        //patient index columns anyway)
                        if (configClone.RootFilterContainer_ID != null)
                        {
                            foreach (var clonedFilter in SqlQueryBuilderHelper.GetAllFiltersUsedInContainerTreeRecursively(configClone.RootFilterContainer))
                            {
                                var oldTableAlias = j.GetJoinTableAlias();
                                var newTableAlias = cloneJoinUse.GetJoinTableAlias();

                                clonedFilter.WhereSQL = clonedFilter.WhereSQL.Replace(oldTableAlias, newTableAlias);
                                clonedFilter.SaveToDatabase();
                            }
                        }
                        
                    }
                }

                //its another container (a subcontainer), recursively call the clone operation on it and add that subtree to teh clone container
                if (container != null)
                {
                    var cloneSubContainer = container.CloneEntireTreeRecursively(notifier, original, clone,parentToCloneJoinablesDictionary);

                    notifier.OnCheckPerformed(new CheckEventArgs("Created clone container " + cloneSubContainer + " with ID " + cloneSubContainer.ID, CheckResult.Success));
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

                //it is not a root container, either the container is an orphan (very bad) or it's parent is a root container (or it's parent and so on)
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
            foreach (var orderable in GetOrderedContents().ToArray())
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
            List<CohortAggregateContainer> toReturn = new List<CohortAggregateContainer>();

            var subs = GetSubContainers();
            toReturn.AddRange(subs);

            foreach (CohortAggregateContainer sub in subs)
                toReturn.AddRange(sub.GetAllSubContainersRecursively());
            
            return toReturn;
        }
    }
}
