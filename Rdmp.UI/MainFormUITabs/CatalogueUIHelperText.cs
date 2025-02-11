using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.UI.MainFormUITabs
{
    public static class CatalogueUIHelperText
    {
        public static readonly string Acronym = "A shorthand name for the catalogue.";
        public static readonly string ShortDescription = """
            The Short Description should provide a clear and brief descriptive signpost for researchers who are searching for data that may be relevant to their research.
            The Short Description should allow the reader to determine the scope of the data collection and accurately summarise its content.
            Effective Short Descriptions should avoid long sentences and abbreviations where possible.
            Note: Researchers will view Titles and the first line of Short Descriptions (list view) when searching for datasets and choosing whether to explore their content further.
            Short Descriptions should be different from the full description for a dataset.
            Example: CPRD Aurum contains primary care data contributed by General Practitioner (GP) practices using EMIS Web® including patient registration information and all care events that GPs have chosen to record as part of their usual medical practice.
            """;
        public static readonly string Description = """
            An HTML account of the data that provides context and scope of the data, limited to 10000 characters, and/or a resolvable URL that describes the dataset.
            Additional information can be recorded and included using the Associated media field.
            """;
        public static readonly string ResourceType = """
            Research Study: Data relating to a specific Research Study
            Cohort: Data relating to a specific Cohort
            National Registry: Data relating to a National Registry
            Healthcare Provider Registry: Data relating to a Healthcare Provider
            EHR Extract: Data relating to a single EHR Extract
            Other: Data relating to some other resource
            """;
        public static readonly string DatasetType = """
            Types include those listed below. Datasets can have more than one type associated.
            Health and disease: Includes any data related to mental health, cardiovascular, cancer, rare diseases, metabolic and endocrine, neurological, reproductive, maternity and neonatology, respiratory, immunity, musculoskeletal, vision, renal and urogenital, oral and gastrointestinal, cognitive function or hearing.
            Treatments/Interventions: Includes any data related to treatment or interventions related to vaccines or which are preventative or therapeutic in nature.
            Measurements/Tests: Includes any data related to laboratory or other diagnostics.
            Imaging types: Includes any data related to CT, MRI, PET, x-ray, ultrasound or pathology imaging.
            Imaging area of the body: Indicates whether the dataset relates to head, chest, arm abdomen or leg imaging.
            Omics: Includes any data related to proteomics, transcriptomics, epigenomics, metabolomics, multiomics, metagenomics or genomics.
            Socioeconomic: Includes any data related to education, crime and justice, ethnicity, housing, labour, ageing, economics, marital status, social support, deprivation, religion, occupation, finances or family circumstances.
            Lifestyle: Includes any data related to smoking, physical activity, dietary habits or alcohol.
            Registry: Includes any data related to disease registries for research, national disease registries, audits, or birth and deaths records.
            Environment and energy: Includes any data related to the monitoring or study of environmental or energy factors or events.
            Information and communication: Includes any data related to the study or application of information and communication.
            Politics: Includes any data related to political views, activities, voting, etc.
            """;
        public static readonly string DatasetSubtype = """
            Mental health: 
            Cardiovascular: 
            Cancer: 
            Rare diseases: 
            Metabolic and endocrine: 
            Neurological: 
            Reproductive: 
            Maternity and neonatology: 
            Respiratory: 
            Immunity: 
            Musculoskeletal: 
            Vision: 
            Renal and urogenital: 
            Oral and gastrointestinal: 
            Cognitive function: 
            Hearing: 
            Others: 
            Vaccines: 
            Preventive: 
            Therapeutic: 
            Laboratory: 
            Other diagnostics: 
            CT: 
            MRI: 
            PET: 
            X-ray: 
            Ultrasound: 
            Pathology: 
            Head: 
            Chest: 
            Arm: 
            Abdomen: 
            Leg: 
            Proteomics: 
            Transcriptomics: 
            Epigenomics: 
            Metabolomics: 
            Metagenomics: 
            Genomics: 
            Lipidomics: 
            Education: 
            Crime and justice: 
            Ethnicity: 
            Housing: 
            Labour: 
            Ageing : 
            Economics: 
            Marital status: 
            Social support: 
            Deprivation: 
            Religion: 
            Occupation: 
            Finances: 
            Family circumstance: 
            Smoking: 
            Physical activity: 
            Dietary habits: 
            Alcohol: 
            Disease registry (research): 
            National disease registries and audits: 
            Births and deaths: 
            Not applicable: 
            """;
        public static readonly string DataSource = """
            EPR: Data Extracted from Electronic Patient Record.
            Electronic survey: Data has been extracted from electronic surveys.
            LIMS: Data has been extracted from a laboratory information management system.
            Paper-based: Data has been extracted from paper forms.
            Free text NLP: Data has been extracted from unstructured freetext using natural language processing.
            Machine generated: Data has been machine generated i.e. imaging.
            Other: Data has been extracted by other means.
            """;
        public static readonly string DataSourceSetting = """
            Cohort, study, trial: Cohort, study or trial data collection as part of protocol.
            Clinic: Specific clinic such as antenatal clinic.
            Primary care - Referrals: General medical practitioner referral to another service.
            Primary care - Clinic: General medical practitioner practice.
            Primary care - Out of hours: General medical practitioner care or advice outside of standard hours.
            Secondary care - Accident and emergency: Accident emergency department.
            Secondary care - Outpatients: Outpatient care.
            Secondary care - In-patients: In-patient care.
            Secondary care - Ambulance: Care provided in association with ambulance service.
            Secondary care - ICU: Intensive care units, also referred to as critical care units (CCUs) or intensive therapy units (ITUs).
            Prescribing - Community pharmacy: Pharmacy based in the community.
            Prescribing - Community pharmacy: Pharmacy based in a hospital setting.
            Patient report outcome: Reported by patient.
            Wearables: Data collection devices worn on the body.
            Local authority: Local authority or entity associated with a local authority.
            National government: National government or entity associated with the national government.
            Community: Community settings.
            Services: Services such as drug misuse or blood transfusion.
            Home: Home setting.
            Private: Private medical clinic.
            Social care - Health care at home: service provided in the home or residence of a person.
            Social care - Other social data: service provided in a setting outside of the person's home or residence.
            Census: collected as part of census.
            Other: Other setting.
            """;
        public static readonly string Keywords = """
            Please provide relevant and specific keywords that can improve the search engine optimization of your dataset.
            Please enter one keyword at a time and click Add New Field to add further keywords.
            Text from the title is automatically included in the search, there is no need to include this in the keywords.
            Include words that researcher may include in their searches.
            """;
        public static readonly string PurposeOfDataset = """
            Research cohort: Data collected for a defined group of people.
            Study: Data collected for a specific research study.
            Disease registry: Data collected as part of a disease registry.
            Trial: Data collected for as part of a clinical trial.
            Care: Data collected as part of routine clinical care.
            Audit: Data collected as part of an audit programme.
            Administrative: Data collected for administrative and management information purposes.
            Financial: Data collected either for payments or for billing.
            Statutory: Data collected in compliance with statutory requirements.
            Other: Data collected for other purpose.
            """;
        public static readonly string GeographicalCoverage = """
            The geographical area covered by the dataset.
            Please provide a valid location.
            For locations in the UK, this location should conform to ONS standards.
            For locations in other countries we use ISO 3166-1 & ISO 3166-2.
            """;
        public static readonly string Granularity = """
            National: Catalogue contains national level data
            Regional: Catalogue contains data for a specific region
            HealthBoard: Catalogue contains data for a specific Healthboard
            Clinic: Catalogue contains data for a specific clinic
            Other:Catalogue contains some other level of granularity
            """;
        public static readonly string StartDate = "The earliest date found in the catalogue.";
        public static readonly string EndDate = "The latest date found in the catalogue.";
        public static readonly string AccessContact = """
            Organisations are expected to provide a dedicated email address associated with the data access request process.
            If no contact point is provided in this field, this field will be defaulted to the teams support email provided in the teams setting.
            Note: An employee's email address can only be provided on a temporary basis and if one is provided, you must obtain explicit consent for this purpose.
            """;
        public static readonly string DataController = """
            Data Controller means a person/entity who (either alone or jointly or in common with other persons/entities) determines the purposes for which and the way any Data Subject data, specifically personal data or are to be processed.
            Notes: For most organisations this will be the same as the Data Custodian of the dataset. If this is not the case, please indicate that there is a different controller.
            If there is a different controller please complete the Data Processor attribute to indicate if the Data Custodian is a Processor rather than the Data Controller.
            In some cases, there may be multiple Data Controllers i.e. GP data. If this is the case, please indicate the fact in a free-text field and describe the data sharing arrangement or a link to it, so that this can be understood by research users.
            Example: NHS England
            """;
        public static readonly string DataProcessor = """
            Notes: Required to complete if the Data Custodian is the Data Processor rather than the Data Controller.
            If the Publisher is also the Data Controller please provide “Not Applicable”.
            Examples: Not Applicable, SAIL
            """;
        public static readonly string Juristiction = """A full list of country codes can be found here (alpha-2 column): https://www.iso.org/obp/ui/#search/code/""";
        public static readonly string DOI = "Please note: This is not the DOI of the publication(s) associated with the dataset.";
        public static readonly string ControlledVocabulary = "List any relevant terminologies / ontologies / controlled vocabularies, such as ICD 10 Codes, NHS Data Dictionary National Codes or SNOMED CT International, that are being used by the dataset.";
        public static readonly string People = "Please list the email addresses of any people/organisations associated with this catalogue.";
        public static readonly string UpdateFrequency = """
            Static: Dataset published once.
            Irregular: Dataset published at uneven intervals.
            Continuous: Dataset published without interruption.
            Biennial: Dataset published every two years.
            Annual: Dataset published occurs once a year.
            Biannual: Dataset published twice a year.
            Quarterly: Dataset published every three months.
            Bimonthly: Dataset published every two months.
            Monthly: Dataset published once a month.
            Biweekly: Dataset published every two weeks.
            Weekly: Dataset published once a week.
            Twice weekly: Dataset published twice a week.
            Daily: Dataset published once a day.
            Other: Dataset published using other interval.
            """;
        public static readonly string InitialReleaseDate = "The date the catalogue was initially made available for research use.";
        public static readonly string UpdateLag = """
            Less than 1 week: Typical time lag of less than a week.
            1-2 weeks: Typical time-lag of one to two weeks.
            2-4 weeks: Typical time-lag of two to four weeks.
            1-2 months: Typical time-lag of one to two months.
            2-6 months: Typical time-lag of two to six months.
            6 months plus: Typical time-lag of more than six months.
            Variable: Variable time-lag.
            Not applicable: Not Applicable i.e. static dataset.
            Other: Other time-lag.
            """;
        public static readonly string AssociatedMedia = """
            Please provide any media associated with the Gateway Organisation using a valid URL for the content.
            This is an opportunity to provide additional context that could be useful for researchers wanting to understand more about the dataset and its relevance to their research question.
            Note: media assets should be hosted by the organisation.
            Example: This could be a link to a PDF Document that describes methodology or further detail about the datasets, or a graph or chart that provides further context about the dataset.
            If you are providing multiple links for associated media, we recommend that you separate these with a comma.
            """;
    }
}
