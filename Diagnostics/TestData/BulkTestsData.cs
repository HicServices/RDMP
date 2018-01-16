using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataHelper;
using CatalogueLibrary.Repositories;
using CatalogueLibrary.Triggers;
using DataLoadEngine.Migration;
using HIC.Common.Validation;
using HIC.Common.Validation.Constraints;
using HIC.Common.Validation.Constraints.Primary;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace Diagnostics.TestData
{
    public class BulkTestsData
    {
        private readonly CatalogueRepository _repository;
        public readonly DiscoveredDatabase BulkDataDatabase;
        

        public const string BulkDataTable = "BulkData";
        public const string SlowView = "vSlowView";

        public readonly int ExpectedNumberOfRowsInTestData = 100000;

        public BulkTestsData(CatalogueRepository repository, DiscoveredDatabase targetDatabase, int numberOfRows = 100000)
        {
            _repository = repository;
            BulkDataDatabase = targetDatabase;
            ExpectedNumberOfRowsInTestData = numberOfRows;
        }

        public void SetupTestData()
        {
            //make sure database exists
            if (!BulkDataDatabase.Exists())
                BulkDataDatabase.Create();

            var server = BulkDataDatabase.Server;
            using (var con = server.GetConnection())
            {
                con.Open();
                
                var createTable = server.GetCommand(
                    @"
if exists (select * from sysobjects where name='" + BulkDataTable + @"' and xtype='U')
begin
drop table " +BulkDataTable+@";
end

CREATE TABLE " + BulkDataTable+ @"(
       [chi] [varchar](10) NOT NULL,
       [dtCreated] [date] NULL,
       [current_record] [bit] NULL,
       [notes] [varchar](max) NULL,
       [chi_num_of_curr_record] [varchar](10) NULL,
       [chi_status] [varchar](2) NULL,
       [century] [varchar](2) NULL,
       [surname] [varchar](20) NULL,
       [forename] [varchar](50) NULL,
       [sex] [varchar](1) NULL,
       [current_address_L1] [varchar](255) NULL,
       [current_address_L2] [varchar](255) NULL,
       [current_address_L3] [varchar](255) NULL,
       [current_address_L4] [varchar](255) NULL,
       [current_postcode] [varchar](10) NULL,
       [date_of_death] date NULL,
       [source_death] [varchar](2) NULL,
       [area_residence] [varchar](1) NULL,
       [hb_extract] [varchar](1) NULL,
       [current_gp] [varchar](5) NULL,
       [birth_surname] [varchar](20) NULL,
       [previous_surname] [varchar](20) NULL,
       [midname] [varchar](50) NULL,
       [alt_forename] [varchar](50) NULL,
       [other_initials] [varchar](5) NULL,
       [previous_address_L1] [varchar](255) NULL,
       [previous_address_L2] [varchar](255) NULL,
       [previous_address_L3] [varchar](255) NULL,
       [previous_address_L4] [varchar](255) NULL,
       [previous_postcode] [varchar](10) NULL,
       [date_address_changed] date NULL,
       [adr] [varchar](2) NULL,
       [current_gp_accept_date] date NULL,
       [previous_gp] [varchar](5) NULL,
       [previous_gp_accept_date] date NULL,
       [date_into_practice] date NULL,
       [date_of_birth] date NULL,
       [patient_triage_score] float,
       [" + SpecialFieldNames.DataLoadRunID+@"] int
 CONSTRAINT [PK_BulkData] PRIMARY KEY CLUSTERED 
(
	[chi] ASC
)
)", con);
                createTable.ExecuteNonQuery();

                //a view that joins to itself on a non indexed column
                UsefulStuff.ExecuteBatchNonQuery(@"
IF EXISTS(select * FROM sys.views where name = 'vSlowView')
begin
drop view vSlowView;
end
GO

CREATE VIEW vSlowView As Select * from " + BulkDataTable + " boss where date_of_death is null and 1= (select count(*) from "+BulkDataTable+" inception where inception.chi = boss.chi)"
                                         , con);

                Stopwatch sw = new Stopwatch();

                //100 batches
                for (int batch = 0; batch < 10; batch++)
                {
                    sw.Start();
                    DataTable dt = new DataTable();
                    GenerateColumns(dt);

                    //each 100th of the expected size
                    for (int i = 0; i < ExpectedNumberOfRowsInTestData/10; i++)
                        dt.Rows.Add(GenerateTestDataRow());


                    SqlBulkCopy bulkcopy = new SqlBulkCopy((SqlConnection) con);
                    bulkcopy.BulkCopyTimeout = 50000;
                    bulkcopy.DestinationTableName = BulkDataTable;

                    UsefulStuff.BulkInsertWithBetterErrorMessages(bulkcopy, dt, server);
                    sw.Stop();
                    Console.WriteLine("Submitting Batch:" + batch + " ("+sw.Elapsed+")");
                    sw.Reset();
                }
                
                con.Close();
            }
            
        }

        public void Destroy()
        {
            BulkDataDatabase.ForceDrop();
        }

        private void GenerateColumns(DataTable dt)
        {
dt.Columns.Add("chi");
dt.Columns.Add("dtCreated");
dt.Columns.Add("current_record");
dt.Columns.Add("notes");
dt.Columns.Add("chi_num_of_curr_record");
dt.Columns.Add("chi_status");
dt.Columns.Add("century");
dt.Columns.Add("surname");
dt.Columns.Add("forename");
dt.Columns.Add("sex");
dt.Columns.Add("current_address_L1");
dt.Columns.Add("current_address_L2");
dt.Columns.Add("current_address_L3");
dt.Columns.Add("current_address_L4");
dt.Columns.Add("current_postcode");
dt.Columns.Add("date_of_death");
dt.Columns.Add("source_death");
dt.Columns.Add("area_residence");
dt.Columns.Add("hb_extract");
dt.Columns.Add("current_gp");
dt.Columns.Add("birth_surname");
dt.Columns.Add("previous_surname");
dt.Columns.Add("midname");
dt.Columns.Add("alt_forename");
dt.Columns.Add("other_initials");
dt.Columns.Add("previous_address_L1");
dt.Columns.Add("previous_address_L2");
dt.Columns.Add("previous_address_L3");
dt.Columns.Add("previous_address_L4");
dt.Columns.Add("previous_postcode");
dt.Columns.Add("date_address_changed");
dt.Columns.Add("adr");
dt.Columns.Add("current_gp_accept_date");
dt.Columns.Add("previous_gp");
dt.Columns.Add("previous_gp_accept_date");
dt.Columns.Add("date_into_practice");
dt.Columns.Add("date_of_birth");
dt.Columns.Add("patient_triage_score",typeof(object));
dt.Columns.Add(SpecialFieldNames.DataLoadRunID);
            
        }

        static Random r = new Random();

        public static object[] GenerateTestDataRow(TestPerson usePerson=null)
        {
            var values = new object[39];

            //use existing person or a new random person
            var person = usePerson??new TestPerson(r);

            values[0] = person.CHI;
            values[1] = TestPerson.GetRandomDateAfter(person.DateOfBirth,r);//all records must have been created after the person was born
            
            if(r.Next(0, 2) == 0)
                values[2] = true;
            else
                values[2] = false;

            values[3] = "Random record";
            
            if(r.Next(0,10 )== 0)//one in 10 records has one of these (an ALIAS chi)
                values[4] = TestPerson.GetRandomCHI(person.DateOfBirth,person.Gender,r);

            values[5] = GetRandomCHIStatus();
            values[6] = person.DateOfBirth.Year.ToString().Substring(0,2);
            values[7] = person.Surname;
            values[8] = person.Forename;
            values[9] = person.Gender;


            var randomAddress = new TestAddress(r);
            
            //if person is dead and dtCreated is after they died use the same address otehrwise use a random one (all records after a person dies have same address)
            values[10] = person.DateOfDeath != null && (DateTime)values[1]>person.DateOfDeath ? person.Address.Line1: randomAddress.Line1;
            values[11] = person.DateOfDeath != null && (DateTime)values[1]>person.DateOfDeath ? person.Address.Line2: randomAddress.Line2;
            values[12] = person.DateOfDeath != null && (DateTime)values[1]>person.DateOfDeath ? person.Address.Line3: randomAddress.Line3;
            values[13] = person.DateOfDeath != null && (DateTime)values[1]>person.DateOfDeath ? person.Address.Line4: randomAddress.Line4;
            values[14] = person.DateOfDeath != null && (DateTime)values[1]>person.DateOfDeath ? person.Address.Postcode.Value: randomAddress.Postcode.Value;

            //if the person is dead and the dtCreated of the record is greater than the date of death populate it
            values[15] = person.GetDateOfDeathOrNullOn((DateTime)values[1]); //pass record creation date and get isdead date back
                
            //if we got a date put the source in as R
            if(values[15] != null)
                values[16] = 'R';
            

            if(!string.IsNullOrWhiteSpace(person.Address.Postcode.District))
                values[17] = person.Address.Postcode.District.Substring(0, 1);

            values[18] = GetRandomLetter(true,r);

            //healthboard 'A' use padding on the name field (to a length of 10!)
            if((char)values[18] == 'A')
                if (values[8] != null)
                    while (values[8].ToString().Length < 10)
                        values[8] = values[8] + " ";

            //in healthboard 'B' they give us both forename and suranme in the same field! - and surname is always blank
            if ((char)values[18] == 'B')
            {
                values[8] = values[8] + " " +values[7];
                values[7] = null;
            }

            values[19] = GetRandomGPCode();

            //birth surname and previous surname fields, sparsely populated
            if (r.Next(0, 10) == 0)
                values[20] = TestPerson.GetRandomSurname(r);
            if (r.Next(0, 10) == 0)
                values[21] = TestPerson.GetRandomSurname(r);
            
            if (r.Next(0, 3) == 0)
                values[22] = person.GetRandomForename(r); //random gender appropriate middle name for 1 person in 3
            
            if (r.Next(0, 5) == 0)
                values[23] = person.GetRandomForename(r); //alternate forename

            if(r.Next(0,3)==0)
                values[24] = GetRandomLetter(true, r);  //one in 3 has an initial

            //people only have previous addresses if they are alive
            if(r.Next(0, 2) == 0 && person.DateOfDeath != null)
            {
                var randomAddress2 = new TestAddress(r);

                values[25] = randomAddress2.Line1;
                values[26] = randomAddress2.Line2;
                values[27] = randomAddress2.Line3;
                values[28] = randomAddress2.Line4;
                values[29] = randomAddress2.Postcode.Value;

                //date of address change is unknown for 50% of records
                if (r.Next(0, 2) == 0)
                {
                    //get after birth but before dtCreated/date of death
                    values[30] = TestPerson.GetRandomDateBetween(person.DateOfBirth, GetMinimum(person.DateOfDeath,(DateTime)values[1]),r);
                }
            }

            //an always null field, why not?!
            values[31] = null;

            DateTime gp_accept_date = TestPerson.GetRandomDateAfter(person.DateOfBirth, r);
            
            //current_gp_accept_date
            values[32] = gp_accept_date;


            //before 1980 some records will be missing forename (deliberate errors!)
            if (gp_accept_date.Year < 1980)
                if (r.Next(gp_accept_date.Year - 1970) == 0)//the farther back you go the more likely they are to be missing a forename
                        values[8] = null;//some people are randomly missing a forename
            
            if(r.Next(0,3)==0)
            {
                values[33] = GetRandomGPCode();
                values[34] = TestPerson.GetRandomDateAfter((DateTime) values[32], r);
            }

            values[35] = TestPerson.GetRandomDateBetween(person.DateOfBirth, GetMinimum(person.DateOfDeath, (DateTime)values[1]), r);
            values[36] = person.DateOfBirth;
            values[37] = GetRandomDouble(r);

            //data load run id will be batches 1 (1900 is first year of possible dtCreated) to 12 (2015 - 1890 / 10 = 12)
            values[38] = (((DateTime) values[1]).Year - 1890)/10;

            return values;
        }

        private static DateTime GetMinimum(DateTime? date1, DateTime date2)
        {
            if (date1 == null)
                return date2;

            if (date2 > date1)
                return (DateTime)date1;

            return date2;
        }

        public static object GetRandomDouble(Random r)
        {
            switch (r.Next(0, 3))
            {
                case 0:
                    return r.Next(100);
                case 1:
                    return Math.Round(r.NextDouble(),2);
                case 2:
                    return r.Next(10) + "." + r.Next(10);
                default:
                    throw new NotImplementedException();
            }
        }

        public static string GetRandomGPCode()
        {
            return GetRandomLetter(true,r).ToString() + r.Next(0, 999);
        }

        public static char GetRandomLetter(bool upperCase,Random r)
        {
            if(upperCase)
                return (char) ('A' + r.Next(0, 26));

            return (char)('a' + r.Next(0, 26));

        }

        private static object GetRandomCHIStatus()
        {
            switch (r.Next(0, 5))
            {
                case 0:return 'C';
                case 1: return 'H';
                case 2:return null;
                case 3: return 'L';
                case 4: return 'R';
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public DataTable GetDataTable(int numberOfRows)
        {
            DataTable dt = new DataTable();
            var server = BulkDataDatabase.Server;
            using (var con = server.GetConnection())
            {
                con.Open();

                var da = server.GetDataAdapter("Select * from " + BulkDataTable,con);
                da.Fill(0, numberOfRows, dt);
            }

            return dt;
        }

        public TableInfo tableInfo;
        public ColumnInfo[] columnInfos;
        public Catalogue catalogue;
        public CatalogueItem[] catalogueItems;
        public ExtractionInformation[] extractionInformations;
        

        public Catalogue ImportAsCatalogue()
        {
            TableInfoImporter f = new TableInfoImporter(_repository, BulkDataDatabase.ExpectTable(BulkDataTable));
            f.DoImport(out tableInfo,out columnInfos);

            ForwardEngineerCatalogue forwardEngineer = new ForwardEngineerCatalogue(tableInfo,columnInfos,true);
            forwardEngineer.ExecuteForwardEngineering(out catalogue,out catalogueItems, out extractionInformations);

            var chi = extractionInformations.Single(e => e.GetRuntimeName().Equals("chi"));
            chi.IsExtractionIdentifier = true;
            chi.SaveToDatabase();

            return catalogue;
        }

        public void DeleteCatalogue()
        {
            var creds = tableInfo.GetCredentialsIfExists(DataAccessContext.InternalDataProcessing);
            tableInfo.DeleteInDatabase();
            if(creds != null)
                try
                {
                    creds.DeleteInDatabase();
                }
                catch (CredentialsInUseException e)
                {
                    Console.WriteLine("Ignored Potential Exception:" + e);
                }

            catalogue.DeleteInDatabase();
        }

        public void SetupValidationOnCatalogue()
        {
            Validator v = new Validator();
            var iv = new ItemValidator("chi");
            iv.PrimaryConstraint = new Chi();
            iv.PrimaryConstraint.Consequence = Consequence.Wrong;

            v.AddItemValidator(iv, "chi", typeof(string));
            catalogue.ValidatorXML = v.SaveToXml();

            catalogue.TimeCoverage_ExtractionInformation_ID =
                catalogue.GetAllExtractionInformation(ExtractionCategory.Any)
                    .Single(e => e.GetRuntimeName().Equals("dtCreated")).ID;

            catalogue.SaveToDatabase();
        }
    }
}
