// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi;
using FAnsi.Discovery;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NUnit.Framework;
using Rdmp.Core.Curation;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Modules.Attachers;
using Rdmp.Core.ReusableLibraryCode.Progress;
using System;
using System.IO;
using System.Linq;
using Tests.Common;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration;

public class ExcelAttacherTests : DatabaseTests
{
    private IWorkbook _workbook;
    private LoadDirectory _loadDirectory;
    private DirectoryInfo _parentDir;
    private DiscoveredDatabase _database;
    private DiscoveredTable _table;
    private string _filename;


    [OneTimeSetUp]
    protected void Setup()
    {
        base.SetUp();

        var workingDir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        _parentDir = workingDir.CreateSubdirectory("ExcelAttacherTests");
        _database = GetCleanedServer(DatabaseType.MicrosoftSQLServer);

        var toCleanup = _parentDir.GetDirectories().SingleOrDefault(d => d.Name.Equals("EXCEL_ATTACHER"));
        toCleanup?.Delete(true);

        _loadDirectory = LoadDirectory.CreateDirectoryStructure(_parentDir, "EXCEL_ATTACHER");

        using var con = _database.Server.GetConnection();
        con.Open();

        var cmdCreateTable = _database.Server.GetCommand(
            $"CREATE Table {_database.GetRuntimeName()}..ExcelAttacher([chi] [varchar](500),[value] [varchar](500))", con);
        cmdCreateTable.ExecuteNonQuery();


        _table = _database.ExpectTable("ExcelAttacher");
    }

    [TearDown]
    public void CleanUp()
    {
        _workbook.Dispose();
        File.Delete(_filename);
    }

    [Test]
    public void Test_BasicExcelAttacher()
    {
        _workbook = new XSSFWorkbook();
        ISheet sheet = _workbook.CreateSheet("Sheet1");
        IRow row = sheet.CreateRow(0);
        row.CreateCell(0).SetCellValue("chi");
        row.CreateCell(1).SetCellValue("value");
        row = sheet.CreateRow(1);
        row.CreateCell(0).SetCellValue("1111111111");
        row.CreateCell(1).SetCellValue("some value");
        row = sheet.CreateRow(2);
        row.CreateCell(0).SetCellValue("2222222222");
        row.CreateCell(1).SetCellValue("some other value");
        _filename = Path.Combine(_loadDirectory.ForLoading.FullName, "ExcelAttacher.xlsx");
        using var fs = new FileStream(_filename, FileMode.Create, FileAccess.Write);
        _workbook.Write(fs);

        var attacher = new ExcelAttacher();
        attacher.Initialize(_loadDirectory, _database);
        attacher.FilePattern = "ExcelAttacher*";
        attacher.TableName = "ExcelAttacher";
        Assert.DoesNotThrow(() => attacher.Attach(new ThrowImmediatelyDataLoadJob(), new GracefulCancellationToken()));
        var table = _database.ExpectTable("ExcelAttacher");
        Assert.That(table.Exists());
        using var con = _database.Server.GetConnection();
        con.Open();
        var reader = _database.Server.GetCommand("Select * from ExcelAttacher", con).ExecuteReader();
        Assert.Multiple(() =>
        {
            Assert.That(reader.Read());
            Assert.That(reader["chi"], Is.EqualTo("1111111111"));
            Assert.That(reader["value"], Is.EqualTo("some value"));
        });
        Assert.Multiple(() =>
        {
            Assert.That(reader.Read());
            Assert.That(reader["chi"], Is.EqualTo("2222222222"));
            Assert.That(reader["value"], Is.EqualTo("some other value"));
        });
        attacher.LoadCompletedSoDispose(ExitCodeType.Success, ThrowImmediatelyDataLoadEventListener.Quiet);
    }

    [Test]
    public void Test_BasicExcelAttacherWithBlankFirstColumn()
    {
        _workbook = new XSSFWorkbook();
        ISheet sheet = _workbook.CreateSheet("Sheet1");
        IRow row = sheet.CreateRow(0);
        row.CreateCell(1).SetCellValue("chi");
        row.CreateCell(2).SetCellValue("value");
        row = sheet.CreateRow(1);
        row.CreateCell(1).SetCellValue("1111111111");
        row.CreateCell(2).SetCellValue("some value");
        row = sheet.CreateRow(2);
        row.CreateCell(1).SetCellValue("2222222222");
        row.CreateCell(2).SetCellValue("some other value");
        _filename = Path.Combine(_loadDirectory.ForLoading.FullName, "ExcelAttacher.xlsx");
        using var fs = new FileStream(_filename, FileMode.Create, FileAccess.Write);
        _workbook.Write(fs);

        var attacher = new ExcelAttacher();
        attacher.Initialize(_loadDirectory, _database);
        attacher.FilePattern = "ExcelAttacher*";
        attacher.TableName = "ExcelAttacher";
        Assert.DoesNotThrow(() => attacher.Attach(new ThrowImmediatelyDataLoadJob(), new GracefulCancellationToken()));
        var table = _database.ExpectTable("ExcelAttacher");
        Assert.That(table.Exists());
        using var con = _database.Server.GetConnection();
        con.Open();
        var reader = _database.Server.GetCommand("Select * from ExcelAttacher", con).ExecuteReader();
        Assert.Multiple(() =>
        {
            Assert.That(reader.Read());
            Assert.That(reader["chi"], Is.EqualTo("1111111111"));
            Assert.That(reader["value"], Is.EqualTo("some value"));
        });
        Assert.Multiple(() =>
        {
            Assert.That(reader.Read());
            Assert.That(reader["chi"], Is.EqualTo("2222222222"));
            Assert.That(reader["value"], Is.EqualTo("some other value"));
        });
        attacher.LoadCompletedSoDispose(ExitCodeType.Success, ThrowImmediatelyDataLoadEventListener.Quiet);
    }

    [Test]
    public void Test_BasicExcelAttacherWithBlankFirstColumnAndRow()
    {
        _workbook = new XSSFWorkbook();
        ISheet sheet = _workbook.CreateSheet("Sheet1");
        IRow row = sheet.CreateRow(1);
        row.CreateCell(1).SetCellValue("chi");
        row.CreateCell(2).SetCellValue("value");
        row = sheet.CreateRow(2);
        row.CreateCell(1).SetCellValue("1111111111");
        row.CreateCell(2).SetCellValue("some value");
        row = sheet.CreateRow(3);
        row.CreateCell(1).SetCellValue("2222222222");
        row.CreateCell(2).SetCellValue("some other value");
        _filename = Path.Combine(_loadDirectory.ForLoading.FullName, "ExcelAttacher.xlsx");
        using var fs = new FileStream(_filename, FileMode.Create, FileAccess.Write);
        _workbook.Write(fs);

        var attacher = new ExcelAttacher();
        attacher.Initialize(_loadDirectory, _database);
        attacher.FilePattern = "ExcelAttacher*";
        attacher.TableName = "ExcelAttacher";
        Assert.DoesNotThrow(() => attacher.Attach(new ThrowImmediatelyDataLoadJob(), new GracefulCancellationToken()));
        var table = _database.ExpectTable("ExcelAttacher");
        Assert.That(table.Exists());
        using var con = _database.Server.GetConnection();
        con.Open();
        var reader = _database.Server.GetCommand("Select * from ExcelAttacher", con).ExecuteReader();
        Assert.Multiple(() =>
        {
            Assert.That(reader.Read());
            Assert.That(reader["chi"], Is.EqualTo("1111111111"));
            Assert.That(reader["value"], Is.EqualTo("some value"));
        });
        Assert.Multiple(() =>
        {
            Assert.That(reader.Read());
            Assert.That(reader["chi"], Is.EqualTo("2222222222"));
            Assert.That(reader["value"], Is.EqualTo("some other value"));
        });
        attacher.LoadCompletedSoDispose(ExitCodeType.Success, ThrowImmediatelyDataLoadEventListener.Quiet);
    }

    [Test]
    public void Test_BasicExcelAttacherWithBlankFirstColumnAndRowAndUnnecessaryOverwrite_ColumnOnly_Number()
    {
        _workbook = new XSSFWorkbook();
        ISheet sheet = _workbook.CreateSheet("Sheet1");
        IRow row = sheet.CreateRow(1);
        row.CreateCell(1).SetCellValue("chi");
        row.CreateCell(2).SetCellValue("value");
        row = sheet.CreateRow(2);
        row.CreateCell(1).SetCellValue("1111111111");
        row.CreateCell(2).SetCellValue("some value");
        row = sheet.CreateRow(3);
        row.CreateCell(1).SetCellValue("2222222222");
        row.CreateCell(2).SetCellValue("some other value");
        _filename = Path.Combine(_loadDirectory.ForLoading.FullName, "ExcelAttacher.xlsx");
        using var fs = new FileStream(_filename, FileMode.Create, FileAccess.Write);
        _workbook.Write(fs);

        var attacher = new ExcelAttacher();
        attacher.Initialize(_loadDirectory, _database);
        attacher.FilePattern = "ExcelAttacher*";
        attacher.TableName = "ExcelAttacher";
        attacher.ColumnOffset = "1";
        Assert.DoesNotThrow(() => attacher.Attach(new ThrowImmediatelyDataLoadJob(), new GracefulCancellationToken()));
        var table = _database.ExpectTable("ExcelAttacher");
        Assert.That(table.Exists());
        using var con = _database.Server.GetConnection();
        con.Open();
        var reader = _database.Server.GetCommand("Select * from ExcelAttacher", con).ExecuteReader();
        Assert.Multiple(() =>
        {
            Assert.That(reader.Read());
            Assert.That(reader["chi"], Is.EqualTo("1111111111"));
            Assert.That(reader["value"], Is.EqualTo("some value"));
        });
        Assert.Multiple(() =>
        {
            Assert.That(reader.Read());
            Assert.That(reader["chi"], Is.EqualTo("2222222222"));
            Assert.That(reader["value"], Is.EqualTo("some other value"));
        });
        attacher.LoadCompletedSoDispose(ExitCodeType.Success, ThrowImmediatelyDataLoadEventListener.Quiet);
    }


    [Test]
    public void Test_BasicExcelAttacherWithBlankFirstColumnAndRowAndUnnecessaryOverwrite_ColumnOnly_Letter()
    {
        _workbook = new XSSFWorkbook();
        ISheet sheet = _workbook.CreateSheet("Sheet1");
        IRow row = sheet.CreateRow(1);
        row.CreateCell(1).SetCellValue("chi");
        row.CreateCell(2).SetCellValue("value");
        row = sheet.CreateRow(2);
        row.CreateCell(1).SetCellValue("1111111111");
        row.CreateCell(2).SetCellValue("some value");
        row = sheet.CreateRow(3);
        row.CreateCell(1).SetCellValue("2222222222");
        row.CreateCell(2).SetCellValue("some other value");
        _filename = Path.Combine(_loadDirectory.ForLoading.FullName, "ExcelAttacher.xlsx");
        using var fs = new FileStream(_filename, FileMode.Create, FileAccess.Write);
        _workbook.Write(fs);

        var attacher = new ExcelAttacher();
        attacher.Initialize(_loadDirectory, _database);
        attacher.FilePattern = "ExcelAttacher*";
        attacher.TableName = "ExcelAttacher";
        attacher.ColumnOffset = "B";
        Assert.DoesNotThrow(() => attacher.Attach(new ThrowImmediatelyDataLoadJob(), new GracefulCancellationToken()));
        var table = _database.ExpectTable("ExcelAttacher");
        Assert.That(table.Exists());
        using var con = _database.Server.GetConnection();
        con.Open();
        var reader = _database.Server.GetCommand("Select * from ExcelAttacher", con).ExecuteReader();
        Assert.Multiple(() =>
        {
            Assert.That(reader.Read());
            Assert.That(reader["chi"], Is.EqualTo("1111111111"));
            Assert.That(reader["value"], Is.EqualTo("some value"));
        });
        Assert.Multiple(() =>
        {
            Assert.That(reader.Read());
            Assert.That(reader["chi"], Is.EqualTo("2222222222"));
            Assert.That(reader["value"], Is.EqualTo("some other value"));
        });
        attacher.LoadCompletedSoDispose(ExitCodeType.Success, ThrowImmediatelyDataLoadEventListener.Quiet);
    }

    [Test]
    public void Test_ExcelAttacherSkipFirstColumn_Letter()
    {
        _workbook = new XSSFWorkbook();
        ISheet sheet = _workbook.CreateSheet("Sheet1");
        IRow row = sheet.CreateRow(0);
        row.CreateCell(1).SetCellValue("junk_data");
        row.CreateCell(2).SetCellValue("chi");
        row.CreateCell(3).SetCellValue("value");
        row = sheet.CreateRow(1);
        row.CreateCell(1).SetCellValue("junk_data");
        row.CreateCell(2).SetCellValue("1111111111");
        row.CreateCell(3).SetCellValue("some value");
        row = sheet.CreateRow(2);
        row.CreateCell(1).SetCellValue("junk_data");
        row.CreateCell(2).SetCellValue("2222222222");
        row.CreateCell(3).SetCellValue("some other value");
        _filename = Path.Combine(_loadDirectory.ForLoading.FullName, "ExcelAttacher.xlsx");
        using var fs = new FileStream(_filename, FileMode.Create, FileAccess.Write);
        _workbook.Write(fs);

        var attacher = new ExcelAttacher();
        attacher.Initialize(_loadDirectory, _database);
        attacher.FilePattern = "ExcelAttacher*";
        attacher.TableName = "ExcelAttacher";
        attacher.ColumnOffset = "C";
        Assert.DoesNotThrow(() => attacher.Attach(new ThrowImmediatelyDataLoadJob(), new GracefulCancellationToken()));
        var table = _database.ExpectTable("ExcelAttacher");
        Assert.That(table.Exists());
        using var con = _database.Server.GetConnection();
        con.Open();
        var reader = _database.Server.GetCommand("Select * from ExcelAttacher", con).ExecuteReader();
        Assert.Multiple(() =>
        {
            Assert.That(reader.Read());
            Assert.That(reader["chi"], Is.EqualTo("1111111111"));
            Assert.That(reader["value"], Is.EqualTo("some value"));
        });
        Assert.Multiple(() =>
        {
            Assert.That(reader.Read());
            Assert.That(reader["chi"], Is.EqualTo("2222222222"));
            Assert.That(reader["value"], Is.EqualTo("some other value"));
        });
        attacher.LoadCompletedSoDispose(ExitCodeType.Success, ThrowImmediatelyDataLoadEventListener.Quiet);
    }


    [Test]
    public void Test_ExcelAttacherSkipFirstColumn_Number()
    {
        _workbook = new XSSFWorkbook();
        ISheet sheet = _workbook.CreateSheet("Sheet1");
        IRow row = sheet.CreateRow(0);
        row.CreateCell(1).SetCellValue("junk_data");
        row.CreateCell(2).SetCellValue("chi");
        row.CreateCell(3).SetCellValue("value");
        row = sheet.CreateRow(1);
        row.CreateCell(1).SetCellValue("junk_data");
        row.CreateCell(2).SetCellValue("1111111111");
        row.CreateCell(3).SetCellValue("some value");
        row = sheet.CreateRow(2);
        row.CreateCell(1).SetCellValue("junk_data");
        row.CreateCell(2).SetCellValue("2222222222");
        row.CreateCell(3).SetCellValue("some other value");
        _filename = Path.Combine(_loadDirectory.ForLoading.FullName, "ExcelAttacher.xlsx");
        using var fs = new FileStream(_filename, FileMode.Create, FileAccess.Write);
        _workbook.Write(fs);

        var attacher = new ExcelAttacher();
        attacher.Initialize(_loadDirectory, _database);
        attacher.FilePattern = "ExcelAttacher*";
        attacher.TableName = "ExcelAttacher";
        attacher.ColumnOffset = "2";
        Assert.DoesNotThrow(() => attacher.Attach(new ThrowImmediatelyDataLoadJob(), new GracefulCancellationToken()));
        var table = _database.ExpectTable("ExcelAttacher");
        Assert.That(table.Exists());
        using var con = _database.Server.GetConnection();
        con.Open();
        var reader = _database.Server.GetCommand("Select * from ExcelAttacher", con).ExecuteReader();
        Assert.Multiple(() =>
        {
            Assert.That(reader.Read());
            Assert.That(reader["chi"], Is.EqualTo("1111111111"));
            Assert.That(reader["value"], Is.EqualTo("some value"));
        });
        Assert.Multiple(() =>
        {
            Assert.That(reader.Read());
            Assert.That(reader["chi"], Is.EqualTo("2222222222"));
            Assert.That(reader["value"], Is.EqualTo("some other value"));
        });
        attacher.LoadCompletedSoDispose(ExitCodeType.Success, ThrowImmediatelyDataLoadEventListener.Quiet);
    }


    [Test]
    public void Test_ExcelAttacherSkipFirstColumnAndRow()
    {
        _workbook = new XSSFWorkbook();
        ISheet sheet = _workbook.CreateSheet("Sheet1");
        IRow row = sheet.CreateRow(0);
        row.CreateCell(1).SetCellValue("junk_data");
        row.CreateCell(2).SetCellValue("junk_data");
        row.CreateCell(3).SetCellValue("junk_data");
        row = sheet.CreateRow(1);

        row.CreateCell(1).SetCellValue("junk_data");
        row.CreateCell(2).SetCellValue("chi");
        row.CreateCell(3).SetCellValue("value");
        row = sheet.CreateRow(2);
        row.CreateCell(1).SetCellValue("junk_data");
        row.CreateCell(2).SetCellValue("1111111111");
        row.CreateCell(3).SetCellValue("some value");
        row = sheet.CreateRow(3);
        row.CreateCell(1).SetCellValue("junk_data");
        row.CreateCell(2).SetCellValue("2222222222");
        row.CreateCell(3).SetCellValue("some other value");
        _filename = Path.Combine(_loadDirectory.ForLoading.FullName, "ExcelAttacher.xlsx");
        using var fs = new FileStream(_filename, FileMode.Create, FileAccess.Write);
        _workbook.Write(fs);

        var attacher = new ExcelAttacher();
        attacher.Initialize(_loadDirectory, _database);
        attacher.FilePattern = "ExcelAttacher*";
        attacher.TableName = "ExcelAttacher";
        attacher.ColumnOffset = "2";
        attacher.RowOffset = 2;
        Assert.DoesNotThrow(() => attacher.Attach(new ThrowImmediatelyDataLoadJob(), new GracefulCancellationToken()));
        var table = _database.ExpectTable("ExcelAttacher");
        Assert.That(table.Exists());
        using var con = _database.Server.GetConnection();
        con.Open();
        var reader = _database.Server.GetCommand("Select * from ExcelAttacher", con).ExecuteReader();
        Assert.Multiple(() =>
        {
            Assert.That(reader.Read());
            Assert.That(reader["chi"], Is.EqualTo("1111111111"));
            Assert.That(reader["value"], Is.EqualTo("some value"));
        });
        Assert.Multiple(() =>
        {
            Assert.That(reader.Read());
            Assert.That(reader["chi"], Is.EqualTo("2222222222"));
            Assert.That(reader["value"], Is.EqualTo("some other value"));
        });
        attacher.LoadCompletedSoDispose(ExitCodeType.Success, ThrowImmediatelyDataLoadEventListener.Quiet);
    }

    [Test]
    public void Test_ExcelAttacherBadColumnOffset()
    {
        _workbook = new XSSFWorkbook();
        ISheet sheet = _workbook.CreateSheet("Sheet1");
        IRow row = sheet.CreateRow(0);
        row.CreateCell(1).SetCellValue("junk_data");
        row.CreateCell(2).SetCellValue("junk_data");
        row.CreateCell(3).SetCellValue("junk_data");
        row = sheet.CreateRow(1);

        row.CreateCell(1).SetCellValue("junk_data");
        row.CreateCell(2).SetCellValue("chi");
        row.CreateCell(3).SetCellValue("value");
        row = sheet.CreateRow(2);
        row.CreateCell(1).SetCellValue("junk_data");
        row.CreateCell(2).SetCellValue("1111111111");
        row.CreateCell(3).SetCellValue("some value");
        row = sheet.CreateRow(3);
        row.CreateCell(1).SetCellValue("junk_data");
        row.CreateCell(2).SetCellValue("2222222222");
        row.CreateCell(3).SetCellValue("some other value");
        _filename = Path.Combine(_loadDirectory.ForLoading.FullName, "ExcelAttacher.xlsx");
        using var fs = new FileStream(_filename, FileMode.Create, FileAccess.Write);
        _workbook.Write(fs);

        var attacher = new ExcelAttacher();
        attacher.Initialize(_loadDirectory, _database);
        attacher.FilePattern = "ExcelAttacher*";
        attacher.TableName = "ExcelAttacher";
        attacher.ColumnOffset = "2.987";
        attacher.RowOffset = 2;
        Assert.Throws<Exception>(() => attacher.Attach(new ThrowImmediatelyDataLoadJob(), new GracefulCancellationToken()),"test");
    }


}
