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

namespace Rdmp.Core.EntityFramework.Models
{
    [Table("Catalogue")]
    public class Catalogue : DatabaseObject, IHasFolder,ICatalogue
    {
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
        public string Folder { get; set => SetField(ref field, value); } = "/";

        public string Description { get; set => SetField(ref field, value); }

        public string ShortDescription { get; set => SetField(ref field, value); }
        public string Detail_Page_URL { get; set => SetField(ref field, value); }

        [Column(TypeName = "nvarchar(max)")]
        public int? Type { get; set => SetField(ref field, value); }
        [Column(TypeName = "nvarchar(255)")]
        public int? Purpose { get; set => SetField(ref field, value); }
        [Column(TypeName = "nvarchar(50)")]
        public int? Periodicity { get; set => SetField(ref field, value); }
        [Column(TypeName = "nvarchar(max)")]
        public int? Granularity { get; set => SetField(ref field, value); }
        public string Geographical_coverage { get; set => SetField(ref field, value); }
        public string Background_summary { get; set => SetField(ref field, value); }
        public string Search_keywords { get; set => SetField(ref field, value); }
        [Column(TypeName = "nvarchar(50)")]
        public int? Update_freq { get; set => SetField(ref field, value); }
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
        Curation.Data.Catalogue.CataloguePeriodicity ICatalogue.Periodicity { get; set; }

        public Curation.Data.ExtractionInformation TimeCoverage_ExtractionInformation => throw new NotImplementedException();

        public Curation.Data.ExtractionInformation PivotCategory_ExtractionInformation => throw new NotImplementedException();

        public AggregateConfiguration[] AggregateConfigurations => Array.Empty<AggregateConfiguration>();

        public Curation.Data.ExternalDatabaseServer LiveLoggingServer => throw new NotImplementedException();

        CatalogueItem[] ICatalogue.CatalogueItems => this.CatalogueItems.ToArray();

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

        public IEnumerable<ExtractionInformation> GetAllExtractionInformation(ExtractionCategory category)
        {
            return CatalogueItems.Select(ci => ci.ExtractionInformation).Where(ei => ei.GetExtractionCategory() == category);
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

        public void GetTableInfos(ICoreChildProvider provider, out List<ITableInfo> normalTables, out List<ITableInfo> lookupTables)
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

        public Curation.Data.SupportingSQLTable[] GetAllSupportingSQLTablesForCatalogue(FetchOptions fetch)
        {
            throw new NotImplementedException();
        }

        Curation.Data.ExtractionInformation[] ICatalogue.GetAllExtractionInformation(ExtractionCategory category)
        {
            throw new NotImplementedException();
        }

        public Curation.Data.ExtractionInformation[] GetAllExtractionInformation()
        {
            throw new NotImplementedException();
        }

        public Curation.Data.SupportingDocument[] GetAllSupportingDocuments(FetchOptions fetch)
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

        public void SaveToDatabase()
        {
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
    }


  
}
