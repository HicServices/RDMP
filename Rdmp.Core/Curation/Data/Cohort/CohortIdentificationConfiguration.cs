// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using NPOI.OpenXmlFormats.Spreadsheet;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort.Joinables;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Curation.FilterImporting;
using Rdmp.Core.Curation.FilterImporting.Construction;
using Rdmp.Core.Databases;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Annotations;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.Ticketing;

namespace Rdmp.Core.Curation.Data.Cohort;

/// <summary>
/// Cohort identification is the job identifying which patients fit certain study criteria.  E.g. "I want all patients who have been prescribed Diazepam for the first time after 2000
/// and who are still alive today".  Every time the data analyst has a new project/cohort to identify he should create a new CohortIdentificationConfiguration, it is the entry point
/// for cohort generation and includes a high level description of what the cohort requirements are, an optional ticket and is the hanging off point for all the
/// RootCohortAggregateContainers (the bit that provides the actual filtering/technical data about how the cohort is identified).
/// </summary>
public class CohortIdentificationConfiguration : DatabaseEntity, ICollectSqlParameters, INamed, IHasDependencies,
    ICustomSearchString, IMightBeReadOnly, IHasFolder
{
    /// <summary>
    /// Characters that apear in front of any <see cref="AggregateConfiguration"/> which is acting as a cohort identification list or patient index table
    /// <seealso cref="AggregateConfiguration.IsCohortIdentificationAggregate"/>.
    /// </summary>
    public const string CICPrefix = "cic_";


    private int? _version;

    public int? Version
    {
        get => _version;
        set => _version = value;
    }




    #region Database Properties

    private string _name;
    private string _description;
    private string _ticket;
    private int? _rootCohortAggregateContainerID;
    private int? _queryCachingServerID;
    private bool _frozen;
    private string _frozenBy;
    private DateTime? _frozenDate;
    private int? _clonedFrom_ID;
    private string _folder;

    /// <inheritdoc/>
    [Unique]
    [NotNull]
    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    /// <summary>
    /// User typed description of the cohort identification criteria in high level terms.  This should be a primer for looking at the contents (cohort sets, set operations
    /// etc).
    /// </summary>
    public string Description
    {
        get => _description;
        set => SetField(ref _description, value);
    }

    /// <summary>
    /// Name of a ticket in your company issue tracking application (if you have one) for logging time/issues with this <see cref="CohortIdentificationConfiguration"/>.
    /// Ties in with <see cref="ITicketingSystem"/> if a compatible plugin is installed.
    /// </summary>
    public string Ticket
    {
        get => _ticket;
        set => SetField(ref _ticket, value);
    }

    /// <summary>
    /// All <see cref="CohortIdentificationConfiguration"/> must have a single unique root <see cref="CohortAggregateContainer"/> in order to be run.  This is the ID of that
    /// container.
    /// <para>You should not share containers/cohort sets with any other <see cref="CohortIdentificationConfiguration"/></para>
    /// </summary>
    public int? RootCohortAggregateContainer_ID
    {
        get => _rootCohortAggregateContainerID;
        set => SetField(ref _rootCohortAggregateContainerID, value);
    }

    /// <summary>
    /// To assist with complex cohort identification queries over multiple datasets (and between servers / server types) you can configure a QueryCachingServer.
    /// This is an <see cref="ExternalDatabaseServer"/> created by <see cref="QueryCachingPatcher"/>.
    /// 
    /// <para>Once setup, each <see cref="AggregateConfiguration"/> query in this <see cref="CohortIdentificationConfiguration"/> will be run independently and
    /// the resulting patient list committed ot the cache server (See QueryCaching.Aggregation.CachedAggregateConfigurationResultsManager).</para>
    /// 
    /// <para>This field holds the ID of the currently configured database (if any) which acts as a result cache</para>
    /// </summary>
    public int? QueryCachingServer_ID
    {
        get => _queryCachingServerID;
        set => SetField(ref _queryCachingServerID, value);
    }

    /// <summary>
    /// Indicates whether the <see cref="CohortIdentificationConfiguration"/> should be considered immutable.  Usually because it has been run and the results committed to
    /// a Project.
    /// <para>IMPORTANT:You should use <see cref="Freeze()"/> rather than just setting this manually so as to also populate <see cref="FrozenBy"/> and <see cref="FrozenDate"/></para>
    /// </summary>
    public bool Frozen
    {
        get => _frozen;
        set => SetField(ref _frozen, value);
    }

    /// <summary>
    /// Username of the user who ran <see cref="Freeze()"/>
    /// <seealso cref="Frozen"/>
    /// </summary>
    public string FrozenBy
    {
        get => _frozenBy;
        set => SetField(ref _frozenBy, value);
    }

    /// <summary>
    /// The date <see cref="Freeze()"/> was last ran
    /// <seealso cref="Frozen"/>
    /// </summary>
    public DateTime? FrozenDate
    {
        get => _frozenDate;
        set => SetField(ref _frozenDate, value);
    }

    /// <summary>
    /// The ID of the CohortIdentificationConfiguration that this one was cloned from (if any).
    /// 
    /// <para>It is possible to delete the parent, resulting in a ClonedFrom_ID which cannot be resolved to a parent object</para>
    /// <seealso cref="Frozen"/>
    /// </summary>
    public int? ClonedFrom_ID
    {
        get => _clonedFrom_ID;
        set => SetField(ref _clonedFrom_ID, value);
    }

    /// <inheritdoc/>
    [UsefulProperty]
    public string Folder
    {
        get => _folder;
        set => SetField(ref _folder, FolderHelper.Adjust(value));
    }

    #endregion

    #region Relationships

    /// <inheritdoc cref="RootCohortAggregateContainer_ID"/>
    [NoMappingToDatabase]
    public CohortAggregateContainer RootCohortAggregateContainer =>
        RootCohortAggregateContainer_ID == null
            ? null
            : Repository.GetObjectByID<CohortAggregateContainer>((int)RootCohortAggregateContainer_ID);

    /// <inheritdoc cref="QueryCachingServer_ID"/>
    [NoMappingToDatabase]
    public ExternalDatabaseServer QueryCachingServer =>
        QueryCachingServer_ID == null
            ? null
            : Repository.GetObjectByID<ExternalDatabaseServer>(QueryCachingServer_ID.Value);

    #endregion


    public List<CohortConfigurationVersion> GetVersions()
    {
        return CatalogueRepository.GetAllObjectsWhere<CohortConfigurationVersion>("CohortIdentificationConfigurationId", this.ID).ToList();
    }

    public CohortIdentificationConfiguration()
    {
    }

    /// <summary>
    /// Declares a new configuration for identifying patient lists matching a study requirements based on the results of cohort sets / patient index tables and set operations
    /// <see cref="CohortIdentificationConfiguration"/>
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="name"></param>
    public CohortIdentificationConfiguration(ICatalogueRepository repository, string name)
    {
        var queryCache = repository.GetDefaultFor(PermissableDefaults.CohortIdentificationQueryCachingServer_ID);

        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "Name", name },
            { "QueryCachingServer_ID", queryCache?.ID ?? (object)DBNull.Value },
            { "Folder", FolderHelper.Root }
        });
    }

    internal CohortIdentificationConfiguration(ICatalogueRepository repository, DbDataReader r)
        : base(repository, r)
    {
        Name = r["Name"].ToString();
        Description = r["Description"] as string;
        RootCohortAggregateContainer_ID = ObjectToNullableInt(r["RootCohortAggregateContainer_ID"]);
        Ticket = r["Ticket"] as string;
        QueryCachingServer_ID = ObjectToNullableInt(r["QueryCachingServer_ID"]);

        Frozen = Convert.ToBoolean(r["Frozen"]);
        FrozenBy = r["FrozenBy"] as string;
        FrozenDate = ObjectToNullableDateTime(r["FrozenDate"]);
        ClonedFrom_ID = ObjectToNullableInt(r["ClonedFrom_ID"]);
        Version = ObjectToNullableInt(r["Version"]);
        Folder = r["Folder"] as string ?? FolderHelper.Root;
    }

    /// <summary>
    /// Deletes the root container and consequently the entire <see cref="CohortIdentificationConfiguration"/>
    /// </summary>
    public override void DeleteInDatabase()
    {
        //container is the parent class even though this is a 1 to 1 and there is a CASCADE which will actually nuke ourselves when we delete the root container!
        if (RootCohortAggregateContainer_ID != null)
            RootCohortAggregateContainer.DeleteInDatabase();

        //shouldn't ever happen but double check anyway in case somebody removes the CASCADE
        if (Exists())
            base.DeleteInDatabase();
        else
            //make sure to do the obscure cross server/database cascade activities too
            //if the repository has obscure dependencies
            CatalogueRepository.ObscureDependencyFinder?.HandleCascadeDeletesForDeletedObject(this);
    }

    /// <summary>
    /// Creates a new <see cref="CohortAggregateContainer"/> if there is no <see cref="RootCohortAggregateContainer_ID"/> yet
    /// </summary>
    public void CreateRootContainerIfNotExists()
    {
        //Only proceed if it doesn't have one already
        if (RootCohortAggregateContainer_ID != null) return;
        //create a new one and record its ID
        RootCohortAggregateContainer_ID =
            new CohortAggregateContainer((ICatalogueRepository)Repository, SetOperation.UNION).ID;

        //save us to database to cement the object
        SaveToDatabase();
    }

    /// <inheritdoc/>
    public override string ToString() => Name;

    public bool ShouldBeReadOnly(out string reason)
    {
        if (Frozen)
        {
            reason = $"{Name} is Frozen";
            return true;
        }

        reason = null;
        return false;
    }

    /// <inheritdoc/>
    public string GetSearchString() =>
        //let the cic acronym match cohort identification configuration
        $"cic {Name}";

    /// <inheritdoc/>
    public ISqlParameter[] GetAllParameters() => CatalogueRepository.GetAllParametersForParentTable(this).ToArray();


    /// <inheritdoc cref="CICPrefix"/>
    public string GetNamingConventionPrefixForConfigurations() => $"{CICPrefix}{ID}_";

    /// <summary>
    /// Returns true if the <see cref="AggregateConfiguration"/> provided has Name compatible with <see cref="CICPrefix"/>
    /// <seealso cref="GetNamingConventionPrefixForConfigurations"/>
    /// </summary>
    /// <param name="aggregate"></param>
    /// <returns></returns>
    public bool IsValidNamedConfiguration(AggregateConfiguration aggregate) =>
        aggregate.Name.StartsWith(GetNamingConventionPrefixForConfigurations());

    /// <summary>
    /// All <see cref="AggregateConfiguration"/>s within a <see cref="CohortIdentificationConfiguration"/> must start with the appropriate prefix (and ID of the cic)
    /// (See <see cref="CICPrefix"/>).  This method will change the <see cref="AggregateConfiguration.Name"/> to match the expected prefix.
    /// <para>If the name change would result in a collisionw ith an existing set in the configuration then (Copy X) will appear at the end of the name</para>
    /// </summary>
    /// <param name="aggregate"></param>
    public void EnsureNamingConvention(AggregateConfiguration aggregate)
    {
        //it is already valid
        if (IsValidNamedConfiguration(aggregate))
            return;

        //make it valid by sticking on the prefix
        aggregate.Name = $"{GetNamingConventionPrefixForConfigurations()}{aggregate.Name}";

        var copy = 0;
        var origName = aggregate.Name;


        var otherConfigurations =
            Repository.GetAllObjects<AggregateConfiguration>().Except(new[] { aggregate }).ToArray();

        //if there is a conflict on the name
        if (otherConfigurations.Any(c => c.Name.Equals(origName)))
            do
            {
                //add Copy 1 then Copy 2 etc
                copy++;
                aggregate.Name = $"{origName} (Copy {copy})";
            } while (otherConfigurations.Any(c => c.Name.Equals(aggregate.Name))); //until there are no more copies

        aggregate.SaveToDatabase();
    }

    /// <summary>
    /// Creates a entirely new copy of the <see cref="CohortIdentificationConfiguration"/> with all new IDs on the root and all child objects.  This includes
    /// filters, patient index tables, parameters, set containers etc.
    /// <para>This is done in a transaction so that if it fails halfway through you won't end up with half a clone configuration</para>
    /// </summary>
    /// <param name="notifier">Event listener for reporting cloning progress and any problems</param>
    /// <returns></returns>
    public CohortIdentificationConfiguration CreateClone(ICheckNotifier notifier)
    {
        //todo this would be nice if it was ICatalogueRepository but transaction is super SQLy
        var cataRepo = (ICatalogueRepository)Repository;
        //start a new super transaction
        using (cataRepo.BeginNewTransaction())
        {
            try
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Super Transaction started on Catalogue Repository",
                    CheckResult.Success));

                var clone = new CohortIdentificationConfiguration(cataRepo, $"{Name} (Clone)");

                notifier.OnCheckPerformed(new CheckEventArgs(
                    $"Created clone configuration '{clone.Name}' with ID {clone.ID} called {clone}",
                    CheckResult.Success));

                //clone the global parameters
                foreach (var p in GetAllParameters())
                {
                    notifier.OnCheckPerformed(new CheckEventArgs($"Cloning global parameter {p.ParameterName}",
                        CheckResult.Success));
                    var cloneP = new AnyTableSqlParameter(cataRepo, clone, p.ParameterSQL)
                    {
                        Comment = p.Comment,
                        Value = p.Value
                    };
                    cloneP.SaveToDatabase();
                }

                //key is the original, value is the clone
                var parentToCloneJoinablesDictionary = GetAllJoinables().ToDictionary(joinable => joinable,
                    joinable => new JoinableCohortAggregateConfiguration(cataRepo, clone,
                        joinable.AggregateConfiguration.CreateClone()));

                clone.ClonedFrom_ID = ID;
                clone.RootCohortAggregateContainer_ID = RootCohortAggregateContainer
                    .CloneEntireTreeRecursively(notifier, this, clone, parentToCloneJoinablesDictionary).ID;
                clone.SaveToDatabase();

                notifier.OnCheckPerformed(
                    new CheckEventArgs("Clone creation successful, about to commit Super Transaction",
                        CheckResult.Success));
                cataRepo.EndTransaction(true);
                notifier.OnCheckPerformed(new CheckEventArgs("Super Transaction committed successfully",
                    CheckResult.Success));

                return clone;
            }
            catch (Exception e)
            {
                cataRepo.EndTransaction(false);
                notifier.OnCheckPerformed(new CheckEventArgs(
                    "Cloning failed, See Exception for details, the Super Transaction was rolled back successfully though",
                    CheckResult.Fail, e));
            }
        }

        return null;
    }

    /// <summary>
    /// Creates an adjusted copy of the <paramref name="toClone"/> to be used as a cohort identification <see cref="AggregateConfiguration"/>.  This could be
    /// an <see cref="AggregateConfiguration"/> graph or one that is acting as a patient index table / cohort set for another <see cref="CohortIdentificationConfiguration"/>.
    /// <para>IMPORTANT: It must be possible to select a single column from which to harvest the patient identifiers from <paramref name="resolveMultipleExtractionIdentifiers"/></para>
    /// </summary>
    /// <param name="toClone">The aggregate to import</param>
    /// <param name="resolveMultipleExtractionIdentifiers">What to do if there are multiple <see cref="ExtractionInformation"/>/<see cref="AggregateDimension"/>
    ///  marked IsExtractionIdentifier</param>
    /// <param name="useTransaction">True to run the import in a transaction</param>
    /// <returns></returns>
    public AggregateConfiguration ImportAggregateConfigurationAsIdentifierList(AggregateConfiguration toClone,
        ChooseWhichExtractionIdentifierToUseFromManyHandler resolveMultipleExtractionIdentifiers,
        bool useTransaction = true)
    {
        if (!useTransaction)
            return CreateCloneOfAggregateConfigurationPrivate(toClone, resolveMultipleExtractionIdentifiers);

        if (Repository is ITableRepository tableRepo)
            using (tableRepo.BeginNewTransactedConnection())
            {
                try
                {
                    var toReturn =
                        CreateCloneOfAggregateConfigurationPrivate(toClone, resolveMultipleExtractionIdentifiers);
                    tableRepo.EndTransactedConnection(true);
                    return toReturn;
                }
                catch (Exception)
                {
                    tableRepo.EndTransactedConnection(false); //abandon
                    throw;
                }
            }

        return CreateCloneOfAggregateConfigurationPrivate(toClone, resolveMultipleExtractionIdentifiers);
    }

    private AggregateConfiguration CreateCloneOfAggregateConfigurationPrivate(AggregateConfiguration toClone,
        ChooseWhichExtractionIdentifierToUseFromManyHandler resolveMultipleExtractionIdentifiers)
    {
        var cataRepo = CatalogueRepository;

        //clone will not have axis or pivot or dimensions other than extraction identifier
        var newConfiguration = toClone.ShallowClone();

        //make its name follow the naming convention e.g. cic_105_LINK103_MyAggregate
        EnsureNamingConvention(newConfiguration);

        //now clear its pivot dimension, make it not extractable and make its countSQL basic/sane
        newConfiguration.PivotOnDimensionID = null;
        newConfiguration.IsExtractable = false;
        newConfiguration.CountSQL = null; //clear the count sql
        newConfiguration.Description = toClone.Description;

        //clone parameters
        foreach (var toCloneParameter in toClone.Parameters)
            new AnyTableSqlParameter((ICatalogueRepository)newConfiguration.Repository, newConfiguration,
                toCloneParameter.ParameterSQL)
            {
                Value = toCloneParameter.Value,
                Comment = toCloneParameter.Comment
            }.SaveToDatabase();


        //now clone its AggregateForcedJoins
        foreach (var t in cataRepo.AggregateForcedJoinManager.GetAllForcedJoinsFor(toClone))
            cataRepo.AggregateForcedJoinManager.CreateLinkBetween(newConfiguration, t);

        if (!toClone.Catalogue.IsApiCall())
        {
            //two cases here either the import has a custom freaky CHI column (dimension) or it doesn't reference CHI at all if it is freaky we want to preserve its freakyness
            var extractionIdentifier = GetExtractionIdentifierFrom(toClone, out var underlyingExtractionInformation,
                resolveMultipleExtractionIdentifiers);

            //now give it 1 dimension which is the only IsExtractionIdentifier column
            var newDimension = new AggregateDimension(cataRepo, underlyingExtractionInformation, newConfiguration);

            //the thing we were cloning had a freaky CHI column (probably had a collate or something involved in it or a masterchi)
            if (extractionIdentifier is AggregateDimension)
            {
                //preserve its freakyness
                newDimension.Alias = extractionIdentifier.Alias;
                newDimension.SelectSQL = extractionIdentifier.SelectSQL;
                newDimension.Order = extractionIdentifier.Order;
                newDimension.SaveToDatabase();
            }
        }

        //now rewire all its filters
        if (toClone.RootFilterContainer_ID != null) //if it has any filters
        {
            //get the tree
            var oldRootContainer = toClone.RootFilterContainer;

            //clone the tree
            var newRootContainer = oldRootContainer.DeepCloneEntireTreeRecursivelyIncludingFilters();
            newConfiguration.RootFilterContainer_ID = newRootContainer.ID;
        }

        newConfiguration.SaveToDatabase();

        return newConfiguration;
    }

    /// <summary>
    /// Delegate for handling the situation in which the user wants to create a cohort based on a given Catalogue but there are multiple IsExtractionIdentifier columns.
    /// For example SMR02 (baby birth records) might have (Mother CHI, Father CHI, Baby CHI).  In this situation the descision on which column to use is resolved by this
    /// class.
    /// </summary>
    /// <param name="catalogue"></param>
    /// <param name="candidates"></param>
    /// <returns></returns>
    public delegate ExtractionInformation ChooseWhichExtractionIdentifierToUseFromManyHandler(ICatalogue catalogue,
        ExtractionInformation[] candidates);


    /// <summary>
    /// Creates a new cohort set <see cref="AggregateConfiguration"/> which initially matches any patient appearing in the dataset (<see cref="Catalogue"/>).
    /// <para>IMPORTANT: It must be possible to select a single column from which to harvest the patient identifiers from <paramref name="resolveMultipleExtractionIdentifiers"/></para>
    /// </summary>
    /// <param name="catalogue">The catalogue to import as a patient identification set (you can import the same Catalogue multiple times e.g.
    /// 'People ever prescribed morphine' EXCEPT 'People ever prescribed percoset'</param>
    /// <param name="resolveMultipleExtractionIdentifiers">What to do if there are multiple <see cref="ExtractionInformation"/>
    ///  marked IsExtractionIdentifier</param>
    /// <param name="importMandatoryFilters"></param>
    /// <returns></returns>
    public AggregateConfiguration CreateNewEmptyConfigurationForCatalogue(ICatalogue catalogue,
        ChooseWhichExtractionIdentifierToUseFromManyHandler resolveMultipleExtractionIdentifiers,
        bool importMandatoryFilters = true)
    {
        var cataRepo = (ICatalogueRepository)Repository;

        var configuration = new AggregateConfiguration(cataRepo, catalogue, $"People in {catalogue}");
        EnsureNamingConvention(configuration);

        if (!catalogue.IsApiCall())
        {
            var extractionIdentifier =
                (ExtractionInformation)GetExtractionIdentifierFrom(catalogue, resolveMultipleExtractionIdentifiers);
            //make the extraction identifier column into the sole dimension on the new configuration
            _ = new AggregateDimension(cataRepo, extractionIdentifier, configuration);
        }

        //no count sql
        configuration.CountSQL = null;
        configuration.SaveToDatabase();

        if (importMandatoryFilters)
            ImportMandatoryFilters(catalogue, configuration, GetAllParameters());

        return configuration;
    }

    private void ImportMandatoryFilters(ICatalogue catalogue, AggregateConfiguration configuration,
        ISqlParameter[] globalParameters)
    {
        var filterImporter = new FilterImporter(new AggregateFilterFactory((ICatalogueRepository)catalogue.Repository),
            globalParameters);

        //Find any Mandatory Filters
        var mandatoryFilters = catalogue.GetAllMandatoryFilters();

        var createdSoFar = new List<AggregateFilter>();

        if (!mandatoryFilters.Any()) return;
        var container = new AggregateFilterContainer((ICatalogueRepository)Repository, FilterContainerOperation.AND);
        configuration.RootFilterContainer_ID = container.ID;
        configuration.SaveToDatabase();

        foreach (var filter in mandatoryFilters)
        {
            var newFilter = (AggregateFilter)filterImporter.ImportFilter(container, filter, createdSoFar.ToArray());

            container.AddChild(newFilter);
            createdSoFar.Add(newFilter);
        }
    }

    /// <summary>
    /// returns an underlying ExtractionInformation which IsExtractionIdentifier (e.g. a chi column).  The first pass approach is to look for a suitable AggregateDimension which
    /// has an underlying  ExtractionInformation which IsExtractionIdentifier but if it doesn't find one then it will look in the Catalogue for one instead.  The reason this is
    /// complex is because you can have datasets with multiple IsExtractionIdentifier columns e.g. SMR02 (birth records) where there is both MotherCHI and BabyCHI which are both
    /// IsExtractionIdentifier - in this case the AggregateConfiguration would need a Dimension of one or the other (but not both!) - if it had neither then the method would throw
    /// when it checked Catalogue and found both.
    /// </summary>
    /// <param name="toClone"></param>
    /// <param name="underlyingExtractionInformation"></param>
    /// <param name="resolveMultipleExtractionIdentifiers"></param>
    /// <returns></returns>
    private static IColumn GetExtractionIdentifierFrom(AggregateConfiguration toClone,
        out ExtractionInformation underlyingExtractionInformation,
        ChooseWhichExtractionIdentifierToUseFromManyHandler resolveMultipleExtractionIdentifiers)
    {
        //make sure it can be cloned before starting

        //Aggregates have AggregateDimensions which are a subset of the Catalogues ExtractionInformations.  Most likely there won't be an AggregateDimension which IsExtractionIdentifier
        //because why would the user have graphs of chi numbers?

        var existingExtractionIdentifierDimensions =
            toClone.AggregateDimensions.Where(d => d.IsExtractionIdentifier).ToArray();
        IColumn extractionIdentifier = null;

        //very unexpected, he had multiple IsExtractionIdentifier Dimensions all configured for simultaneous use on this Aggregate, it is very freaky and we definetly don't want to let him import it for cohort identification
        if (existingExtractionIdentifierDimensions.Length > 1)
            throw new NotSupportedException(
                $"Cannot clone the AggregateConfiguration {toClone} because it has multiple IsExtractionIdentifier dimensions (must be 0 or 1)");

        //It has 1... maybe it's already being used in cohort identification from another project or he just likes to graph chi numbers for some reason
        if (existingExtractionIdentifierDimensions.Length == 1)
        {
            extractionIdentifier = existingExtractionIdentifierDimensions[0];
            underlyingExtractionInformation = existingExtractionIdentifierDimensions[0].ExtractionInformation;
        }
        else
        {
            //As expected the user isn't graphing chis or otherwise configured any Dimensions on this aggregate.  It is probably just a normal aggregate e.g. 'Prescriptions of Morphine over time between 2005-2001' where the only dimension is 'date prescribed'
            //now when we import this for cohort identification we want to keep the cool filters and stuff but we can junk all the dimensions.  BUT we do need to add the IsExtractionIdentifier column from the dataset (and if there are for some reason multiple
            //e.g. babychi, motherchi then we can invoke the delegate to let the user pick one.

            //get the unique IsExtractionIdentifier from the cloned configurations parent catalogue
            underlyingExtractionInformation =
                (ExtractionInformation)GetExtractionIdentifierFrom(
                    toClone.Catalogue, resolveMultipleExtractionIdentifiers);

            return underlyingExtractionInformation;
        }

        return extractionIdentifier;
    }

    private static IColumn GetExtractionIdentifierFrom(ICatalogue catalogue,
        ChooseWhichExtractionIdentifierToUseFromManyHandler resolveMultipleExtractionIdentifiers)
    {
        //the aggregate they are cloning does not have an extraction identifier but the dataset might still have one
        var catalogueCandidates = catalogue.GetAllExtractionInformation(ExtractionCategory.Any)
            .Where(e => e.IsExtractionIdentifier).ToArray();

        //if there are multiple IsExtractionInformation columns
        if (catalogueCandidates.Length == 1) return catalogueCandidates[0];
        if (resolveMultipleExtractionIdentifiers == null) //no delegate has been provided for resolving this
            throw new NotSupportedException(
                $"Cannot create AggregateConfiguration because the Catalogue {catalogue} has {catalogueCandidates.Length} IsExtractionIdentifier ExtractionInformations");
        //there is a delegate to resolve this, invoke it
        return resolveMultipleExtractionIdentifiers(catalogue, catalogueCandidates) ??
               throw new Exception(
                   "User did not pick a candidate ExtractionInformation column from those we offered");
    }

    /// <summary>
    /// Returns all patient index tables declared in this <see cref="CohortIdentificationConfiguration"/> (See <see cref="JoinableCohortAggregateConfiguration"/>)
    /// </summary>
    /// <returns></returns>
    public JoinableCohortAggregateConfiguration[] GetAllJoinables() =>
        Repository.GetAllObjectsWithParent<JoinableCohortAggregateConfiguration>(this).ToArray();


    /// <summary>
    /// Returns all unique <see cref="TableInfo"/> required for building all of the <see cref="AggregateConfiguration"/>s in the
    /// <see cref="AggregateConfiguration.RootFilterContainer_ID"/> or any subcontainers.
    /// </summary>
    /// <returns></returns>
    public TableInfo[] GetDistinctTableInfos()
    {
        return
            RootCohortAggregateContainer.GetAllAggregateConfigurationsRecursively() //get all the aggregate sets
                .SelectMany(a => a
                    .AggregateDimensions //get all the Dimensions (should really be 1 per aggregate which is the IsExtractionIdentifier column but who are we to check)
                    .Select(d => d.ColumnInfo.TableInfo)) //get the TableInfo of the underlying Column for the dimension
                .Distinct() //return distinct array of them
                .ToArray();
    }

    /// <summary>
    /// Freezes the current <see cref="CohortIdentificationConfiguration"/> marking it as immutable.
    /// <para>This is the preferred way of setting <see cref="Frozen"/></para>
    /// <seealso cref="Frozen"/>
    /// </summary>
    public void Freeze()
    {
        if (Frozen)
            return;

        Frozen = true;
        FrozenBy = Environment.UserName;
        FrozenDate = DateTime.Now;
        SaveToDatabase();
    }

    /// <summary>
    /// Clears the <see cref="Frozen"/> flag fields and saves to database
    /// </summary>
    public void Unfreeze()
    {
        if (!Frozen)
            return;

        Frozen = false;
        FrozenBy = null;
        FrozenDate = null;
        SaveToDatabase();
    }

    /// <inheritdoc/>
    public IHasDependencies[] GetObjectsThisDependsOn()
    {
        var dependencies = new List<IHasDependencies>();

        dependencies.AddRange(GetAllParameters().Cast<AnyTableSqlParameter>());

        if (RootCohortAggregateContainer_ID != null)
            dependencies.AddRange(RootCohortAggregateContainer.GetAllAggregateConfigurationsRecursively());

        return dependencies.ToArray();
    }

    /// <inheritdoc/>
    public IHasDependencies[] GetObjectsDependingOnThis() => Array.Empty<IHasDependencies>();
}