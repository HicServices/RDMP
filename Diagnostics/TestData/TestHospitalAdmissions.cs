using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnostics.TestData
{
    public class TestHospitalAdmissions
    {
        private readonly TestDemography _demography;
        private readonly Random _r;

        public const string DatasetDescription = "This is a test dataset created by the DataManagementPlatform User Acceptance Test it is meant to bear some resemblance to SMR01 in that it contains denormalised conditions (1-4) of which a person may have between 1 and 4 populated.  Although this is bad database design (the correct relational mapping would be a 1 to m relationship) it is the way SMR01 is so it is a good test dataset.";


        public const string LookupTableName = "z_TestICD10Lookup";
        public const string HospitalAdmissionsTableName = "TestHospitalAdmissions";

        private Dictionary<TestPerson, List<TestAdmission>> _admissionsDictionary = new Dictionary<TestPerson, List<TestAdmission>>();

        public TestHospitalAdmissions(TestDemography demography,Random r)
        {
            _demography = demography;
            _r = r;
            SetupICD10Dictionary();
            SetupHospitalAdmissions();

        }

        private void SetupHospitalAdmissions()
        {
            foreach (TestPerson person in _demography.People)
            {
                _admissionsDictionary.Add(person, new List<TestAdmission>());

                //this should produce a bell curve since r.Next is called for every iteration of the for loop 
                for (int i = 0; i < _r.Next(100); i++)
                    _admissionsDictionary[person].Add(new TestAdmission(this, person, person.DateOfBirth, _r));
            }

        }

        public string GetCreateHospitalAdmissions(bool anonymousVersion)
        {
            return
                @"/*Drop table if it already exists */
               IF OBJECT_ID('" + HospitalAdmissionsTableName + @"') IS NOT NULL
 DROP TABLE " + HospitalAdmissionsTableName + @"

CREATE TABLE " + HospitalAdmissionsTableName + @"
(
AdmissionDate datetime NOT NULL,
DischargeDate datetime NOT NULL,
Condition1 varchar(4) NOT NULL,
Condition2 varchar(4) NULL,
Condition3 varchar(4) NULL,
Condition4 varchar(4) NULL,
" + (anonymousVersion ? "ANOCHI varchar(12)" : "CHI varchar(10)") + @"
)

GO
              IF OBJECT_ID('" + HospitalAdmissionsTableName + @"_Archive') IS NOT NULL
 DROP TABLE " + HospitalAdmissionsTableName + @"_Archive
GO
";
        }

        public string GetINSERTIntoHospitalAdmissions(bool createANOVersion)
        {
            StringBuilder builder = new StringBuilder();

            foreach (KeyValuePair<TestPerson, List<TestAdmission>> kvp in _admissionsDictionary)
            {
                var person = kvp.Key;
                foreach (TestAdmission admission in kvp.Value)
                {
                    builder.Append("INSERT INTO " + HospitalAdmissionsTableName + "(");
                    builder.Append(createANOVersion ? "ANOCHI" : "CHI");
                    builder.Append(",AdmissionDate,DischargeDate,Condition1,Condition2,Condition3,Condition4) VALUES ('");

                    builder.Append(createANOVersion ? person.ANOCHI : person.CHI);
                    builder.Append(string.Format("','{0}','{1}','{2}',{3},{4},{5}",
                        admission.AdmissionDate.ToString("yyyy-MM-dd"),
                        admission.DischargeDate.ToString("yyyy-MM-dd"),
                        admission.Condition1,
                        admission.Condition2 == null ? "NULL": "'"+admission.Condition2+"'", //some don't have these
                        admission.Condition3 == null ? "NULL" : "'" + admission.Condition3 + "'",

                        admission.Condition4 == null ? "NULL" : "'" + admission.Condition4 + "'"));

                    builder.AppendLine(")");
                }
            }

            return builder.ToString();
        }

        public string GetCreateLookupsSql()
        {
            return 
                       @"/*Drop table if it already exists */
               IF OBJECT_ID('" + LookupTableName + @"') IS NOT NULL
 DROP TABLE " + LookupTableName + @"

CREATE TABLE " + LookupTableName + @"
(
Code [varchar](4) PRIMARY KEY,
Description [varchar](500) NULL
)

GO
              IF OBJECT_ID('" + LookupTableName + @"_Archive') IS NOT NULL
 DROP TABLE " + LookupTableName + @"_Archive
GO
";
        }

        public string GetINSERTLookupsSql()
        {
            StringBuilder sb = new StringBuilder();

            foreach (KeyValuePair<string, string> kvp in ICDCodes)
                sb.AppendLine("INSERT INTO " + LookupTableName + " VALUES ('" + kvp.Key + "','" + kvp.Value + "')");

            return sb.ToString();
        }

        private void SetupICD10Dictionary()
        {
            ICDCodes.Add("R891", "Abnormal findings in specimens from other organs systems and tissues: Abnormal level of hormones ");
            ICDCodes.Add("M405", "Lordosis unspecified ");
            ICDCodes.Add("Q844", "Congenital leukonychia ");
            ICDCodes.Add("H311", "Choroidal degeneration ");
            ICDCodes.Add("Y575", "X-ray contrast media ");
            ICDCodes.Add("W66", "Drowning and submersion following fall into bath-tub ");
            ICDCodes.Add("M212", "Flexion deformity ");
            ICDCodes.Add("S599", "Unspecified injury of forearm ");
            ICDCodes.Add("P71", "Transitory neonatal disorders of calcium and magnesium metabolism ");
            ICDCodes.Add("T269", "Corrosion of eye and adnexa part unspecified ");
            ICDCodes.Add("D739", "Disease of spleen unspecified ");
            ICDCodes.Add("G000", "Haemophilus meningitis ");
            ICDCodes.Add("Y523", "Coronary vasodilators not elsewhere classified ");
            ICDCodes.Add("T424", "Poisoning: Benzodiazepines ");
            ICDCodes.Add("T792", "Traumatic secondary and recurrent haemorrhage ");
            ICDCodes.Add("K052", "Acute periodontitis ");
            ICDCodes.Add("Z567", "Other and unspecified problems related to employment ");
            ICDCodes.Add("F530", "Mild mental and behavioural disorders associated with the puerperium not elsewhere classified ");
            ICDCodes.Add("P006", "Fetus and newborn affected by surgical procedure on mother ");
            ICDCodes.Add("M429", "Spinal osteochondrosis unspecified ");
            ICDCodes.Add("R103", "Pain localized to other parts of lower abdomen ");
            ICDCodes.Add("A029", "Salmonella infection unspecified ");
            ICDCodes.Add("F721", "Severe mental retardation: Significant impairment of behaviour requiring attention or treatment ");
            ICDCodes.Add("N498", "Inflammatory disorders of other specified male genital organs ");
            ICDCodes.Add("Z960", "Presence of urogenital implants ");
            ICDCodes.Add("V756", "Bus occupant injured in collision with railway train or railway vehicle: Passenger injured in traffic accident ");
            ICDCodes.Add("F179", "Mental and behavioural disorders due to use of tobacco: Unspecified mental and behavioural disorder ");
            ICDCodes.Add("A060", "Acute amoebic dysentery ");
            ICDCodes.Add("H80", "Otosclerosis ");
            ICDCodes.Add("Q048", "Other specified congenital malformations of brain ");
            ICDCodes.Add("P70", "Transitory disorders of carbohydrate metabolism specific to fetus and newborn ");
            ICDCodes.Add("I512", "Rupture of papillary muscle not elsewhere classified ");
            ICDCodes.Add("V698", "Occupant [any] of heavy transport vehicle injured in other specified transport accidents ");
            ICDCodes.Add("C22", "Malignant neoplasm of liver and intrahepatic bile ducts ");
            ICDCodes.Add("L54", "Erythema in diseases classified elsewhere ");
            ICDCodes.Add("E31", "Polyglandular dysfunction ");
            ICDCodes.Add("M862", "Subacute osteomyelitis ");
            ICDCodes.Add("O72", "Postpartum haemorrhage ");
            ICDCodes.Add("T47", "Poisoning by agents primarily affecting the gastrointestinal system ");
            ICDCodes.Add("K59", "Other functional intestinal disorders ");
            ICDCodes.Add("Q666", "Other congenital valgus deformities of feet ");
            ICDCodes.Add("I71", "Aortic aneurysm and dissection ");
            ICDCodes.Add("T301", "Burn of first degree body region unspecified ");
            ICDCodes.Add("C439", "Malignant neoplasm: Malignant melanoma of skin unspecified ");
            ICDCodes.Add("D023", "Carcinoma in situ: Other parts of respiratory system ");
            ICDCodes.Add("S830", "Dislocation of patella ");
            ICDCodes.Add("D571", "Sickle-cell anaemia without crisis ");
            ICDCodes.Add("A09", "Other gastroenteritis and colitis of infectious and unspecified origin ");
            ICDCodes.Add("M190", "Primary arthrosis of other joints ");
            ICDCodes.Add("Q899", "Congenital malformation unspecified ");
            ICDCodes.Add("Y581", "Typhoid and paratyphoid vaccine ");
            ICDCodes.Add("T222", "Burn of second degree of shoulder and upper limb except wrist and hand ");
            ICDCodes.Add("I151", "Hypertension secondary to other renal disorders ");
            ICDCodes.Add("N060", "Isolated proteinuria with specified morphological lesion: Minor glomerular abnormality ");
            ICDCodes.Add("V862", "Person on outside of all-terrain or other off-road motor vehicle injured in traffic accident ");
            ICDCodes.Add("T781", "Other adverse food reactions not elsewhere classified ");
            ICDCodes.Add("Z84", "Family history of other conditions ");
            ICDCodes.Add("G722", "Myopathy due to other toxic agents ");
            ICDCodes.Add("Z914", "Personal history of psychological trauma not elsewhere classified ");
            ICDCodes.Add("D357", "Benign neoplasm: Other specified endocrine glands ");
            ICDCodes.Add("T609", "Toxic effect: Pesticide unspecified ");
            ICDCodes.Add("G933", "Postviral fatigue syndrome ");
            ICDCodes.Add("E169", "Disorder of pancreatic internal secretion unspecified ");
            ICDCodes.Add("C169", "Malignant neoplasm: Stomach unspecified ");
            ICDCodes.Add("N973", "Female infertility of cervical origin ");
            ICDCodes.Add("V416", "Car occupant injured in collision with pedal cycle: Passenger injured in traffic accident ");
            ICDCodes.Add("Q500", "Congenital absence of ovary ");
            ICDCodes.Add("M943", "Chondrolysis ");
            ICDCodes.Add("Q230", "Congenital stenosis of aortic valve ");
            ICDCodes.Add("D293", "Benign neoplasm: Epididymis ");
            ICDCodes.Add("J050", "Acute obstructive laryngitis [croup] ");
            ICDCodes.Add("B340", "Adenovirus infection unspecified site ");
            ICDCodes.Add("K765", "Hepatic veno-occlusive disease ");
            ICDCodes.Add("N051", "Unspecified nephritic syndrome: Focal and segmental glomerular lesions ");
            ICDCodes.Add("K870", "Disorders of gallbladder and biliary tract in diseases classified elsewhere ");
            ICDCodes.Add("V493", "Car occupant [any] injured in unspecified nontraffic accident ");
            ICDCodes.Add("K731", "Chronic lobular hepatitis not elsewhere classified ");
            ICDCodes.Add("D380", "Neoplasm of uncertain or unknown behaviour: Larynx ");
            ICDCodes.Add("S40", "Superficial injury of shoulder and upper arm ");
            ICDCodes.Add("Q560", "Hermaphroditism not elsewhere classified ");
            ICDCodes.Add("I602", "Subarachnoid haemorrhage from anterior communicating artery ");
            ICDCodes.Add("F321", "Moderate depressive episode ");
            ICDCodes.Add("M364", "Arthropathy in hypersensitivity reactions classified elsewhere ");
            ICDCodes.Add("O621", "Secondary uterine inertia ");
            ICDCodes.Add("A361", "Nasopharyngeal diphtheria ");
            ICDCodes.Add("E662", "Extreme obesity with alveolar hypoventilation ");
            ICDCodes.Add("S453", "Injury of superficial vein at shoulder and upper arm level ");
            ICDCodes.Add("T206", "Corrosion of second degree of head and neck ");
            ICDCodes.Add("M713", "Other bursal cyst ");
            ICDCodes.Add("V569", "Occupant of pick-up truck or van injured in collision with other nonmotor vehicle: Unspecified occupant of pick-up truck or van injured in traffic accident ");
            ICDCodes.Add("K383", "Fistula of appendix ");
            ICDCodes.Add("E512", "Wernicke encephalopathy ");
            ICDCodes.Add("P024", "Fetus and newborn affected by prolapsed cord ");
            ICDCodes.Add("N028", "Recurrent and persistent haematuria: Other ");
            ICDCodes.Add("M839", "Adult osteomalacia unspecified ");
            ICDCodes.Add("T020", "Fractures involving head with neck ");
            ICDCodes.Add("S61", "Open wound of wrist and hand ");
            ICDCodes.Add("J211", "Acute bronchiolitis due to human metapneumovirus ");
            ICDCodes.Add("L570", "Actinic keratosis ");
            ICDCodes.Add("C438", "Malignant neoplasm: Overlapping malignant melanoma of skin ");
            ICDCodes.Add("T423", "Poisoning: Barbiturates ");
            ICDCodes.Add("B602", "Naegleriasis ");
            ICDCodes.Add("D213", "Benign neoplasm: Connective and other soft tissue of thorax ");
            ICDCodes.Add("M905", "Osteonecrosis in other diseases classified elsewhere ");
            ICDCodes.Add("E798", "Other disorders of purine and pyrimidine metabolism ");
            ICDCodes.Add("T141", "Open wound of unspecified body region ");
            ICDCodes.Add("X93", "Assault by handgun discharge ");
            ICDCodes.Add("N25", "Disorders resulting from impaired renal tubular function ");
            ICDCodes.Add("N338", "Bladder disorders in other diseases classified elsewhere ");
            ICDCodes.Add("G833", "Monoplegia unspecified ");
            ICDCodes.Add("M601", "Interstitial myositis ");
            ICDCodes.Add("K745", "Biliary cirrhosis unspecified ");
            ICDCodes.Add("L981", "Factitial dermatitis ");
            ICDCodes.Add("I724", "Aneurysm and dissection of artery of lower extremity ");
            ICDCodes.Add("H403", "Glaucoma secondary to eye trauma ");
            ICDCodes.Add("N151", "Renal and perinephric abscess ");
            ICDCodes.Add("E44", "Protein-energy malnutrition of moderate and mild degree ");
            ICDCodes.Add("Z631", "Problems in relationship with parents and in-laws ");
            ICDCodes.Add("Q319", "Congenital malformation of larynx unspecified ");
            ICDCodes.Add("Z549", "Convalescence following unspecified treatment ");
            ICDCodes.Add("W28", "Contact with powered lawnmower ");
            ICDCodes.Add("A932", "Colorado tick fever ");
            ICDCodes.Add("Y77", "Ophthalmic devices associated with adverse incidents ");
            ICDCodes.Add("Q105", "Congenital stenosis and stricture of lacrimal duct ");
            ICDCodes.Add("J173", "Pneumonia in parasitic diseases ");
            ICDCodes.Add("V501", "Occupant of pick-up truck or van injured in collision with pedestrian or animal: Passenger injured in nontraffic accident ");
            ICDCodes.Add("V719", "Bus occupant injured in collision with pedal cycle: Unspecified bus occupant injured in traffic accident ");
            ICDCodes.Add("B973", "Retrovirus as the cause of diseases classified to other chapters ");
            ICDCodes.Add("K45", "Other abdominal hernia ");
            ICDCodes.Add("O242", "Diabetes mellitus in pregnancy: Pre-existing malnutrition-related diabetes mellitus ");
            ICDCodes.Add("X03", "Exposure to controlled fire not in building or structure ");
            ICDCodes.Add("J13", "Pneumonia due to Streptococcus pneumoniae ");
            ICDCodes.Add("V782", "Bus occupant injured in noncollision transport accident: Person on outside of vehicle injured in nontraffic accident ");
            ICDCodes.Add("T604", "Toxic effect: Rodenticides ");
            ICDCodes.Add("S340", "Concussion and oedema of lumbar spinal cord ");
            ICDCodes.Add("T809", "Unspecified complication following infusion transfusion and therapeutic injection ");
            ICDCodes.Add("F89", "Unspecified disorder of psychological development ");
            ICDCodes.Add("K358", "Acute appendicitis other and unspecified ");
            ICDCodes.Add("E358", "Disorders of other endocrine glands in diseases classified elsewhere ");
            ICDCodes.Add("S540", "Injury of ulnar nerve at forearm level ");
            ICDCodes.Add("V37", "Occupant of three-wheeled motor vehicle injured in collision with fixed or stationary object ");
            ICDCodes.Add("R194", "Change in bowel habit ");
            ICDCodes.Add("Y445", "Thrombolytic drugs ");
            ICDCodes.Add("Z638", "Other specified problems related to primary support group ");
            ICDCodes.Add("T054", "Traumatic amputation of one foot and other leg [any level except foot] ");
            ICDCodes.Add("F50", "Eating disorders ");
            ICDCodes.Add("P102", "Intraventricular haemorrhage due to birth injury ");
            ICDCodes.Add("H490", "Third [oculomotor] nerve palsy ");
            ICDCodes.Add("E25", "Adrenogenital disorders ");
            ICDCodes.Add("A009", "Cholera unspecified ");
            ICDCodes.Add("G710", "Muscular dystrophy ");
            ICDCodes.Add("R300", "Dysuria ");
            ICDCodes.Add("Q824", "Ectodermal dysplasia (anhidrotic) ");
            ICDCodes.Add("V179", "Pedal cyclist injured in collision with fixed or stationary object: Unspecified pedal cyclist injured in traffic accident ");
            ICDCodes.Add("K650", "Acute peritonitis ");
            ICDCodes.Add("R860", "Abnormal findings in specimens from male genital organs: Abnormal level of enzymes ");
            ICDCodes.Add("E03", "Other hypothyroidism ");
            ICDCodes.Add("T360", "Poisoning: Penicillins ");
            ICDCodes.Add("G040", "Acute disseminated encephalitis ");
            ICDCodes.Add("O618", "Other failed induction of labour ");
            ICDCodes.Add("Z61", "Problems related to negative life events in childhood ");
            ICDCodes.Add("G009", "Bacterial meningitis unspecified ");
            ICDCodes.Add("Z304", "Surveillance of contraceptive drugs ");
            ICDCodes.Add("B69", "Cysticercosis ");
            ICDCodes.Add("M259", "Joint disorder unspecified ");
            ICDCodes.Add("R56", "Convulsions not elsewhere classified ");
            ICDCodes.Add("O982", "Gonorrhoea complicating pregnancy childbirth and the puerperium ");
            ICDCodes.Add("K830", "Cholangitis ");
            ICDCodes.Add("M248", "Other specific joint derangements not elsewhere classified ");
            ICDCodes.Add("E320", "Persistent hyperplasia of thymus ");
            ICDCodes.Add("O652", "Obstructed labour due to pelvic inlet contraction ");
            ICDCodes.Add("Q370", "Cleft hard palate with bilateral cleft lip ");
            ICDCodes.Add("F432", "Adjustment disorders ");
            ICDCodes.Add("J398", "Other specified diseases of upper respiratory tract ");
            ICDCodes.Add("H904", "Sensorineural hearing loss unilateral with unrestricted hearing on the contralateral side ");
            ICDCodes.Add("Z008", "Other general examinations ");
            ICDCodes.Add("O03", "Spontaneous abortion ");
            ICDCodes.Add("V73", "Bus occupant injured in collision with car pick-up truck or van ");
            ICDCodes.Add("I829", "Embolism and thrombosis of unspecified vein ");
            ICDCodes.Add("T82", "Complications of cardiac and vascular prosthetic devices implants and grafts ");
            ICDCodes.Add("V254", "Motorcycle rider injured in collision with railway train or railway vehicle: Driver injured in traffic accident ");
            ICDCodes.Add("R848", "Abnormal findings in specimens from respiratory organs and thorax: Other abnormal findings ");
            ICDCodes.Add("C608", "Malignant neoplasm: Overlapping lesion of penis ");
            ICDCodes.Add("X99", "Assault by sharp object ");
            ICDCodes.Add("Q172", "Microtia ");
            ICDCodes.Add("D059", "Carcinoma in situ of breast unspecified ");
            ICDCodes.Add("Q341", "Congenital cyst of mediastinum ");
            ICDCodes.Add("Y539", "Agent primarily affecting the gastrointestinal system unspecified ");
            ICDCodes.Add("I803", "Phlebitis and thrombophlebitis of lower extremities unspecified ");
            ICDCodes.Add("B872", "Ocular myiasis ");
            ICDCodes.Add("L539", "Erythematous condition unspecified ");
            ICDCodes.Add("T233", "Burn of third degree of wrist and hand ");
            ICDCodes.Add("Q374", "Cleft hard and soft palate with bilateral cleft lip ");
            ICDCodes.Add("O868", "Other specified puerperal infections ");
            ICDCodes.Add("S636", "Sprain and strain of finger(s) ");
            ICDCodes.Add("O048", "Medical abortion: Complete or unspecified with other and unspecified complications ");
            ICDCodes.Add("V921", "Water-transport-related drowning and submersion without accident to watercraft: Passenger ship ");
            ICDCodes.Add("R870", "Abnormal findings in specimens from female genital organs: Abnormal level of enzymes ");
            ICDCodes.Add("B871", "Wound myiasis ");
            ICDCodes.Add("T221", "Burn of first degree of shoulder and upper limb except wrist and hand ");
            ICDCodes.Add("V532", "Occupant of pick-up truck or van injured in collision with car pick-up truck or van: Person on outside of vehicle injured in nontraffic accident ");
            ICDCodes.Add("T013", "Open wounds involving multiple regions of lower limb(s) ");
            ICDCodes.Add("Z998", "Dependence on other enabling machines and devices ");
            ICDCodes.Add("O037", "Spontaneous abortion: Complete or unspecified complicated by embolism ");
            ICDCodes.Add("P269", "Unspecified pulmonary haemorrhage originating in the perinatal period ");
            ICDCodes.Add("K621", "Rectal polyp ");
            ICDCodes.Add("Z313", "Other assisted fertilization methods ");
            ICDCodes.Add("A749", "Chlamydial infection unspecified ");
            ICDCodes.Add("D432", "Neoplasm of uncertain or unknown behaviour: Brain unspecified ");
            ICDCodes.Add("K089", "Disorder of teeth and supporting structures unspecified ");
            ICDCodes.Add("N009", "Acute nephritic syndrome: Unspecified ");
            ICDCodes.Add("L759", "Apocrine sweat disorder unspecified ");
            ICDCodes.Add("O072", "Failed medical abortion complicated by embolism ");
            ICDCodes.Add("V346", "Occupant of three-wheeled motor vehicle injured in collision with heavy transport vehicle or bus: Passenger injured in traffic accident ");
            ICDCodes.Add("Y813", "General- and plastic-surgery devices associated with adverse incidents: Surgical instruments materials and devices (including sutures) ");
            ICDCodes.Add("E671", "Hypercarotenaemia ");
            ICDCodes.Add("J303", "Other allergic rhinitis ");
            ICDCodes.Add("K073", "Anomalies of tooth position ");
            ICDCodes.Add("R060", "Dyspnoea ");
            ICDCodes.Add("V393", "Occupant [any] of three-wheeled motor vehicle injured in unspecified nontraffic accident ");
            ICDCodes.Add("C695", "Malignant neoplasm: Lacrimal gland and duct ");
            ICDCodes.Add("K068", "Other specified disorders of gingiva and edentulous alveolar ridge ");
            ICDCodes.Add("L272", "Dermatitis due to ingested food ");
            ICDCodes.Add("V529", "Occupant of pick-up truck or van injured in collision with two- or three-wheeled motor vehicle: Unspecified occupant of pick-up truck or van injured in traffic accident ");
            ICDCodes.Add("I862", "Pelvic varices ");
            ICDCodes.Add("F48", "Other neurotic disorders ");
            ICDCodes.Add("J342", "Deviated nasal septum ");
            ICDCodes.Add("S142", "Injury of nerve root of cervical spine ");
            ICDCodes.Add("C723", "Malignant neoplasm: Optic nerve ");
            ICDCodes.Add("P111", "Other specified brain damage due to birth injury ");
            ICDCodes.Add("S055", "Penetrating wound of eyeball with foreign body ");
            ICDCodes.Add("K265", "Duodenal ulcer: Chronic or unspecified with perforation ");
            ICDCodes.Add("G560", "Carpal tunnel syndrome ");
            ICDCodes.Add("B418", "Other forms of paracoccidioidomycosis ");
            ICDCodes.Add("Z558", "Other problems related to education and literacy ");
            ICDCodes.Add("T266", "Corrosion of cornea and conjunctival sac ");
            ICDCodes.Add("T252", "Burn of second degree of ankle and foot ");
            ICDCodes.Add("B963", "Haemophilus influenzae [H. influenzae] as the cause of diseases classified to other chapters ");
            ICDCodes.Add("T143", "Dislocation sprain and strain of unspecified body region ");
            ICDCodes.Add("I849", "Unspecified haemorrhoids without complication ");
            ICDCodes.Add("G121", "Other inherited spinal muscular atrophy ");
            ICDCodes.Add("C151", "Malignant neoplasm: Thoracic part of oesophagus ");
            ICDCodes.Add("T051", "Traumatic amputation of one hand and other arm [any level except hand] ");
            ICDCodes.Add("J350", "Chronic tonsillitis ");
            ICDCodes.Add("K529", "Noninfective gastroenteritis and colitis unspecified ");
            ICDCodes.Add("M810", "Postmenopausal osteoporosis ");
            ICDCodes.Add("H187", "Other corneal deformities ");
            ICDCodes.Add("Z29", "Need for other prophylactic measures");
            ICDCodes.Add("L73", "Other follicular disorders");
            ICDCodes.Add("M032", "Other postinfectious arthropathies in diseases classified elsewhere");
        }


        public Dictionary<string,string> ICDCodes= new Dictionary<string, string>();


        public string GetRandomICDCode(Random random)
        {
            return ICDCodes.Keys.ToArray()[random.Next(ICDCodes.Count)];
        }
    }
}
