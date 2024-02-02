// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
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

public class FixedWidthTests : DatabaseTests
{
    private static FixedWidthFormatFile CreateFormatFile()
    {
        var fileInfo = new FileInfo(Path.Combine(TestContext.CurrentContext.TestDirectory, @"FixedWidthFormat.csv"));

        File.WriteAllText(fileInfo.FullName, LoadDirectory.ExampleFixedWidthFormatFileContents);

        Assert.That(fileInfo.Exists);

        return new FixedWidthFormatFile(fileInfo);
    }

    [Test]
    public void TestLoadingFormat()
    {
        var formatFile = CreateFormatFile();

        Assert.That(formatFile.FormatColumns, Has.Length.EqualTo(8));

        Assert.Multiple(() =>
        {
            Assert.That(formatFile.FormatColumns[0].Field, Is.EqualTo("gmc"));
            Assert.That(formatFile.FormatColumns[0].From, Is.EqualTo(1));
            Assert.That(formatFile.FormatColumns[0].To, Is.EqualTo(7));
            Assert.That(formatFile.FormatColumns[0].Size, Is.EqualTo(1 + formatFile.FormatColumns[0].To - formatFile.FormatColumns[0].From));
        });
        Assert.Multiple(() =>
        {
            Assert.That(formatFile.FormatColumns[0].Size, Is.EqualTo(7));

            Assert.That(formatFile.FormatColumns[1].Field, Is.EqualTo("gp_code"));
            Assert.That(formatFile.FormatColumns[1].From, Is.EqualTo(8));
            Assert.That(formatFile.FormatColumns[1].To, Is.EqualTo(12));
            Assert.That(formatFile.FormatColumns[1].Size, Is.EqualTo(1 + formatFile.FormatColumns[1].To - formatFile.FormatColumns[1].From));
        });
        Assert.Multiple(() =>
        {
            Assert.That(formatFile.FormatColumns[1].Size, Is.EqualTo(5));

            Assert.That(formatFile.FormatColumns[2].Field, Is.EqualTo("surname"));
            Assert.That(formatFile.FormatColumns[2].From, Is.EqualTo(13));
            Assert.That(formatFile.FormatColumns[2].To, Is.EqualTo(32));
            Assert.That(formatFile.FormatColumns[2].Size, Is.EqualTo(1 + formatFile.FormatColumns[2].To - formatFile.FormatColumns[2].From));
        });
        Assert.Multiple(() =>
        {
            Assert.That(formatFile.FormatColumns[2].Size, Is.EqualTo(20));

            Assert.That(formatFile.FormatColumns[3].Field, Is.EqualTo("forename"));
            Assert.That(formatFile.FormatColumns[3].From, Is.EqualTo(33));
            Assert.That(formatFile.FormatColumns[3].To, Is.EqualTo(52));
            Assert.That(formatFile.FormatColumns[3].Size, Is.EqualTo(1 + formatFile.FormatColumns[3].To - formatFile.FormatColumns[3].From));
        });
        Assert.Multiple(() =>
        {
            Assert.That(formatFile.FormatColumns[3].Size, Is.EqualTo(20));

            Assert.That(formatFile.FormatColumns[4].Field, Is.EqualTo("initials"));
            Assert.That(formatFile.FormatColumns[4].From, Is.EqualTo(53));
            Assert.That(formatFile.FormatColumns[4].To, Is.EqualTo(55));
            Assert.That(formatFile.FormatColumns[4].Size, Is.EqualTo(1 + formatFile.FormatColumns[4].To - formatFile.FormatColumns[4].From));
        });
        Assert.Multiple(() =>
        {
            Assert.That(formatFile.FormatColumns[4].Size, Is.EqualTo(3));

            Assert.That(formatFile.FormatColumns[5].Field, Is.EqualTo("practice_code"));
            Assert.That(formatFile.FormatColumns[5].From, Is.EqualTo(56));
            Assert.That(formatFile.FormatColumns[5].To, Is.EqualTo(60));
            Assert.That(formatFile.FormatColumns[5].Size, Is.EqualTo(1 + formatFile.FormatColumns[5].To - formatFile.FormatColumns[5].From));
        });
        Assert.Multiple(() =>
        {
            Assert.That(formatFile.FormatColumns[5].Size, Is.EqualTo(5));

            Assert.That(formatFile.FormatColumns[6].Field, Is.EqualTo("date_into_practice"));
            Assert.That(formatFile.FormatColumns[6].From, Is.EqualTo(61));
            Assert.That(formatFile.FormatColumns[6].To, Is.EqualTo(68));
            Assert.That(formatFile.FormatColumns[6].Size, Is.EqualTo(1 + formatFile.FormatColumns[6].To - formatFile.FormatColumns[6].From));
        });
        Assert.Multiple(() =>
        {
            Assert.That(formatFile.FormatColumns[6].Size, Is.EqualTo(8));
            Assert.That(formatFile.FormatColumns[6].DateFormat, Is.EqualTo("yyyyMMdd"));

            Assert.That(formatFile.FormatColumns[7].Field, Is.EqualTo("date_out_of_practice"));
            Assert.That(formatFile.FormatColumns[7].From, Is.EqualTo(69));
            Assert.That(formatFile.FormatColumns[7].To, Is.EqualTo(76));
            Assert.That(formatFile.FormatColumns[7].Size, Is.EqualTo(1 + formatFile.FormatColumns[7].To - formatFile.FormatColumns[7].From));
        });
        Assert.Multiple(() =>
        {
            Assert.That(formatFile.FormatColumns[7].Size, Is.EqualTo(8));
            Assert.That(formatFile.FormatColumns[7].DateFormat, Is.EqualTo("yyyyMMdd"));
        });
    }

    [Test]
    public void TestLoadingFormatThenFile()
    {
        var formatFile = CreateFormatFile();

        var tempFileToCreate = Path.Combine(TestContext.CurrentContext.TestDirectory, "unitTestFixedWidthFile.txt");

        var streamWriter = File.CreateText(tempFileToCreate);
        try
        {
            streamWriter.WriteLine("002644099999Akerman             Frank               FM 380512004040120090501");
            streamWriter.WriteLine("002705600000SHAW                LENA                LC 852251978100119941031");
            streamWriter.Flush();
            streamWriter.Close();

            var dataTable = formatFile.GetDataTableFromFlatFile(new FileInfo(tempFileToCreate));
            Assert.That(dataTable.Rows, Has.Count.EqualTo(2));
            Assert.Multiple(() =>
            {
                Assert.That(dataTable.Rows[0]["gmc"], Is.EqualTo("0026440"));
                Assert.That(dataTable.Rows[0]["gp_code"], Is.EqualTo("99999"));
                Assert.That(dataTable.Rows[0]["surname"], Is.EqualTo("Akerman"));
                Assert.That(dataTable.Rows[0]["forename"], Is.EqualTo("Frank"));
                Assert.That(dataTable.Rows[0]["initials"], Is.EqualTo("FM"));
                Assert.That(dataTable.Rows[0]["practice_code"], Is.EqualTo("38051"));
                Assert.That(dataTable.Rows[0]["date_into_practice"], Is.EqualTo(new DateTime(2004, 4, 1)));
                Assert.That(dataTable.Rows[0]["date_out_of_practice"], Is.EqualTo(new DateTime(2009, 5, 1)));
            });
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

        var formatFile = new FileInfo(Path.Combine(TestContext.CurrentContext.TestDirectory, "Format.csv"));

        File.WriteAllText(formatFile.FullName, $@"From,To,Field,Size,DateFormat
1,5,{flatFileColumn},5");


        //Create the working directory that will be processed
        var workingDir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        var parentDir = workingDir.CreateSubdirectory("FixedWidthTests");

        var toCleanup = parentDir.GetDirectories().SingleOrDefault(d => d.Name.Equals("TestHeaderMatching"));
        toCleanup?.Delete(true);

        var loadDirectory = LoadDirectory.CreateDirectoryStructure(parentDir, "TestHeaderMatching");


        //create the file we will be trying to load
        File.WriteAllText(Path.Combine(loadDirectory.ForLoading.FullName, "file.txt"),
            testCase == FixedWidthTestCase.InsufficientLengthOfCharactersInFileToLoad
                ? @"12345
12"
                : @"12345
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

        Assert.Multiple(() =>
        {
            Assert.That(table.Exists());
            Assert.That(table.GetRowCount(), Is.EqualTo(0));
        });
        try
        {
            Regex errorRegex;
            Exception ex;

            switch (testCase)
            {
                //Success Case
                case FixedWidthTestCase.CompatibleHeaders:
                    attacher.Attach(new ThrowImmediatelyDataLoadJob(), new GracefulCancellationToken());
                    Assert.That(table.GetRowCount(), Is.EqualTo(2));
                    return; //Return


                //Error cases, set the expected error result
                case FixedWidthTestCase.MisnamedHeaders:
                    errorRegex = new Regex(
                        @"Format file \(.*Format.csv\) indicated there would be a header called 'chickenDippers' but the column did not appear in the RAW database table \(Columns in RAW were myNumber\)");
                    ex = Assert.Throws<Exception>(() =>
                        attacher.Attach(new ThrowImmediatelyDataLoadJob(), new GracefulCancellationToken()));
                    break;
                case FixedWidthTestCase.InsufficientLengthOfCharactersInFileToLoad:
                    errorRegex = new Regex(
                        @"Error on line 2 of file file.txt, the format file \(.*Format.csv\) specified that a column myNumber would be found between character positions 1 and 5 but the current line is only 2 characters long");
                    ex = Assert.Throws<FlatFileLoadException>(() =>
                        attacher.Attach(new ThrowImmediatelyDataLoadJob(), new GracefulCancellationToken()));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(testCase));
            }

            //Assert the expected error result is the real one
            Assert.That(errorRegex.IsMatch(ex.Message));
        }
        finally
        {
            table.Drop();
        }
    }
}