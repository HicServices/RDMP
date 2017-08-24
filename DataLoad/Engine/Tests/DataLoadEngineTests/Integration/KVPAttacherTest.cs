using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataHelper;
using DataLoadEngine.Job;
using LoadModules.Generic.Attachers;
using LoadModules.Generic.DataFlowOperations;
using LoadModules.Generic.DataFlowSources;
using NUnit.Framework;
using ReusableLibraryCode.Checks;
using Tests.Common;

namespace DataLoadEngineTests.Integration
{
    public class KVPAttacherTest:DatabaseTests
    {

        public enum KVPAttacherTestCase
        {
            OneFileWithPrimaryKey,
            OneFileWithoutPrimaryKey,
            TwoFilesWithPrimaryKey
        }

        [Test]
        [TestCase(KVPAttacherTestCase.OneFileWithPrimaryKey)]
        [TestCase(KVPAttacherTestCase.OneFileWithoutPrimaryKey)]
        [TestCase(KVPAttacherTestCase.TwoFilesWithPrimaryKey)]
        public void KVPAttacherTest_Attach(KVPAttacherTestCase testCase)
        {
            bool hasPk = testCase != KVPAttacherTestCase.OneFileWithoutPrimaryKey;

            var attacher = new KVPAttacher();
            var tbl = DiscoveredDatabaseICanCreateRandomTablesIn.ExpectTable("KVPTestTable");

            var workingDir = new DirectoryInfo(".");
            var parentDir = workingDir.CreateSubdirectory("KVPAttacherTestProjectDirectory");
            var projectDir = HICProjectDirectory.CreateDirectoryStructure(parentDir, "KVPAttacherTest", true);

            string filepk = "kvpTestFilePK.csv";
            string filepk2 = "kvpTestFilePK2.csv";
            string fileNoPk = "kvpTestFile_NoPK.csv";

            if (testCase == KVPAttacherTestCase.OneFileWithPrimaryKey || testCase == KVPAttacherTestCase.TwoFilesWithPrimaryKey)
                CopyToBin(projectDir, filepk);

            if (testCase == KVPAttacherTestCase.TwoFilesWithPrimaryKey)
                CopyToBin(projectDir, filepk2);

            if (testCase == KVPAttacherTestCase.OneFileWithoutPrimaryKey)
                CopyToBin(projectDir, fileNoPk);
            
            if (tbl.Exists())
                tbl.Drop();
            
            //Create destination data table on server (where the data will ultimately end up)
            using (var con = (SqlConnection) tbl.Database.Server.GetConnection())
            {
                con.Open();
                string sql = hasPk
                    ? "CREATE TABLE KVPTestTable (Person varchar(100), Test varchar(50), Result int)"
                    : "CREATE TABLE KVPTestTable (Test varchar(50), Result int)";

                new SqlCommand(sql, con).ExecuteNonQuery();
            }

            var remnantPipeline =
                CatalogueRepository.GetAllObjects<Pipeline>("WHERE Name='KVPAttacherTestPipeline'").SingleOrDefault();

            if(remnantPipeline != null)
                remnantPipeline.DeleteInDatabase();

            //Setup the Pipeline
            var p = new Pipeline(CatalogueRepository, "KVPAttacherTestPipeline");

            //With a CSV source
            var flatFileLoad = new PipelineComponent(CatalogueRepository, p, typeof (DelimitedFlatFileDataFlowSource), 0,"Data Flow Source");
            
            //followed by a Transpose that turns columns to rows (see how the test file grows right with new records instead of down, this is common in KVP input files but not always)
            var transpose = new PipelineComponent(CatalogueRepository, p, typeof (Transposer), 1, "Transposer");

            var saneHeaders = transpose.CreateArgumentsForClassIfNotExists(typeof (Transposer)).Single(a => a.Name.Equals("MakeHeaderNamesSane"));
            saneHeaders.SetValue(false);
            saneHeaders.SaveToDatabase();

            //set the source separator to comma
            flatFileLoad.CreateArgumentsForClassIfNotExists(typeof(DelimitedFlatFileDataFlowSource));
            var arg = flatFileLoad.PipelineComponentArguments.Single(a => a.Name.Equals("Separator"));
            arg.SetValue(",");
            arg.SaveToDatabase();

            arg = flatFileLoad.PipelineComponentArguments.Single(a => a.Name.Equals("MakeHeaderNamesSane"));
            arg.SetValue(false);
            arg.SaveToDatabase();

            p.SourcePipelineComponent_ID = flatFileLoad.ID;
            p.SaveToDatabase();
            
            try
            {
                attacher.PipelineForReadingFromFlatFile = p;
                attacher.TableName = "KVPTestTable";

                switch (testCase)
                {
                    case KVPAttacherTestCase.OneFileWithPrimaryKey:
                        attacher.FilePattern = filepk;
                        break;
                    case KVPAttacherTestCase.OneFileWithoutPrimaryKey:
                        attacher.FilePattern = fileNoPk;
                        break;
                    case KVPAttacherTestCase.TwoFilesWithPrimaryKey:
                        attacher.FilePattern = "kvpTestFilePK*.*";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("testCase");
                }
                

                if (hasPk)
                    attacher.PrimaryKeyColumns = "Person";

                attacher.TargetDataTableKeyColumnName = "Test";
                attacher.TargetDataTableValueColumnName = "Result";
                
                attacher.Initialize(projectDir,DiscoveredDatabaseICanCreateRandomTablesIn);

                attacher.Attach(new ThrowImmediatelyDataLoadJob());

                //test file contains 291 values belonging to 3 different people
                int expectedRows = 291;

                //if we loaded two files (or should have done) then add the number of values in that file (54)
                if (testCase == KVPAttacherTestCase.TwoFilesWithPrimaryKey)
                    expectedRows += 54;

                Assert.AreEqual(expectedRows, tbl.GetRowCount());

            }
            finally
            {
                p.DeleteInDatabase();
                tbl.Drop();
            }
        }


        [Test]
        public void KVPAttacherCheckTest_TableNameMissing()
        {
            var ex = Assert.Throws<Exception>(() => new KVPAttacher().Check(new ThrowImmediatelyCheckNotifier()));
            Assert.IsTrue(ex.Message.StartsWith("Argument TableName has not been set"));
        }

        [Test]
        public void KVPAttacherCheckTest_FilePathMissing()
        {
            var kvp = new KVPAttacher();
            kvp.TableName = "MyTable";

            var ex = Assert.Throws<Exception>(()=>kvp.Check(new ThrowImmediatelyCheckNotifier()));
            Assert.IsTrue(ex.Message.StartsWith("Argument FilePattern has not been set"));
        }



        [Test]
        [TestCase("PrimaryKeyColumns")]
        [TestCase("TargetDataTableKeyColumnName")]
        [TestCase("TargetDataTableValueColumnName")]
        public void KVPAttacherCheckTest_BasicArgumentMissing(string missingField)
        {
            var kvp = new KVPAttacher();
            kvp.TableName = "MyTable";
            kvp.FilePattern = "*.csv";

            if (missingField != "PrimaryKeyColumns")
                kvp.PrimaryKeyColumns = "dave,bob";

            if (missingField != "TargetDataTableKeyColumnName")
                kvp.TargetDataTableKeyColumnName = "frank";

            if (missingField != "TargetDataTableValueColumnName")
                kvp.TargetDataTableValueColumnName = "smith";
            
            var ex = Assert.Throws<Exception>(() => kvp.Check(new ThrowImmediatelyCheckNotifier()));
            Assert.IsTrue(ex.Message.StartsWith("Argument " + missingField + " has not been set"));
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void KVPAttacherCheckTest_Crossover(bool isKeyColumnDuplicate)
        {
            var kvp = new KVPAttacher();
            kvp.TableName = "MyTable";
            kvp.FilePattern = "*.csv";
            kvp.PrimaryKeyColumns = "dave,bob";
            kvp.TargetDataTableKeyColumnName =  isKeyColumnDuplicate ?"dave":"Fish";
            kvp.TargetDataTableValueColumnName = isKeyColumnDuplicate ? "tron" : "dave";

            var ex = Assert.Throws<Exception>(() => kvp.Check(new ThrowImmediatelyCheckNotifier()));
            Assert.AreEqual("Field 'dave' is both a PrimaryKeyColumn and a TargetDataTable column, this is not allowed.  Your fields Pk1,Pk2,Pketc,Key,Value must all be mutually exclusive", ex.Message);
        }

        [Test]
        public void KVPAttacherCheckTest_CrossoverKeyAndValue()
        {
            var kvp = new KVPAttacher();
            kvp.TableName = "MyTable";
            kvp.FilePattern = "*.csv";
            kvp.PrimaryKeyColumns = "dave";
            kvp.TargetDataTableKeyColumnName = "Key";
            kvp.TargetDataTableValueColumnName = "Key";

            var ex = Assert.Throws<Exception>(() => kvp.Check(new ThrowImmediatelyCheckNotifier()));
            Assert.AreEqual("TargetDataTableKeyColumnName cannot be the same as TargetDataTableValueColumnName", ex.Message);
        }

        private void CopyToBin(HICProjectDirectory projDir, string file)
        {
            
            string testFileLocation = ".\\Resources\\" + file;
            Assert.IsTrue(File.Exists(testFileLocation));

            File.Copy(testFileLocation, projDir.ForLoading.FullName + "\\" + file, true);
        }
    }
}
