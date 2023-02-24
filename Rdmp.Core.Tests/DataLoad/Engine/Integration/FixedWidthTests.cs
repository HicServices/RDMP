// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Rdmp.Core.Curation;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Modules.Attachers;
using Rdmp.Core.DataLoad.Modules.Exceptions;
using Tests.Common;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration;

public class FixedWidthTests :DatabaseTests
{
    private FixedWidthFormatFile CreateFormatFile()
    {
        var fileInfo = new FileInfo(Path.Combine(TestContext.CurrentContext.TestDirectory,@"FixedWidthFormat.csv"));

        File.WriteAllText(fileInfo.FullName, LoadDirectory.ExampleFixedWidthFormatFileContents);
            
        Assert.IsTrue(fileInfo.Exists);

        return new FixedWidthFormatFile(fileInfo);
    }
        
    [Test]
    public void TestLoadingFormat()
    {
        var formatFile = CreateFormatFile();

        Assert.AreEqual(8,formatFile.FormatColumns.Length);

        Assert.AreEqual("gmc",formatFile.FormatColumns[0].Field);
        Assert.AreEqual(1, formatFile.FormatColumns[0].From);
        Assert.AreEqual(7, formatFile.FormatColumns[0].To);
        Assert.AreEqual(1+ formatFile.FormatColumns[0].To - formatFile.FormatColumns[0].From, formatFile.FormatColumns[0].Size);
        Assert.AreEqual(7, formatFile.FormatColumns[0].Size);

        Assert.AreEqual("gp_code", formatFile.FormatColumns[1].Field);
        Assert.AreEqual(8, formatFile.FormatColumns[1].From);
        Assert.AreEqual(12, formatFile.FormatColumns[1].To);
        Assert.AreEqual(1 + formatFile.FormatColumns[1].To - formatFile.FormatColumns[1].From, formatFile.FormatColumns[1].Size);
        Assert.AreEqual(5, formatFile.FormatColumns[1].Size);

        Assert.AreEqual("surname", formatFile.FormatColumns[2].Field);
        Assert.AreEqual(13, formatFile.FormatColumns[2].From);
        Assert.AreEqual(32, formatFile.FormatColumns[2].To);
        Assert.AreEqual(1 + formatFile.FormatColumns[2].To - formatFile.FormatColumns[2].From, formatFile.FormatColumns[2].Size);
        Assert.AreEqual(20, formatFile.FormatColumns[2].Size);

        Assert.AreEqual("forename", formatFile.FormatColumns[3].Field);
        Assert.AreEqual(33, formatFile.FormatColumns[3].From);
        Assert.AreEqual(52, formatFile.FormatColumns[3].To);
        Assert.AreEqual(1 + formatFile.FormatColumns[3].To - formatFile.FormatColumns[3].From, formatFile.FormatColumns[3].Size);
        Assert.AreEqual(20, formatFile.FormatColumns[3].Size);

        Assert.AreEqual("initials", formatFile.FormatColumns[4].Field);
        Assert.AreEqual(53, formatFile.FormatColumns[4].From);
        Assert.AreEqual(55, formatFile.FormatColumns[4].To);
        Assert.AreEqual(1 + formatFile.FormatColumns[4].To - formatFile.FormatColumns[4].From, formatFile.FormatColumns[4].Size);
        Assert.AreEqual(3, formatFile.FormatColumns[4].Size);

        Assert.AreEqual("practice_code", formatFile.FormatColumns[5].Field);
        Assert.AreEqual(56, formatFile.FormatColumns[5].From);
        Assert.AreEqual(60, formatFile.FormatColumns[5].To);
        Assert.AreEqual(1 + formatFile.FormatColumns[5].To - formatFile.FormatColumns[5].From, formatFile.FormatColumns[5].Size);
        Assert.AreEqual(5, formatFile.FormatColumns[5].Size);
            
        Assert.AreEqual("date_into_practice", formatFile.FormatColumns[6].Field);
        Assert.AreEqual(61, formatFile.FormatColumns[6].From);
        Assert.AreEqual(68, formatFile.FormatColumns[6].To);
        Assert.AreEqual(1 + formatFile.FormatColumns[6].To - formatFile.FormatColumns[6].From, formatFile.FormatColumns[6].Size);
        Assert.AreEqual(8, formatFile.FormatColumns[6].Size);
        Assert.AreEqual("yyyyMMdd", formatFile.FormatColumns[6].DateFormat);

        Assert.AreEqual("date_out_of_practice", formatFile.FormatColumns[7].Field);
        Assert.AreEqual(69, formatFile.FormatColumns[7].From);
        Assert.AreEqual(76, formatFile.FormatColumns[7].To);
        Assert.AreEqual(1 + formatFile.FormatColumns[7].To - formatFile.FormatColumns[7].From, formatFile.FormatColumns[7].Size);
        Assert.AreEqual(8, formatFile.FormatColumns[7].Size);
        Assert.AreEqual("yyyyMMdd", formatFile.FormatColumns[7].DateFormat);
    }

    [Test]
    public void TestLoadingFormatThenFile()
    {
        var formatFile = CreateFormatFile();

        var tempFileToCreate = Path.Combine(TestContext.CurrentContext.TestDirectory,"unitTestFixedWidthFile.txt");

        var streamWriter = File.CreateText(tempFileToCreate);
        try
        {
            streamWriter.WriteLine("002644099999Akerman             Frank               FM 380512004040120090501");
            streamWriter.WriteLine("002705600000SHAW                LENA                LC 852251978100119941031");
            streamWriter.Flush();
            streamWriter.Close();
                
            var dataTable = formatFile.GetDataTableFromFlatFile(new FileInfo(tempFileToCreate));
            Assert.AreEqual(dataTable.Rows.Count,2);
            Assert.AreEqual("0026440", dataTable.Rows[0]["gmc"]);
            Assert.AreEqual("99999", dataTable.Rows[0]["gp_code"]);
            Assert.AreEqual("Akerman", dataTable.Rows[0]["surname"]);
            Assert.AreEqual("Frank", dataTable.Rows[0]["forename"]);
            Assert.AreEqual("FM", dataTable.Rows[0]["initials"]);
            Assert.AreEqual("38051", dataTable.Rows[0]["practice_code"]);
            Assert.AreEqual(new DateTime(2004, 4, 1), dataTable.Rows[0]["date_into_practice"]);
            Assert.AreEqual(new DateTime(2009,5,1), dataTable.Rows[0]["date_out_of_practice"]);
                

        }
        finally 
        {
            File.Delete(tempFileToCreate);
        }
    }

    public enum FixedWidthTestCase
    {
        CompatibleHeaders,
        MisnamedHeaders,
        InsufficientLengthOfCharactersInFileToLoad
    }

    [Test]
    [TestCase(FixedWidthTestCase.CompatibleHeaders)]
    [TestCase(FixedWidthTestCase.MisnamedHeaders)]
    [TestCase(FixedWidthTestCase.InsufficientLengthOfCharactersInFileToLoad)]
    public void TestHeaderMatching(FixedWidthTestCase testCase)
    {
        //Create the format file
        var flatFileColumn = "myNumber";

        if (testCase == FixedWidthTestCase.MisnamedHeaders)
            flatFileColumn = "chickenDippers";

        var formatFile = new FileInfo(Path.Combine(TestContext.CurrentContext.TestDirectory,"Format.csv"));

        File.WriteAllText(formatFile.FullName, $@"From,To,Field,Size,DateFormat
1,5,{flatFileColumn},5");


        //Create the working directory that will be processed
        var workingDir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        var parentDir = workingDir.CreateSubdirectory("FixedWidthTests");

        var toCleanup = parentDir.GetDirectories().SingleOrDefault(d => d.Name.Equals("TestHeaderMatching"));
        if (toCleanup != null)
            toCleanup.Delete(true);

        var loadDirectory = LoadDirectory.CreateDirectoryStructure(parentDir, "TestHeaderMatching");


        //create the file we will be trying to load
        if(testCase == FixedWidthTestCase.InsufficientLengthOfCharactersInFileToLoad)
            File.WriteAllText(Path.Combine(loadDirectory.ForLoading.FullName, "file.txt"), @"12345
12");
        else
            File.WriteAllText(Path.Combine(loadDirectory.ForLoading.FullName , "file.txt"),@"12345
67890");
        var db = GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer);

        var attacher = new FixedWidthAttacher();
        attacher.Initialize(loadDirectory, db);
        attacher.PathToFormatFile = formatFile;
        attacher.TableName = "TestHeaderMatching_Compatible";
        attacher.FilePattern = "*.txt";

        using (var con = db.Server.GetConnection())
        {
            con.Open();
            db.Server.GetCommand("CREATE TABLE TestHeaderMatching_Compatible( myNumber int)", con).ExecuteNonQuery();
        }

        var table = db.ExpectTable("TestHeaderMatching_Compatible");
            
        Assert.IsTrue(table.Exists());
        Assert.AreEqual(0, table.GetRowCount());
        try
        {
            Regex errorRegex;
            Exception ex;

            switch (testCase)
            {
                //Success Case
                case FixedWidthTestCase.CompatibleHeaders:
                    attacher.Attach(new ThrowImmediatelyDataLoadJob(), new GracefulCancellationToken());
                    Assert.AreEqual(2, table.GetRowCount());
                    return;//Return


                //Error cases, set the expected error result
                case FixedWidthTestCase.MisnamedHeaders:
                    errorRegex = new Regex(
                        @"Format file \(.*Format.csv\) indicated there would be a header called 'chickenDippers' but the column did not appear in the RAW database table \(Columns in RAW were myNumber\)");
                    ex = Assert.Throws<Exception>(() => attacher.Attach(new ThrowImmediatelyDataLoadJob(), new GracefulCancellationToken()));
                    break;
                case FixedWidthTestCase.InsufficientLengthOfCharactersInFileToLoad:
                    errorRegex = new Regex(
                        @"Error on line 2 of file file.txt, the format file \(.*Format.csv\) specified that a column myNumber would be found between character positions 1 and 5 but the current line is only 2 characters long");
                    ex = Assert.Throws<FlatFileLoadException>(() => attacher.Attach(new ThrowImmediatelyDataLoadJob(), new GracefulCancellationToken()));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(testCase));
            }
              
            //Assert the expected error result is the real one
            Assert.IsTrue(errorRegex.IsMatch(ex.Message));
                

        }
        finally
        {
            table.Drop();
        }
    }
}