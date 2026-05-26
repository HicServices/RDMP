using FAnsi;
using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using MongoDB.Driver;
using Rdmp.Core.CohortCreation.Execution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.EntityFramework;
using Rdmp.Core.EntityFramework.Helpers;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Revertable;
using Rdmp.Core.Providers;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Annotations;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;
using static Rdmp.Core.EntityFramework.Models.SupportingSQLTable;

namespace Rdmp.Core.EntityFramework.Models
{
    [Table("Catalogue")]
    public class Catalogue : DatabaseObject, IHasFolder,ICatalogue
    {

        public Catalogue() { }
        public Catalogue(RDMPDbContext catalogueDbContext, string v)
        {
            CatalogueDbContext = catalogueDbContext;
        }

        [Key]
        public override int ID { get; set; }

        [NotMapped]
        public override RDMPDbContext CatalogueDbContext { get; set; }
        [MaxLength(100)]
        public string Acronym { get; set => SetField(ref field, value); }

        [Required]
        [MaxLength(500)]
        public string Name { get; set => SetField(ref field, value); }

        [MaxLength(500)]
        public string Folder { get => FolderHelper.Adjust(field); set => SetField(ref field, FolderHelper.Adjust(value)); } = "/";

        public string Description { get; set => SetField(ref field, value); }
        public string InternalNote { get; set => SetField(ref field, value); }

        public string ShortDescription { get; set => SetField(ref field, value); }
        public string Detail_Page_URL { get; set => SetField(ref field, value); }

        [Column(TypeName = "nvarchar(max)")]
        public string Type { get; set => SetField(ref field, value); }
        [Column(TypeName = "nvarchar(255)")]
        public string Purpose { get; set => SetField(ref field, value); }
        [Column(TypeName = "nvarchar(50)")]
        public string Periodicity { get; set => SetField(ref field, value); }
        [Column(TypeName = "nvarchar(max)")]
        public string Granularity { get; set => SetField(ref field, value); }
        public string Geographical_coverage { get; set => SetField(ref field, value); }
        public string Background_summary { get; set => SetField(ref field, value); }
        public string Search_keywords { get; set => SetField(ref field, value); }
        [Column(TypeName = "nvarchar(50)")]
        public string Update_freq { get; set => SetField(ref field, value); }
        public string Update_sched { get; set => SetField(ref field, value); }
        public string Time_coverage { get; set => SetField(ref field, value); }
        public DateTime? Last_revision_date { get; set => SetField(ref field, value); }
        public string Contact_details { get; set => SetField(ref field, value); }
        public string Resource_owner { get; set => SetField(ref field, value); }
        public string Attribution_citation { get; set => SetField(ref field, value); }
        public string Access_options { get; set => SetField(ref field, value); }
        public string SubjectNumbers { get; set => SetField(ref field, value); }
        public string API_access_URL { get; set => SetField(ref field, value); }
        public string Browse_URL { get; set => SetField(ref field, value); }
        public string Bulk_Download_URL { get; set => SetField(ref field, value); }
        public string Query_tool_URL { get; set => SetField(ref field, value); }
        public string Source_URL { get; set => SetField(ref field, value); }
        public string Country_of_origin { get; set => SetField(ref field, value); }
        public string Data_standards { get; set => SetField(ref field, value); }
        public string Administrative_contact_name { get; set => SetField(ref field, value); }
        public string Administrative_contact_email { get; set => SetField(ref field, value); }
        public string Administrative_contact_telephone { get; set => SetField(ref field, value); }
        public string Administrative_contact_address { get; set => SetField(ref field, value); }
        public bool? Explicit_consent { get; set => SetField(ref field, value); }
        public string Ethics_approver { get; set => SetField(ref field, value); }
        public string Source_of_data_collection { get; set => SetField(ref field, value); }
        public string Ticket { get; set => SetField(ref field, value); }
        public DateTime? DatasetStartDate { get; set => SetField(ref field, value); }
        public string DataType { get; set => SetField(ref field, value); }
        public string DataSubType { get; set => SetField(ref field, value); }
        public string DataSource { get; set => SetField(ref field, value); }
        public string DataSourceSetting { get; set => SetField(ref field, value); }
        public DateTime? DatasetReleaseDate { get; set => SetField(ref field, value); }
        public DateTime? StartDate { get; set => SetField(ref field, value); }
        public DateTime? EndDate { get; set => SetField(ref field, value); }
        [Column(TypeName = "nvarchar(255)")]

        public int? UpdateLag { get; set => SetField(ref field, value); }
        public string Juristiction { get; set => SetField(ref field, value); }
        public string DataController { get; set => SetField(ref field, value); }
        public string DataProcessor { get; set => SetField(ref field, value); }
        public string ControlledVocabulary { get; set => SetField(ref field, value); }
        public string AssociatedPeople { get; set => SetField(ref field, value); }
        public string AssociatedMedia { get; set => SetField(ref field, value); }
        public string Doi { get; set => SetField(ref field, value); }
        public bool IsDeprecated { get; set => SetField(ref field, value); }
        public bool IsInternalDataset { get; set => SetField(ref field, value); }

        public virtual ICollection<CatalogueItem> CatalogueItems { get; set; }
        public virtual ICollection<LoadMetadata> LoadMetadatas { get; set; }
        public string LoggingDataTask { get; set; }
        public int? LiveLoggingServer_ID { get; set; }
        public string ValidatorXML { get; set; }
        public int? TimeCoverage_ExtractionInformation_ID { get; set; }
        public int? PivotCategory_ExtractionInformation_ID { get; set; }
        CataloguePeriodicity ICatalogue.Periodicity { get; set; }

        public ExtractionInformation TimeCoverage_ExtractionInformation => throw new NotImplementedException();

        public ExtractionInformation PivotCategory_ExtractionInformation => throw new NotImplementedException();

        public AggregateConfiguration[] AggregateConfigurations => Array.Empty<AggregateConfiguration>();

        public ExternalDatabaseServer LiveLoggingServer => throw new NotImplementedException();

        CatalogueItem[] ICatalogue.CatalogueItems => this.CatalogueItems.ToArray();


        public virtual List<LoadMetadataCatalogueLinkage> LoadMetadataCatalogueLinkages { get; set; }

        AggregateConfiguration[] ICatalogue.AggregateConfigurations => throw new NotImplementedException();

        public override string ToString() => Name;

        public List<CatalogueItem> GetCatalogueItemsForExtractionCategory(ExtractionCategory extractionCategory)
        {
            //todo need to add the extraction cetegory to the db model
            switch (extractionCategory)
            {
                case ExtractionCategory.Core:
                    return CatalogueItems.ToList();//.Where(ci => ci.ex)
                case ExtractionCategory.Supplemental:
                    return CatalogueItems.ToList();//.Where(ci => ci.ex)
                case ExtractionCategory.Deprecated:
                    return CatalogueItems.ToList();//.Where(ci => ci.ex)
                case ExtractionCategory.Internal:
                    return CatalogueItems.ToList();//.Where(ci => ci.ex)
                case ExtractionCategory.SpecialApprovalRequired:
                    return CatalogueItems.ToList();//.Where(ci => ci.ex)
                case ExtractionCategory.ProjectSpecific:
                    return CatalogueItems.ToList();//.Where(ci => ci.ex)
                case ExtractionCategory.Any:
                    return CatalogueItems.ToList();//.Where(ci => ci.ex)
                case ExtractionCategory.NotExtractable:
                    return CatalogueItems.ToList();//.Where(ci => ci.ex)
                default:
                    return CatalogueItems.ToList();
            }
        }

        public ExtractionInformation[] GetAllExtractionInformation(ExtractionCategory category)
        {
            return CatalogueItems.Select(ci => ci.ExtractionInformation).Where(ei => ei.GetExtractionCategory() == category).ToArray();
        }

        public ITableInfo[] GetTableInfoList(bool includeLookupTables)
        {
            throw new NotImplementedException();
        }

        public ITableInfo[] GetLookupTableInfoList()
        {
            throw new NotImplementedException();
        }

        public void GetTableInfos(out List<ITableInfo> normalTables, out List<ITableInfo> lookupTables)
        {
            throw new NotImplementedException();
        }

        public DiscoveredServer GetDistinctLiveDatabaseServer(DataAccessContext context, bool setInitialDatabase, out IDataAccessPoint distinctAccessPoint)
        {
            throw new NotImplementedException();
        }

        public DiscoveredServer GetDistinctLiveDatabaseServer(DataAccessContext context, bool setInitialDatabase)
        {
            throw new NotImplementedException();
        }

        public ITableInfo[] GetTableInfosIdeallyJustFromMainTables()
        {
            throw new NotImplementedException();
        }

        public SupportingSQLTable[] GetAllSupportingSQLTablesForCatalogue(FetchOptions fetch)
        {
            throw new NotImplementedException();
        }

        EntityFramework.Models.ExtractionInformation[] ICatalogue.GetAllExtractionInformation(ExtractionCategory category)
        {
            throw new NotImplementedException();
        }

        public ExtractionInformation[] GetAllExtractionInformation()
        {
            throw new NotImplementedException();
        }

        public SupportingDocument[] GetAllSupportingDocuments(FetchOptions fetch)
        {
            throw new NotImplementedException();
        }

        public ExtractionFilter[] GetAllMandatoryFilters()
        {
            throw new NotImplementedException();
        }

        public ExtractionFilter[] GetAllFilters()
        {
            throw new NotImplementedException();
        }

        public DatabaseType? GetDistinctLiveDatabaseServerType()
        {
            throw new NotImplementedException();
        }

        public CatalogueExtractabilityStatus GetExtractabilityStatus(RDMPDbContext catalogueDbContext)
        {
            throw new NotImplementedException();
        }

        public ICatalogue ShallowClone()
        {
            throw new NotImplementedException();
        }

        public bool IsApiCall()
        {
            throw new NotImplementedException();
        }

        public bool IsApiCall(out IPluginCohortCompiler plugin)
        {
            throw new NotImplementedException();
        }

        public bool IsProjectSpecific(RDMPDbContext catalogueDbContext)
        {
            throw new NotImplementedException();
        }

        public IHasDependencies[] GetObjectsThisDependsOn()
        {
            throw new NotImplementedException();
        }

        public IHasDependencies[] GetObjectsDependingOnThis()
        {
            throw new NotImplementedException();
        }

        public IQuerySyntaxHelper GetQuerySyntaxHelper()
        {
            throw new NotImplementedException();
        }

        public void RevertToDatabaseState()
        {
            throw new NotImplementedException();
        }

        public RevertableObjectReport HasLocalChanges()
        {
            throw new NotImplementedException();
        }

        public bool Exists()
        {
            return true;
            //throw new NotImplementedException();
        }


        public void ClearAllInjections()
        {
            //throw new NotImplementedException();
        }

        public void Check(ICheckNotifier notifier)
        {
            //throw new NotImplementedException();
        }

        internal static bool IsAcceptableName(string name, out object reason)
        {
            throw new NotImplementedException();
        }


        // Enums
        public enum CatalogueType
        {
            Unknown = 0,
            ResearchStudy = 1,
            Cohort = 2,
            NationalRegistry = 3,
            HealthcareProviderRegistry = 4,
            EHRExtract = 5
        }



        public enum CatalogueGranularity
        {
            Unknown = 0,
            National = 1,
            Regional = 2,
            HealthBoard = 3,
            Hospital = 4,
            Clinic = 5
        }

        public enum UpdateFrequencies
        {
            Other = 0,
            Static = 1,
            Irregular = 2,
            Continuous = 3,
            Biennial = 4,
            Annual = 5,
            Biannual = 6,
            Quarterly = 7,
            Bimonthly = 8,
            Monthly = 9,
            Biweekly = 10,
            Weekly = 11,
            TwiceWeekly = 12,
            Daily = 13
        }

        public enum UpdateLagTimes
        {
            Other = 0,
            LessThanAWeek = 1,
            OneToTwoWeeks = 2,
            TwoToFourWeeks = 3,
            OneToTwoMonths = 4,
            TwoToSixMonths = 5,
            SixMonthsPlus = 6,
            Variable = 7,
            NotApplicable = 8
        }

        public enum DatasetPurpose
        {
            Other = 0,
            ResearchCohort = 1,
            Study = 2,
            DiseaseRegistry = 3,
            Trial = 4,
            Care = 5,
            Audit = 6,
            Administrative = 7,
            Financial = 8,
            Statutory = 9
        }
        public enum CataloguePeriodicity
        {
            Unknown = 0,
            Daily = 1,
            Weekly = 2,
            Fortnightly = 3,
            Monthly = 4,
            BiMonthly = 5,
            Quarterly = 6,
            Yearly = 7
        }
        #region Enums

        /// <summary>
        /// Somewhat arbitrary concepts for defining the limitations of a Catalogues data
        /// </summary>

        /// <summary>
        /// Notional user declared period on which the data in the Catalogue is refreshed.  This may not have any bearing
        /// on reality.  Not used by RDMP for any technical processes.
        /// </summary>

        /// <summary>
        /// Notional user declared boundary for the dataset defined by the Catalogue.  The data should be isolated to this Granularity
        /// </summary>

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

    }



}
