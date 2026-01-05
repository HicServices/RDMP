// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using FAnsi;
using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.CohortCreation.Execution;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Curation.Data.ImportExport;
using Rdmp.Core.Curation.Data.Serialization;
using Rdmp.Core.Logging;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Injection;
using Rdmp.Core.Providers;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Annotations;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Rdmp.Core.Ticketing;

namespace Rdmp.Core.Curation.Data;

/// <inheritdoc cref="ICatalogue"/>
public sealed class Catalogue : DatabaseEntity, IComparable, ICatalogue, IInjectKnown<CatalogueItem[]>,
    IInjectKnown<CatalogueExtractabilityStatus>, IEquatable<Catalogue>
{
    #region Database Properties

    private string _acronym;
    private string _name;
    private string _folder = FolderHelper.Root;
    private string _description;
    private Uri _detailPageUrl;
    private CatalogueType _type;
    private DatasetPurpose _purpose;
    private CataloguePeriodicity _periodicity;
    private CatalogueGranularity _granularity;
    private string _geographicalCoverage;
    private string _backgroundSummary;
    private string _searchKeywords;
    private UpdateFrequencies _updateFreq;
    private string _updateSched;
    private string _timeCoverage;
    private DateTime? _lastRevisionDate;
    private string _contactDetails;
    private string _resourceOwner;
    private string _attributionCitation;
    private string _accessOptions;
    private string _subjectNumbers;
    private Uri _apiAccessUrl;
    private Uri _browseUrl;
    private Uri _bulkDownloadUrl;
    private Uri _queryToolUrl;
    private Uri _sourceUrl;
    private string _countryOfOrigin;


    private string _dataStandards;
    private string _administrativeContactName;
    private string _administrativeContactEmail;
    private string _administrativeContactTelephone;
    private string _administrativeContactAddress;
    private bool? _explicitConsent;
    private string _ethicsApprover;
    private string _sourceOfDataCollection;
    private string _ticket;
    private DateTime? _datasetStartDate;
    private string _loggingDataTask;
    private string _validatorXml;
    private int? _timeCoverageExtractionInformationID;
    private int? _pivotCategoryExtractionInformationID;
    private bool _isDeprecated;
    private bool _isInternalDataset;
    private int? _liveLoggingServerID;

    private string _shortDescription;
    private string _dataType;
    private string _dataSubtype;
    private string _dataSource;
    private string _dataSourceSetting;
    private DateTime? _datasetReleaseDate;
    private DateTime? _startDate;
    private DateTime? _endDate;
    private UpdateLagTimes _updateLag;
    private string _juristiction;
    private string _dataController;
    private string _dataProcessor;
    private string _controlledVocabulary;
    private string _associatedPeople;
    private string _associatedMedia;
    private string _doi;
    private Lazy<CatalogueItem[]> _knownCatalogueItems;


    /// <inheritdoc/>
    [Unique]
    [DoNotImportDescriptions(AllowOverwriteIfBlank = true)]
    public string Acronym
    {
        get => _acronym;
        set => SetField(ref _acronym, value);
    }

    /// <summary>
    /// The full human-readable name of the dataset.  This should usually match the name of the underlying <see cref="TableInfo"/> but might differ
    /// if there are multiple tables powering the Catalogue, osr they don't have user accessible names.
    /// </summary>
    [Unique]
    [NotNull]
    [DoNotImportDescriptions]
    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    /// <inheritdoc/>
    [DoNotImportDescriptions]
    [UsefulProperty]
    public string Folder
    {
        get => _folder;
        set => SetField(ref _folder, FolderHelper.Adjust(value));
    }


    /// <inheritdoc/>
    [UsefulProperty]
    public string Description
    {
        get => _description;
        set => SetField(ref _description, value);
    }

    /// <summary>
    /// User defined Uri for a website page which describes the dataset (probably null)
    /// </summary>
    public Uri Detail_Page_URL
    {
        get => _detailPageUrl;
        set => SetField(ref _detailPageUrl, value);
    }

    /// <summary>
    /// User defined classification of the Type of dataset the Catalogue is e.g. Cohort, ResearchStudy etc
    /// </summary>
    public CatalogueType Type
    {
        get => _type;
        set => SetField(ref _type, value);
    }

    /// <summary>
    /// User defined classification of the Type of dataset the Catalogue is e.g. Cohort, ResearchStudy etc
    /// </summary>
    public DatasetPurpose Purpose
    {
        get => _purpose;
        set => SetField(ref _purpose, value);
    }

    /// <inheritdoc/>
    public CataloguePeriodicity Periodicity
    {
        get => _periodicity;
        set => SetField(ref _periodicity, value);
    }

    /// <summary>
    /// User specified field describing how the dataset is subdivided/bounded e.g. relates to a multiple 'HealthBoards' / 'Clinics' / 'Hosptials' etc.
    /// </summary>
    public CatalogueGranularity Granularity
    {
        get => _granularity;
        set => SetField(ref _granularity, value);
    }

    /// <summary>
    /// User specified free text field.  Not used for anything by RDMP.
    /// </summary>
    public string Geographical_coverage
    {
        get => _geographicalCoverage;
        set => SetField(ref _geographicalCoverage, value);
    }

    /// <summary>
    /// User specified free text field.  Not used for anything by RDMP.
    /// </summary>
    public string Background_summary
    {
        get => _backgroundSummary;
        set => SetField(ref _backgroundSummary, value);
    }

    /// <summary>
    /// User specified list of keywords that are intended to help in finding the Catalogue
    /// </summary>
    public string Search_keywords
    {
        get => _searchKeywords;
        set => SetField(ref _searchKeywords, value);
    }

    /// <summary>
    /// User specified free text field.  Not used for anything by RDMP.
    /// <seealso cref="Periodicity"/>
    /// </summary>
    public UpdateFrequencies Update_freq
    {
        get => _updateFreq;
        set => SetField(ref _updateFreq, value);
    }

    /// <summary>
    /// User specified free text field.  Not used for anything by RDMP.
    /// <seealso cref="Periodicity"/>
    /// </summary>
    public string Update_sched
    {
        get => _updateSched;
        set => SetField(ref _updateSched, value);
    }


    ///<inheritdoc/>
    public string Time_coverage
    {
        get => _timeCoverage;
        set => SetField(ref _timeCoverage, value);
    }

    /// <summary>
    /// User specified date that user allegedly reviewed the contents of the Catalogue / Metadata
    /// </summary>
    public DateTime? Last_revision_date
    {
        get => _lastRevisionDate;
        set => SetField(ref _lastRevisionDate, value);
    }

    /// <summary>
    /// User specified free text field.  Not used for anything by RDMP.
    /// </summary>
    public string Contact_details
    {
        get => _contactDetails;
        set => SetField(ref _contactDetails, value);
    }

    /// <summary>
    /// User specified free text field.  Not used for anything by RDMP.
    /// </summary>
    public string Resource_owner
    {
        get => _resourceOwner;
        set => SetField(ref _resourceOwner, value);
    }

    /// <summary>
    /// User specified free text field.  Not used for anything by RDMP.
    /// </summary>
    public string Attribution_citation
    {
        get => _attributionCitation;
        set => SetField(ref _attributionCitation, value);
    }

    /// <summary>
    /// User specified free text field.  Not used for anything by RDMP.
    /// </summary>
    public string Access_options
    {
        get => _accessOptions;
        set => SetField(ref _accessOptions, value);
    }

    /// <summary>
    /// User specified free text field.  Not used for anything by RDMP.
    /// </summary>
    public string SubjectNumbers
    {
        get => _subjectNumbers;
        set => SetField(ref _subjectNumbers, value);
    }

    /// <summary>
    /// User specified field.  Supposedly a URL for a webservice for accessing the dataset? Not used for anything by RDMP.
    /// </summary>
    public Uri API_access_URL
    {
        get => _apiAccessUrl;
        set => SetField(ref _apiAccessUrl, value);
    }

    /// <summary>
    /// User specified field.  Supposedly a URL for a webservice for browsing the dataset? Not used for anything by RDMP.
    /// </summary>
    public Uri Browse_URL
    {
        get => _browseUrl;
        set => SetField(ref _browseUrl, value);
    }

    /// <summary>
    /// User specified field.  Supposedly a URL for a webservice for bulk downloading the dataset? Not used for anything by RDMP.
    /// </summary>
    public Uri Bulk_Download_URL
    {
        get => _bulkDownloadUrl;
        set => SetField(ref _bulkDownloadUrl, value);
    }

    /// <summary>
    /// User specified field.  Supposedly a URL for a webservice for querying the dataset? Not used for anything by RDMP.
    /// </summary>
    public Uri Query_tool_URL
    {
        get => _queryToolUrl;
        set => SetField(ref _queryToolUrl, value);
    }

    /// <summary>
    /// User specified field.  Supposedly a URL for a website describing where you procured the data from? Not used for anything by RDMP.
    /// </summary>
    public Uri Source_URL
    {
        get => _sourceUrl;
        set => SetField(ref _sourceUrl, value);
    }

    /// <summary>
    /// User specified free text field.  Not used for anything by RDMP.
    /// </summary>
    public string Country_of_origin
    {
        get => _countryOfOrigin;
        set => SetField(ref _countryOfOrigin, value);
    }

    /// <summary>
    /// User specified free text field.  Not used for anything by RDMP.
    /// </summary>
    public string Data_standards
    {
        get => _dataStandards;
        set => SetField(ref _dataStandards, value);
    }

    /// <summary>
    /// User specified free text field.  Not used for anything by RDMP.
    /// </summary>
    public string Administrative_contact_name
    {
        get => _administrativeContactName;
        set => SetField(ref _administrativeContactName, value);
    }

    /// <summary>
    /// User specified free text field.  Not used for anything by RDMP.
    /// </summary>
    public string Administrative_contact_email
    {
        get => _administrativeContactEmail;
        set => SetField(ref _administrativeContactEmail, value);
    }

    /// <summary>
    /// User specified free text field.  Not used for anything by RDMP.
    /// </summary>
    public string Administrative_contact_telephone
    {
        get => _administrativeContactTelephone;
        set => SetField(ref _administrativeContactTelephone, value);
    }

    /// <summary>
    /// User specified free text field.  Not used for anything by RDMP.
    /// </summary>
    public string Administrative_contact_address
    {
        get => _administrativeContactAddress;
        set => SetField(ref _administrativeContactAddress, value);
    }

    /// <summary>
    /// User specified field.  Not used for anything by RDMP.
    /// </summary>
    public bool? Explicit_consent
    {
        get => _explicitConsent;
        set => SetField(ref _explicitConsent, value);
    }

    /// <summary>
    /// User specified free text field.  Not used for anything by RDMP.
    /// </summary>
    public string Ethics_approver
    {
        get => _ethicsApprover;
        set => SetField(ref _ethicsApprover, value);
    }

    /// <summary>
    /// User specified free text field.  Not used for anything by RDMP.
    /// </summary>
    public string Source_of_data_collection
    {
        get => _sourceOfDataCollection;
        set => SetField(ref _sourceOfDataCollection, value);
    }

    /// <summary>
    /// Identifier for a ticket in your <see cref="ITicketingSystem"/> for documenting / auditing work on the Catalogue and for
    /// recording issues (if you are not using the RDMP issue system (see CatalogueItemIssue))
    /// </summary>
    public string Ticket
    {
        get => _ticket;
        set => SetField(ref _ticket, value);
    }

    /// <inheritdoc/>
    [DoNotExtractProperty]
    public string LoggingDataTask
    {
        get => _loggingDataTask;
        set => SetField(ref _loggingDataTask, value);
    }

    /// <inheritdoc/>
    [DoNotExtractProperty]
    public string ValidatorXML
    {
        get => _validatorXml;
        set => SetField(ref _validatorXml, value);
    }

    /// <inheritdoc/>
    [Relationship(typeof(ExtractionInformation), RelationshipType.IgnoreableLocalReference,
        ValueGetter = nameof(GetAllExtractionInformation))] //todo do we want to share this?
    [DoNotExtractProperty]
    public int? TimeCoverage_ExtractionInformation_ID
    {
        get => _timeCoverageExtractionInformationID;
        set => SetField(ref _timeCoverageExtractionInformationID, value);
    }

    /// <inheritdoc/>
    [DoNotExtractProperty]
    [Relationship(typeof(ExtractionInformation), RelationshipType.IgnoreableLocalReference,
        ValueGetter = nameof(GetAllExtractionInformation))]
    public int? PivotCategory_ExtractionInformation_ID
    {
        get => _pivotCategoryExtractionInformationID;
        set => SetField(ref _pivotCategoryExtractionInformationID, value);
    }

    /// <inheritdoc/>
    [DoNotExtractProperty]
    [DoNotImportDescriptions]
    public bool IsDeprecated
    {
        get => _isDeprecated;
        set => SetField(ref _isDeprecated, value);
    }

    /// <inheritdoc/>
    [DoNotExtractProperty]
    [DoNotImportDescriptions]
    public bool IsInternalDataset
    {
        get => _isInternalDataset;
        set => SetField(ref _isInternalDataset, value);
    }

    /// <inheritdoc/>
    [Relationship(typeof(ExternalDatabaseServer), RelationshipType.LocalReference)]
    [DoNotExtractProperty]
    public int? LiveLoggingServer_ID
    {
        get => _liveLoggingServerID;
        set => SetField(ref _liveLoggingServerID, value);
    }

    /// <inheritdoc/>
    public DateTime? DatasetStartDate
    {
        get => _datasetStartDate;
        set => SetField(ref _datasetStartDate, value);
    }
    /// <inheritdoc/>

    public string ShortDescription { get => _shortDescription; set => SetField(ref _shortDescription, value); }
    /// <inheritdoc/>
    public string DataType { get => _dataType; set => SetField(ref _dataType, value); }
    /// <inheritdoc/>
    public string DataSubType { get => _dataSubtype; set => SetField(ref _dataSubtype, value); }
    /// <inheritdoc/>
    public string DataSource { get => _dataSource; set => SetField(ref _dataSource, value); }
    /// <inheritdoc/>
    public string DataSourceSetting { get => _dataSourceSetting; set => SetField(ref _dataSourceSetting, value); }
    /// <inheritdoc/>
    public DateTime? DatasetReleaseDate { get => _datasetReleaseDate; set => SetField(ref _datasetReleaseDate, value); }
    /// <inheritdoc/>
    public DateTime? StartDate { get => _startDate; set => SetField(ref _startDate, value); }
    /// <inheritdoc/>
    public DateTime? EndDate { get => _endDate; set => SetField(ref _endDate, value); }
    /// <inheritdoc/>
    public UpdateLagTimes UpdateLag { get => _updateLag; set => SetField(ref _updateLag, value); }
    /// <inheritdoc/>
    public string Juristiction { get => _juristiction; set => SetField(ref _juristiction, value); }
    /// <inheritdoc/>
    public string DataController { get => _dataController; set => SetField(ref _dataController, value); }
    /// <inheritdoc/>
    public string DataProcessor { get => _dataProcessor; set => SetField(ref _dataProcessor, value); }
    /// <inheritdoc/>
    public string ControlledVocabulary { get => _controlledVocabulary; set => SetField(ref _controlledVocabulary, value); }
    /// <inheritdoc/>
    public string AssociatedPeople { get => _associatedPeople; set => SetField(ref _associatedPeople, value); }
    /// <inheritdoc/>

    public string AssociatedMedia { get => _associatedMedia; set => SetField(ref _associatedMedia, value); }

    public string Doi { get => _doi; set => SetField(ref _doi, value); }

    #endregion

    #region Relationships

    /// <inheritdoc/>
    [NoMappingToDatabase]
    public CatalogueItem[] CatalogueItems => _knownCatalogueItems.Value;

    /// <inheritdoc/>
    public LoadMetadata[] LoadMetadatas()
    {
        
        var loadMetadataLinkIDs = Repository.GetAllObjectsWhere<LoadMetadataCatalogueLinkage>("CatalogueID", ID).Select(l => l.LoadMetadataID);

        return Repository.GetAllObjects<LoadMetadata>().Where(cat => loadMetadataLinkIDs.Contains(cat.ID)).ToArray();
    }

    /// <inheritdoc/>
    [NoMappingToDatabase]
    public AggregateConfiguration[] AggregateConfigurations =>
        Repository.GetAllObjectsWithParent<AggregateConfiguration>(this);

    /// <inheritdoc/>
    [NoMappingToDatabase]
    public ExternalDatabaseServer LiveLoggingServer =>
       LiveLoggingServer_ID == null
                   ? null
                   : Repository.GetObjectByID<ExternalDatabaseServer>((int)LiveLoggingServer_ID);

    /// <inheritdoc/>
    [NoMappingToDatabase]
    public ExtractionInformation TimeCoverage_ExtractionInformation =>
        TimeCoverage_ExtractionInformation_ID == null
            ? null
            : Repository.GetObjectByID<ExtractionInformation>(TimeCoverage_ExtractionInformation_ID.Value);

    /// <inheritdoc/>
    [NoMappingToDatabase]
    public ExtractionInformation PivotCategory_ExtractionInformation =>
        PivotCategory_ExtractionInformation_ID == null
            ? null
            : Repository.GetObjectByID<ExtractionInformation>(PivotCategory_ExtractionInformation_ID.Value);

    #endregion

    #region Enums

    /// <summary>
    /// Somewhat arbitrary concepts for defining the limitations of a Catalogues data
    /// </summary>
    public enum CatalogueType
    {
        /// <summary>
        /// No CatalogueType has been specified
        /// </summary>
        Unknown,

        /// <summary>
        /// Catalogue data relates to a research study
        /// </summary>
        ResearchStudy,

        /// <summary>
        /// Catalogue data relates to or defines a Cohort
        /// </summary>
        Cohort,

        /// <summary>
        /// Catalogue data is collected by a national registry
        /// </summary>
        NationalRegistry,

        /// <summary>
        /// Catalogue data is collected by a healthcare provider
        /// </summary>
        HealthcareProviderRegistry,

        /// <summary>
        /// Catalogue data can be classified as Electronic Health Records (prescriptions, hospital records etc.)
        /// </summary>
        EHRExtract
    }

    /// <summary>
    /// Notional user declared period on which the data in the Catalogue is refreshed.  This may not have any bearing
    /// on reality.  Not used by RDMP for any technical processes.
    /// </summary>
    public enum CataloguePeriodicity
    {
        /// <summary>
        /// No period for the dataset has been specified
        /// </summary>
        Unknown,

        /// <summary>
        /// Data is updated on a daily basis
        /// </summary>
        Daily,

        /// <summary>
        /// Data is updated on a weekly basis
        /// </summary>
        Weekly,

        /// <summary>
        /// Data is updated every 2 weeks
        /// </summary>
        Fortnightly,

        /// <summary>
        /// Data is updated every month
        /// </summary>
        Monthly,

        /// <summary>
        /// Data is updated every 2 months
        /// </summary>
        BiMonthly,

        /// <summary>
        /// Data is updated every 4 months
        /// </summary>
        Quarterly,

        /// <summary>
        /// Data is updated on a yearly basis
        /// </summary>
        Yearly
    }

    /// <summary>
    /// Notional user declared boundary for the dataset defined by the Catalogue.  The data should be isolated to this Granularity
    /// </summary>
    public enum CatalogueGranularity
    {
        /// <summary>
        /// No granularity has been specified
        /// </summary>
        Unknown,

        /// <summary>
        /// Contains data relating to multiple nations
        /// </summary>
        National,

        /// <summary>
        /// Contains data relating to multiple regions (e.g. Scotland / England)
        /// </summary>
        Regional,

        /// <summary>
        /// Contains data relating to multiple healthboards (e.g. Tayside / Fife)
        /// </summary>
        HealthBoard,

        /// <summary>
        /// Contains data relating to multiple hospitals (e.g. Ninewells)
        /// </summary>
        Hospital,

        /// <summary>
        /// Contains data relating to multiple clinics (e.g. Radiology)
        /// </summary>
        Clinic
    }

    /// <summary>
    /// Notional user declared type of data catalogue contains. Copied from the HDR Gateway
    /// </summary>
    public enum DatasetType
    {
        /// <summary>
        ///  Includes any data related to mental health, cardiovascular, cancer, rare diseases, metabolic and endocrine, neurological, reproductive, maternity and neonatology, respiratory, immunity, musculoskeletal, vision, renal and urogenital, oral and gastrointestinal, cognitive function or hearing.
        /// </summary>
        HealthcareAndDisease,
        /// <summary>
        /// Includes any data related to treatment or interventions related to vaccines or which are preventative or therapeutic in nature.
        /// </summary>
        TreatmentsAndInterventions,
        /// <summary>
        /// Includes any data related to laboratory or other diagnostics.
        /// </summary>
        MeasurementsAndTests,
        /// <summary>
        /// Includes any data related to CT, MRI, PET, x-ray, ultrasound or pathology imaging.
        /// </summary>
        ImagingTypes,
        /// <summary>
        ///  Indicates whether the dataset relates to head, chest, arm abdomen or leg imaging.
        /// </summary>
        ImagingAreaOfTheBody,
        /// <summary>
        ///  Includes any data related to proteomics, transcriptomics, epigenomics, metabolomics, multiomics, metagenomics or genomics.
        /// </summary>
        Omics,
        /// <summary>
        ///  Includes any data related to education, crime and justice, ethnicity, housing, labour, ageing, economics, marital status, social support, deprivation, religion, occupation, finances or family circumstances.
        /// </summary>
        Socioeconomic,
        /// <summary>
        /// Includes any data related to smoking, physical activity, dietary habits or alcohol.
        /// </summary>
        Lifestyle,
        /// <summary>
        ///  Includes any data related to disease registries for research, national disease registries, audits, or birth and deaths records.
        /// </summary>
        Registry,
        /// <summary>
        ///  Includes any data related to the monitoring or study of environmental or energy factors or events.
        /// </summary>
        EnvironmentalAndEnergy,
        /// <summary>
        ///  Includes any data related to the study or application of information and communication.
        /// </summary>
        InformationAndCommunication,
        /// <summary>
        ///  Includes any data related to political views, activities, voting, etc.
        /// </summary>
        Politics
    }

    public enum DatasetSubType
    {
        NotApplicable,
        BirthsAndDeaths,
        NationalDiseaseRegistryAndAudits,
        ResearchDiseaseRegistry,
        Alcohol,
        DietaryHabits,
        PhysicalActivity,
        FamilyCircumstance,
        Finances,
        Occupation,
        Religion,
        Deprivation,
        SocialSupport,
        MaritalStatus,
        Economics,
        Ageing,
        Labour,
        Housing,
        Ethnicity,
        CrimeAndJustice,
        Education,
        Lipidomics,
        Genomics,
        Metagenomics,
        Metabolomics,
        Epigenomics,
        Transcriptomics,
        Proteomics,
        Leg,
        Abdomen,
        Arm,
        MentalHealth,
        Cardiovascular,
        Cancer,
        RareDiseases,
        MetabolicAndEndocrine,
        Neurological,
        Reproductve,
        MaternityAndNeonatology,
        Chest,
        Head,
        Pathology,
        Ultrasound,
        XRay,
        PET,
        MRI,
        CT,
        CognitiveFunction,
        Hearing,
        Others,
        Vaccines,
        Preventative,
        Theraputic,
        Laboratory,
        OtherDiagnosis,
        Respiratory,
        Immunity,
        Musculoskeletal,
        Vision,
        RenalAndUrogenital,
        OralAndGastrointestinal
    }


    public enum UpdateFrequencies
    {
        Other,
        Static,
        Irregular,
        Continuous,
        Biennial,
        Annual,
        Biannual,
        Quarterly,
        Bimonthly,
        Monthly,
        Biweekly,
        Weekly,
        TwiceWeekly,
        Daily
    }

    public enum UpdateLagTimes
    {
        Other,
        LessThanAWeek,
        OneToTwoWeeks,
        TwoToFourWeeks,
        OneToTwoMonths,
        TwoToSixMonths,
        SixMonthsPlus,
        Variable,
        NotApplicable
    }

    public enum DatasetPurpose
    {
        Other,
        ResearchCohort,
        Study,
        DiseaseRegistry,
        Trial,
        Care,
        Audit,
        Administrative,
        Finantial,
        Statutory
    }

    public enum DataSourceTypes
    {
        Other,
        EPR,
        ElectronicSurvey,
        LIMS,
        PaperBased,
        FreeTextNLP,
        MachineLearning
    }
    public enum DataSourceSettingTypes
    {
        Other,
        CohortStudyTrial,
        Clinic,
        PrimaryCareReferrals,
        PrimaryCareClinic,
        PrimaryCareOutOfHours,
        SecondaryCareAccidentAndEmergency,
        SecondaryCareOutpatients,
        SecondaryCareInPateints,
        SecondaryCareAmbulance,
        SecondaryCareICU,
        PrescribingCommunityPharmacy,
        PateintReportOutcome,
        Wearables,
        LocalAuthority,
        NationalGovernment,
        Community,
        Services,
        Home,
        Private,
        SocialCareHealthcareAtHome,
        SocialCareOthersocialData,
        Census
    }
    #endregion

    /// <summary>
    /// Creates a new instance from an unknown repository (for use with serialization).  You must set
    /// <see cref="IMapsDirectlyToDatabaseTable.Repository"/> before Methods that retrieve other objects or
    /// save state can be called (e.g. <see cref="ISaveable.SaveToDatabase"/>)
    /// </summary>
    public Catalogue()
    {
        ClearAllInjections();
    }

    /// <summary>
    /// Declares a new empty virtual dataset with the given Name.  This will not have any virtual columns and will not be tied to any underlying tables.
    /// 
    /// <para>The preferred method of getting a Catalogue is to use <see cref="TableInfoImporter"/> and <see cref="ForwardEngineerCatalogue"/></para>
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="name"></param>
    public Catalogue(ICatalogueRepository repository, string name)
    {
        var loggingServer = repository.GetDefaultFor(PermissableDefaults.LiveLoggingServer_ID);

        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "Name", name },
            { "LiveLoggingServer_ID", loggingServer == null ? DBNull.Value : loggingServer.ID }
        });

        if (ID == 0 || string.IsNullOrWhiteSpace(Name) || Repository != repository)
            throw new ArgumentException("Repository failed to properly hydrate this class");

        //if there is a default logging server
        if (LiveLoggingServer_ID == null)
        {
            var liveLoggingServer = repository.GetDefaultFor(PermissableDefaults.LiveLoggingServer_ID);

            if (liveLoggingServer != null)
                LiveLoggingServer_ID = liveLoggingServer.ID;
        }


        ClearAllInjections();
    }

    /// <summary>
    /// Creates a single runtime instance of the Catalogue based on the current state of the row read from the DbDataReader (does not advance the reader)
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="r"></param>
    internal Catalogue(ICatalogueRepository repository, DbDataReader r)
        : base(repository, r)
    {

        Acronym = r["Acronym"].ToString();
        Name = r["Name"].ToString();
        Description = r["Description"].ToString();

        //detailed info url with support for invalid urls
        Detail_Page_URL = ParseUrl(r, "Detail_Page_URL");

        LoggingDataTask = r["LoggingDataTask"] as string;

        if (r["LiveLoggingServer_ID"] == DBNull.Value)
            LiveLoggingServer_ID = null;
        else
            LiveLoggingServer_ID = (int)r["LiveLoggingServer_ID"];

        ////Type - with handling for invalid enum values listed in database
        var type = r["Type"];
        if (type == null || type == DBNull.Value)
        {
            Type = CatalogueType.Unknown;
        }
        else
        {
            if (Enum.TryParse(type.ToString(), true, out CatalogueType typeAsEnum))
                Type = typeAsEnum;
            else
                throw new Exception($" r[\"Type\"] had value {type} which is not contained in Enum CatalogueType");
        }
        var purpose = r["Purpose"];
        if (purpose == null || purpose == DBNull.Value)
        {
            Purpose = DatasetPurpose.Other;
        }
        else
        {
            if (Enum.TryParse(purpose.ToString(), true, out DatasetPurpose purposeAsEnum))
                Purpose = purposeAsEnum;
            else
                throw new Exception($" r[\"Purpose\"] had value {purpose} which is not contained in Enum DatasetPurpose");
        }

        //Periodicity - with handling for invalid enum values listed in database
        var periodicity = r["Periodicity"];
        if (periodicity == null || periodicity == DBNull.Value)
        {
            Periodicity = CataloguePeriodicity.Unknown;
        }
        else
        {
            if (Enum.TryParse(periodicity.ToString(), true, out CataloguePeriodicity periodicityAsEnum))
                Periodicity = periodicityAsEnum;
            else
                throw new Exception(
                    $" r[\"Periodicity\"] had value {periodicity} which is not contained in Enum CataloguePeriodicity");
        }

        var granularity = r["Granularity"];
        if (granularity == null || granularity == DBNull.Value)
        {
            Granularity = CatalogueGranularity.Unknown;
        }
        else
        {
            if (Enum.TryParse(granularity.ToString(), true, out CatalogueGranularity granularityAsEnum))
                Granularity = granularityAsEnum;
            else
                throw new Exception(
                    $" r[\"granularity\"] had value {granularity} which is not contained in Enum CatalogueGranularity");
        }

        Geographical_coverage = r["Geographical_coverage"].ToString();
        Background_summary = r["Background_summary"].ToString();
        Search_keywords = r["Search_keywords"].ToString();
        var updateFreq = r["Update_freq"];
        if (updateFreq == null || updateFreq == DBNull.Value)
        {
            Update_freq = UpdateFrequencies.Other;
        }
        else
        {
            if (Enum.TryParse(updateFreq.ToString(), true, out UpdateFrequencies updateFreqAsEnum))
                Update_freq = updateFreqAsEnum;
            else
                throw new Exception(
                    $" r[\"Update_freq\"] had value {updateFreq} which is not contained in Enum UpdateFrequencies");
        }
        Update_sched = r["Update_sched"].ToString();
        Time_coverage = r["Time_coverage"].ToString();
        SubjectNumbers = r["SubjectNumbers"].ToString();

        var dt = r["Last_revision_date"];
        if (dt == null || dt == DBNull.Value)
            Last_revision_date = null;
        else
            Last_revision_date = (DateTime)dt;

        Contact_details = r["Contact_details"].ToString();
        Resource_owner = r["Resource_owner"].ToString();
        Attribution_citation = r["Attribution_citation"].ToString();
        Access_options = r["Access_options"].ToString();

        Country_of_origin = r["Country_of_origin"].ToString();
        Data_standards = r["Data_standards"].ToString();
        Administrative_contact_name = r["Administrative_contact_name"].ToString();
        Administrative_contact_email = r["Administrative_contact_email"].ToString();
        Administrative_contact_telephone = r["Administrative_contact_telephone"].ToString();
        Administrative_contact_address = r["Administrative_contact_address"].ToString();
        Ethics_approver = r["Ethics_approver"].ToString();
        Source_of_data_collection = r["Source_of_data_collection"].ToString();

        if (r["Explicit_consent"] != null && r["Explicit_consent"] != DBNull.Value)
            Explicit_consent = (bool)r["Explicit_consent"];

        TimeCoverage_ExtractionInformation_ID = ObjectToNullableInt(r["TimeCoverage_ExtractionInformation_ID"]);
        PivotCategory_ExtractionInformation_ID = ObjectToNullableInt(r["PivotCategory_ExtractionInformation_ID"]);

        var oDatasetStartDate = r["DatasetStartDate"];
        if (oDatasetStartDate == null || oDatasetStartDate == DBNull.Value)
            DatasetStartDate = null;
        else
            DatasetStartDate = (DateTime)oDatasetStartDate;


        ValidatorXML = r["ValidatorXML"] as string;

        Ticket = r["Ticket"] as string;

        //detailed info url with support for invalid urls
        API_access_URL = ParseUrl(r, "API_access_URL");
        Browse_URL = ParseUrl(r, "Browse_URL");
        Bulk_Download_URL = ParseUrl(r, "Bulk_Download_URL");
        Query_tool_URL = ParseUrl(r, "Query_tool_URL");
        Source_URL = ParseUrl(r, "Source_URL");
        IsDeprecated = (bool)r["IsDeprecated"];
        IsInternalDataset = (bool)r["IsInternalDataset"];

        Folder = r["Folder"].ToString();

        ShortDescription = r["ShortDescription"].ToString();
        DataSource = r["DataSource"].ToString();
        DataSourceSetting = r["DataSourceSetting"].ToString();
        StartDate = !string.IsNullOrEmpty(r["StartDate"].ToString()) ? DateTime.Parse(r["StartDate"].ToString()) : null;
        EndDate = !string.IsNullOrEmpty(r["EndDate"].ToString()) ? DateTime.Parse(r["EndDate"].ToString()) : null;
        DatasetReleaseDate = !string.IsNullOrEmpty(r["DatasetReleaseDate"].ToString()) ? DateTime.Parse(r["DatasetReleaseDate"].ToString()) : null;
        DataController = r["DataController"].ToString();
        DataProcessor = r["DataProcessor"].ToString();
        Juristiction = r["Juristiction"].ToString();
        AssociatedPeople = r["AssociatedPeople"].ToString();
        AssociatedMedia = r["AssociatedMedia"].ToString();
        ControlledVocabulary = r["ControlledVocabulary"].ToString();
        DataType = r["DataType"].ToString();
        DataSubType = r["DataSubType"].ToString();
        Doi = r["Doi"].ToString();
        var updateLag = r["UpdateLag"];
        if (updateLag == null || updateLag == DBNull.Value)
        {
            UpdateLag = UpdateLagTimes.Other;
        }
        else
        {
            if (Enum.TryParse(updateLag.ToString(), true, out UpdateLagTimes updateLagAsEnum))
                UpdateLag = updateLagAsEnum;
            else
                throw new Exception(
                    $" r[\"UpdateLag\"] had value {updateLag} which is not contained in Enum UpdateLagTimes");
        }
        ClearAllInjections();
    }

    internal Catalogue(ShareManager shareManager, ShareDefinition shareDefinition)
    {
        shareManager.UpsertAndHydrate(this, shareDefinition);
        ClearAllInjections();
    }

    /// <inheritdoc/>
    public override string ToString() => Name;

    /// <summary>
    /// Sorts alphabetically based on <see cref="Name"/>
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public int CompareTo(object obj)
    {
        if (obj is Catalogue)
            return -string.Compare(obj.ToString(), ToString(),
                StringComparison.CurrentCulture); //sort alphabetically (reverse)

        throw new Exception($"Cannot compare {GetType().Name} to {obj?.GetType().Name}");
    }

    public override bool Equals(object obj) => obj is Catalogue c && (ReferenceEquals(this, c) || Equals(c));

    public bool Equals(Catalogue other) => base.Equals(other);

    public override int GetHashCode() => base.GetHashCode();

    public static bool operator ==(Catalogue left, Catalogue right) => Equals(left, right);

    public static bool operator !=(Catalogue left, Catalogue right) => !Equals(left, right);

    /// <summary>
    /// Checks that the Catalogue has a sensible Name (See <see cref="IsAcceptableName(string)"/>).  Then checks that there are no missing ColumnInfos
    /// </summary>
    /// <param name="notifier"></param>
    public void Check(ICheckNotifier notifier)
    {
        if (!IsAcceptableName(Name, out var reason))
            notifier.OnCheckPerformed(
                new CheckEventArgs(
                    $"Catalogue name {Name} (ID={ID}) does not follow naming conventions reason:{reason}",
                    CheckResult.Fail));
        else
            notifier.OnCheckPerformed(new CheckEventArgs($"Catalogue name {Name} follows naming conventions ",
                CheckResult.Success));

        var tables = GetTableInfoList(true);
        foreach (var t in tables)
            t.Check(notifier);

        var extractionInformations = GetAllExtractionInformation(ExtractionCategory.Core);

        if (extractionInformations.Any())
        {
            var missingColumnInfos = false;

            foreach (var missingColumnInfo in extractionInformations.Where(e => e.ColumnInfo == null))
            {
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        $"ColumnInfo behind ExtractionInformation/CatalogueItem {missingColumnInfo.GetRuntimeName()} is MISSING, it must have been deleted",
                        CheckResult.Fail));
                missingColumnInfos = true;
            }

            if (missingColumnInfos)
                return;

            notifier.OnCheckPerformed(
                new CheckEventArgs(
                    $"Found {extractionInformations.Length} ExtractionInformation(s), preparing to validate SQL with QueryBuilder",
                    CheckResult.Success));

            var accessContext = DataAccessContext.InternalDataProcessing;

            try
            {
                var setInitialDatabase = tables.Any(static t => t.Database != null);
                var server = DataAccessPortal.ExpectDistinctServer(tables, accessContext, setInitialDatabase);
                using var con = server.GetConnection();
                con.Open();

                string sql;
                try
                {
                    var qb = new QueryBuilder(null, null)
                    {
                        TopX = 1
                    };
                    qb.AddColumnRange(extractionInformations);

                    sql = qb.SQL;
                    notifier.OnCheckPerformed(new CheckEventArgs(
                        $"Query Builder assembled the following SQL:{Environment.NewLine}{sql}", CheckResult.Success));
                }
                catch (Exception e)
                {
                    notifier.OnCheckPerformed(
                        new CheckEventArgs($"Could not generate extraction SQL for Catalogue {this}",
                            CheckResult.Fail, e));
                    return;
                }

                using (var cmd = DatabaseCommandHelper.GetCommand(sql, con))
                {
                    cmd.CommandTimeout = 10;
                    using var r = cmd.ExecuteReader();
                    if (r.Read())
                        notifier.OnCheckPerformed(new CheckEventArgs(
                            $"successfully read a row of data from the extraction SQL of Catalogue {this}",
                            CheckResult.Success));
                    else
                        notifier.OnCheckPerformed(new CheckEventArgs(
                            $"The query produced an empty result set for Catalogue{this}", CheckResult.Warning));
                }

                con.Close();
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        $"Extraction SQL Checking failed for Catalogue {this} make sure that you can access the underlying server under DataAccessContext.{accessContext} and that the SQL generated runs correctly (see internal exception for details)",
                        CheckResult.Fail, e));
            }
        }

        //supporting documents
        var f = new SupportingDocumentsFetcher(this);
        f.Check(notifier);
    }

    /// <inheritdoc/>
    public ITableInfo[] GetTableInfoList(bool includeLookupTables)
    {
        GetTableInfos(out var normalTables, out var lookupTables);

        return includeLookupTables ? normalTables.Union(lookupTables).ToArray() : normalTables.ToArray();
    }

    /// <inheritdoc/>
    public ITableInfo[] GetLookupTableInfoList()
    {
        GetTableInfos(out _, out var lookupTables);

        return lookupTables.ToArray();
    }

    /// <inheritdoc/>
    public void GetTableInfos(out List<ITableInfo> normalTables, out List<ITableInfo> lookupTables)
    {
        GetAllTableInfos(t => t.IsLookupTable(), out normalTables, out lookupTables);
    }

    /// <inheritdoc/>
    public void GetTableInfos(ICoreChildProvider provider, out List<ITableInfo> normalTables,
        out List<ITableInfo> lookupTables)
    {
        GetAllTableInfos(t => t.IsLookupTable(provider), out normalTables, out lookupTables);
    }

    private void GetAllTableInfos(Func<ITableInfo, bool> isLookupTableDelegate, out List<ITableInfo> normalTables,
        out List<ITableInfo> lookupTables)
    {
        var tables = GetColumnInfos().GroupBy(c => c.TableInfo_ID).Select(c => c.First().TableInfo).ToArray();

        normalTables = new List<ITableInfo>(tables.Where(t => !isLookupTableDelegate(t)));
        lookupTables = tables.Except(normalTables).ToList();
    }

    private IEnumerable<ColumnInfo> GetColumnInfos()
    {
        return CatalogueItems.All(ci => ci.IsColumnInfoCached())
            ? CatalogueItems.Select(ci => ci.ColumnInfo).Where(col => col != null)
            : Repository.GetAllObjectsInIDList<ColumnInfo>(CatalogueItems.Where(ci => ci.ColumnInfo_ID.HasValue)
                .Select(ci => ci.ColumnInfo_ID.Value).Distinct().ToList());
    }

    /// <inheritdoc/>
    public ExtractionFilter[] GetAllMandatoryFilters()
    {
        return GetAllExtractionInformation(ExtractionCategory.Any).SelectMany(f => f.ExtractionFilters)
            .Where(f => f.IsMandatory).ToArray();
    }

    /// <inheritdoc/>
    public ExtractionFilter[] GetAllFilters()
    {
        return GetAllExtractionInformation(ExtractionCategory.Any).SelectMany(f => f.ExtractionFilters).ToArray();
    }

    /// <inheritdoc/>
    public DiscoveredServer GetDistinctLiveDatabaseServer(DataAccessContext context, bool setInitialDatabase,
        out IDataAccessPoint distinctAccessPoint)
    {
        var tables = GetTableInfosIdeallyJustFromMainTables();

        distinctAccessPoint = tables.FirstOrDefault();

        return DataAccessPortal.ExpectDistinctServer(tables, context, setInitialDatabase);
    }

    /// <inheritdoc/>
    public DiscoveredServer GetDistinctLiveDatabaseServer(DataAccessContext context, bool setInitialDatabase) =>
        DataAccessPortal.ExpectDistinctServer(GetTableInfosIdeallyJustFromMainTables(), context, setInitialDatabase);

    /// <inheritdoc/>
    public ITableInfo[] GetTableInfosIdeallyJustFromMainTables()
    {
        //try with only the normal tables
        var tables = GetTableInfoList(false);

        //there are no normal tables!
        if (!tables.Any())
            tables = GetTableInfoList(true);

        return tables;
    }

    /// <inheritdoc/>
    public DatabaseType? GetDistinctLiveDatabaseServerType()
    {
        var tables = GetTableInfosIdeallyJustFromMainTables();

        var type = tables.Select(t => t.DatabaseType).Distinct().ToArray();

        return type.Length switch
        {
            0 => null,
            1 => type[0],
            _ => throw new AmbiguousDatabaseTypeException(
                $"The Catalogue '{this}' has TableInfos belonging to multiple DatabaseTypes ({string.Join(",", tables.Select(t => $"{t.GetRuntimeName()}(ID={t.ID} is {t.DatabaseType})"))}")
        };
    }

    /// <summary>
    /// Gets the <see cref="LogManager"/> for logging load events related to this Catalogue / its LoadMetadata (if it has one).  This will throw if no
    /// logging server has been configured.
    /// </summary>
    /// <returns></returns>
    public LogManager GetLogManager()
    {
        if (LiveLoggingServer_ID == null)
            throw new Exception($"No live logging server set for Catalogue {Name}");

        var server = DataAccessPortal.ExpectServer(LiveLoggingServer, DataAccessContext.Logging);

        return new LogManager(server);
    }

    /// <inheritdoc/>
    public IHasDependencies[] GetObjectsThisDependsOn()
    {
        var iDependOn = new List<IHasDependencies>();

        iDependOn.AddRange(CatalogueItems);
        var lmdList = LoadMetadatas();
        if (lmdList.Length > 0)
            foreach (var lmd in lmdList)
            {
                iDependOn.Add(lmd);
            }

        return iDependOn.ToArray();
    }

    /// <inheritdoc/>
    public IHasDependencies[] GetObjectsDependingOnThis() => AggregateConfigurations;

    /// <inheritdoc/>
    public SupportingDocument[] GetAllSupportingDocuments(FetchOptions fetch)
    {
        return Repository.GetAllObjects<SupportingDocument>().Where(o => Fetch(o, fetch)).ToArray();
    }

    /// <inheritdoc/>
    public SupportingSQLTable[] GetAllSupportingSQLTablesForCatalogue(FetchOptions fetch)
    {
        return Repository.GetAllObjects<SupportingSQLTable>().Where(o => Fetch(o, fetch)).ToArray();
    }

    private bool Fetch(ISupportingObject o, FetchOptions fetch)
    {
        return fetch switch
        {
            FetchOptions.AllGlobals => o.IsGlobal,
            FetchOptions.ExtractableGlobalsAndLocals => (o.Catalogue_ID == ID || o.IsGlobal) && o.Extractable,
            FetchOptions.ExtractableGlobals => o.IsGlobal && o.Extractable,
            FetchOptions.AllLocals => o.Catalogue_ID == ID && !o.IsGlobal,
            FetchOptions.ExtractableLocals => o.Catalogue_ID == ID && o.Extractable && !o.IsGlobal,
            FetchOptions.AllGlobalsAndAllLocals => o.Catalogue_ID == ID || o.IsGlobal,
            _ => throw new ArgumentOutOfRangeException(nameof(fetch))
        };
    }


    private string GetFetchSQL(FetchOptions fetch)
    {
        return fetch switch
        {
            FetchOptions.AllGlobals => "WHERE IsGlobal=1",
            FetchOptions.ExtractableGlobalsAndLocals => $"WHERE (Catalogue_ID={ID} OR IsGlobal=1) AND Extractable=1",
            FetchOptions.ExtractableGlobals => "WHERE IsGlobal=1 AND Extractable=1",
            FetchOptions.AllLocals =>
                $"WHERE Catalogue_ID={ID}  AND IsGlobal=0" //globals still retain their Catalogue_ID in case the configurer removes the global attribute in which case they revert to belonging to that Catalogue as a local
            ,
            FetchOptions.ExtractableLocals => $"WHERE Catalogue_ID={ID} AND Extractable=1 AND IsGlobal=0",
            FetchOptions.AllGlobalsAndAllLocals => $"WHERE Catalogue_ID={ID} OR IsGlobal=1",
            _ => throw new ArgumentOutOfRangeException(nameof(fetch))
        };
    }

    /// <inheritdoc/>
    public ExtractionInformation[] GetAllExtractionInformation() => GetAllExtractionInformation(ExtractionCategory.Any);

    /// <inheritdoc/>
    public ExtractionInformation[] GetAllExtractionInformation(ExtractionCategory category)
    {
        return
            CatalogueItems.Select(ci => ci.ExtractionInformation)
                .Where(e => e != null &&
                            (e.ExtractionCategory == category || category == ExtractionCategory.Any))
                .ToArray();
    }

    private CatalogueExtractabilityStatus _extractabilityStatus;

    /// <summary>
    /// Records the known extractability status (as a cached answer for <see cref="GetExtractabilityStatus"/>)
    /// </summary>
    /// <param name="instance"></param>
    public void InjectKnown(CatalogueExtractabilityStatus instance)
    {
        _extractabilityStatus = instance;
    }

    /// <inheritdoc/>
    public void InjectKnown(CatalogueItem[] instance)
    {
        _knownCatalogueItems = new Lazy<CatalogueItem[]>(instance);
    }

    /// <summary>
    /// Clears the cached answer of <see cref="GetExtractabilityStatus"/>
    /// </summary>
    public void ClearAllInjections()
    {
        _extractabilityStatus = null;
        _knownCatalogueItems =
            new Lazy<CatalogueItem[]>(() => Repository.GetAllObjectsWithParent<CatalogueItem, Catalogue>(this));
    }

    /// <inheritdoc/>
    public CatalogueExtractabilityStatus GetExtractabilityStatus(IDataExportRepository dataExportRepository)
    {
        if (_extractabilityStatus != null)
            return _extractabilityStatus;

        if (dataExportRepository == null)
            return null;

        _extractabilityStatus = dataExportRepository.GetExtractabilityStatus(this);
        return _extractabilityStatus;
    }

    /// <inheritdoc/>
    public bool IsProjectSpecific(IDataExportRepository dataExportRepository)
    {
        var e = GetExtractabilityStatus(dataExportRepository);
        return e is { IsProjectSpecific: true };
    }

    /// <summary>
    /// Gets an IQuerySyntaxHelper for the <see cref="GetDistinctLiveDatabaseServerType"/> amongst all underlying <see cref="TableInfo"/>.  This can be used to assist query building.
    /// </summary>
    /// <returns></returns>
    public IQuerySyntaxHelper GetQuerySyntaxHelper()
    {
        var type = GetDistinctLiveDatabaseServerType() ?? throw new AmbiguousDatabaseTypeException(
            $"Catalogue '{this}' has no extractable columns so no Database Type could be determined");
        return QuerySyntaxHelperFactory.Create(type);
    }

    #region Static Methods

    /// <summary>
    /// Returns true if the given name would be sensible for a Catalogue.  This means no slashes, hashes @ symbols etc and other things which make XML serialization hard
    /// or prevent naming a database table after a Catalogue (all things we might want to do with the <see cref="Catalogue.Name"/>).
    /// </summary>
    /// <param name="name"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    public static bool IsAcceptableName(string name, out string reason)
    {
        if (name == null || string.IsNullOrWhiteSpace(name))
        {
            reason = "Name cannot be blank";
            return false;
        }

        var invalidCharacters = name.Where(c =>
            Path.GetInvalidPathChars().Contains(c) || c == '\\' || c == '/' || c == '.' || c == '#' || c == '@' ||
            c == '$').ToArray();
        if (invalidCharacters.Any())
        {
            reason =
                $"The following invalid characters were found:{string.Join(",", invalidCharacters.Select(c => $"'{c}'"))}";
            return false;
        }

        reason = null;
        return true;
    }

    /// <inheritdoc cref="Catalogue.IsAcceptableName(string,out string)"/>
    public static bool IsAcceptableName(string name) => IsAcceptableName(name, out _);

    #endregion


    /// <inheritdoc/>
    public ICatalogue ShallowClone()
    {
        var clone = new Catalogue(CatalogueRepository, $"{Name} Clone");
        CopyShallowValuesTo(clone);
        return clone;
    }


    public bool IsApiCall() => Name.StartsWith(PluginCohortCompiler.ApiPrefix);

    public bool IsApiCall(out IPluginCohortCompiler plugin)
    {
        if (!IsApiCall())
        {
            plugin = null;
            return false;
        }

        plugin = PluginCohortCompilerFactory
            .CreateAll().FirstOrDefault(p => p.ShouldRun(this));

        return true;
    }

    public override string GetSummary(bool includeName, bool includeID)
    {
        var extractionPrimaryKeys = CatalogueItems.Where(c => c.ExtractionInformation?.IsPrimaryKey ?? false).ToArray();

        var sb = new StringBuilder();
        sb.Append(base.GetSummary(includeName, includeID));

        if (extractionPrimaryKeys.Any())
            sb.AppendLine($"Extraction Primary Key(s): {extractionPrimaryKeys.ToBeautifulString()}");

        return sb.ToString();
    }
}