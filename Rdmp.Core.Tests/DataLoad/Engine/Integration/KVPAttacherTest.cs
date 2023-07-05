// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Modules.Attachers;
using Rdmp.Core.DataLoad.Modules.DataFlowOperations;
using Rdmp.Core.DataLoad.Modules.DataFlowSources;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Tests.Common;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration;

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
        var hasPk = testCase != KVPAttacherTestCase.OneFileWithoutPrimaryKey;
        var db = GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer);
        var attacher = new KVPAttacher();
        var tbl = db.ExpectTable("KVPTestTable");

        var workingDir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        var parentDir = workingDir.CreateSubdirectory("KVPAttacherTestProjectDirectory");
        var projectDir = LoadDirectory.CreateDirectoryStructure(parentDir, "KVPAttacherTest", true);

        var filepk = "kvpTestFilePK.csv";
        var filepk2 = "kvpTestFilePK2.csv";
        var fileNoPk = "kvpTestFile_NoPK.csv";

        if (testCase == KVPAttacherTestCase.OneFileWithPrimaryKey || testCase == KVPAttacherTestCase.TwoFilesWithPrimaryKey)
            CopyToBin(projectDir, filepk);

        if (testCase == KVPAttacherTestCase.TwoFilesWithPrimaryKey)
            CopyToBin(projectDir, filepk2);

        if (testCase == KVPAttacherTestCase.OneFileWithoutPrimaryKey)
            CopyToBin(projectDir, fileNoPk);
            
        if (tbl.Exists())
            tbl.Drop();
            
        //Create destination data table on server (where the data will ultimately end SetUp)
        using (var con = (SqlConnection) tbl.Database.Server.GetConnection())
        {
            con.Open();
            var sql = hasPk
                ? "CREATE TABLE KVPTestTable (Person varchar(100), Test varchar(50), Result int)"
                : "CREATE TABLE KVPTestTable (Test varchar(50), Result int)";

            new SqlCommand(sql, con).ExecuteNonQuery();
        }

        var remnantPipeline =
            CatalogueRepository.GetAllObjects<Pipeline>().SingleOrDefault(p=>p.Name.Equals("KVPAttacherTestPipeline"));

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
                    throw new ArgumentOutOfRangeException(nameof(testCase));
            }
                

            if (hasPk)
                attacher.PrimaryKeyColumns = "Person";

            attacher.TargetDataTableKeyColumnName = "Test";
            attacher.TargetDataTableValueColumnName = "Result";
                
            attacher.Initialize(projectDir,db);

            attacher.Attach(new ThrowImmediatelyDataLoadJob(), new GracefulCancellationToken());

            //test file contains 291 values belonging to 3 different people
            var expectedRows = 291;

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
        Assert.AreEqual("Either argument TableName or TableToLoad must be set Rdmp.Core.DataLoad.Modules.Attachers.KVPAttacher, you should specify this value.",ex.Message);
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
        Assert.IsTrue(ex.Message.StartsWith($"Argument {missingField} has not been set"));
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

    private void CopyToBin(LoadDirectory projDir, string file)
    {
            
        var testFileLocation = Path.Combine(TestContext.CurrentContext.TestDirectory,"DataLoad","Engine","Resources" , file);
        Assert.IsTrue(File.Exists(testFileLocation));

        File.Copy(testFileLocation, projDir.ForLoading.FullName + Path.DirectorySeparatorChar + file, true);
    }
}