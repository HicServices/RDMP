using MongoDB.Driver;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.EntityFramework;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Revertable;
using Rdmp.Core.ReusableLibraryCode.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace Rdmp.Core.Models
{
    [Table("Catalogue")]
    public class Catalogue : IMapsDirectlyToDatabaseTable, IHasFolder
    {
        [Key]
        public int ID { get; set; }

        [MaxLength(100)]
        public string Acronym { get; set; }

        [Required]
        [MaxLength(500)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Folder { get; set; } = "/";

        public string Description { get; set; }

        public string ShortDescription { get; set => SetField(ref ShortDescription, value); }
        public string Detail_Page_URL { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public int? Type { get; set; }
        [Column(TypeName = "nvarchar(255)")]
        public int? Purpose { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public int? Periodicity { get; set; }
        [Column(TypeName = "nvarchar(max)")]
        public int? Granularity { get; set; }
        public string Geographical_coverage { get; set; }
        public string Background_summary { get; set; }
        public string Search_keywords { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public int? Update_freq { get; set; }
        public string Update_sched { get; set; }
        public string Time_coverage { get; set; }
        public DateTime? Last_revision_date { get; set; }
        public string Contact_details { get; set; }
        public string Resource_owner { get; set; }
        public string Attribution_citation { get; set; }
        public string Access_options { get; set; }
        public string SubjectNumbers { get; set; }
        public string API_access_URL { get; set; }
        public string Browse_URL { get; set; }
        public string Bulk_Download_URL { get; set; }
        public string Query_tool_URL { get; set; }
        public string Source_URL { get; set; }
        public string Country_of_origin { get; set; }
        public string Data_standards { get; set; }
        public string Administrative_contact_name { get; set; }
        public string Administrative_contact_email { get; set; }
        public string Administrative_contact_telephone { get; set; }
        public string Administrative_contact_address { get; set; }
        public bool? Explicit_consent { get; set; }
        public string Ethics_approver { get; set; }
        public string Source_of_data_collection { get; set; }
        public string Ticket { get; set; }
        public DateTime? DatasetStartDate { get; set; }
        public string DataType { get; set; }
        public string DataSubType { get; set; }
        public string DataSource { get; set; }
        public string DataSourceSetting { get; set; }
        public DateTime? DatasetReleaseDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        [Column(TypeName = "nvarchar(255)")]

        public int? UpdateLag { get; set; }
        public string Juristiction { get; set; }
        public string DataController { get; set; }
        public string DataProcessor { get; set; }
        public string ControlledVocabulary { get; set; }
        public string AssociatedPeople { get; set; }
        public string AssociatedMedia { get; set; }
        public string Doi { get; set; }
        public bool IsDeprecated { get; set; }
        public bool IsInternalDataset { get; set; }

        public virtual ICollection<CatalogueItem> CatalogueItems { get; set; }
        public virtual ICollection<LoadMetadata> LoadMetadatas { get; set; }

        [NotMapped]
        public RDMPDbContext CatalogueDbContext { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(object oldValue, object newValue,
    [CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedExtendedEventArgs(propertyName, oldValue, newValue));
        }

        public override string ToString() => Name;

        private void SetField<T>(ref T field, T value)
        {
            OnPropertyChanged(field, value, null);
            field = value;
        }
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
}
