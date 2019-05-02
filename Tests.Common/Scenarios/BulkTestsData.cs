// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using Diagnostics;
using Diagnostics.TestData;
using Diagnostics.TestData.Exercises;
using FAnsi.Discovery;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataLoad.Triggers;
using Rdmp.Core.Repositories;
using Rdmp.Core.Validation;
using Rdmp.Core.Validation.Constraints;
using Rdmp.Core.Validation.Constraints.Primary;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;

namespace Tests.Common.Scenarios
{
    public class BulkTestsData
    {
        private readonly ICatalogueRepository _repository;
        public readonly DiscoveredDatabase BulkDataDatabase;
        
        public const string BulkDataTable = "BulkData";
        public const string SlowView = "vSlowView";

        public readonly int ExpectedNumberOfRowsInTestData = 100000;

        public TableInfo tableInfo;
        public ColumnInfo[] columnInfos;
        public Catalogue catalogue;
        public CatalogueItem[] catalogueItems;
        public ExtractionInformation[] extractionInformations;
        private DemographyExerciseTestData _dataGenerator;

        private Random r = new Random();

        public BulkTestsData(ICatalogueRepository repository, DiscoveredDatabase targetDatabase, int numberOfRows = 100000)
        {
            _repository = repository;
            BulkDataDatabase = targetDatabase;
            ExpectedNumberOfRowsInTestData = numberOfRows;

            _dataGenerator = new DemographyExerciseTestData();

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
                        dt.Rows.Add(_dataGenerator.GenerateTestDataRow(new TestPerson(r)));
                    
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
            BulkDataDatabase.Drop();
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
            var creds = (DataAccessCredentials)tableInfo.GetCredentialsIfExists(DataAccessContext.InternalDataProcessing);
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

        public ColumnInfo GetColumnInfo(string colName)
        {
            return columnInfos.Single(c => c.GetRuntimeName().Equals(colName));
        }
    }
}
