// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.DataExport.DataExtraction.Pipeline;
using Rdmp.Core.DataFlowPipeline;
using System;
using System.IO;
using System.Linq;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.Tests.DataExport.DataExtraction;

internal class SimpleFileExtractorTests
{
    private SimpleFileExtractor _extractor;
    private DirectoryInfo _inDir;
    private DirectoryInfo _outDir;
    private DirectoryInfo _inDirSub1;
    private DirectoryInfo _inDirSub2;

    [SetUp]
    public void SetUp()
    {
            
        _extractor = new SimpleFileExtractor();

        _inDir = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory,"In"));
        _outDir = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory,"Out"));

        if(_inDir.Exists)
            _inDir.Delete(true);
        if(_outDir.Exists)
            _outDir.Delete(true);

        _inDir.Create();
        _outDir.Create();

        _extractor.LocationOfFiles = _inDir;

        File.WriteAllText(Path.Combine(_inDir.FullName,"blah.txt"),"fff");
        File.WriteAllText(Path.Combine(_inDir.FullName,"blah2.txt"),"fff");
        File.WriteAllText(Path.Combine(_inDir.FullName,"Pat1.txt"),"fff");

        _inDirSub1 = _inDir.CreateSubdirectory("Sub1");
        _inDirSub2 = _inDir.CreateSubdirectory("Sub2");

        File.WriteAllText(Path.Combine(_inDirSub1.FullName,"subBlah.txt"),"fff");
        File.WriteAllText(Path.Combine(_inDirSub2.FullName,"subBlah2.txt"),"fff");
    }

    [Test]
    public void AllFiles()
    {
        _extractor.Directories = false;
        _extractor.Pattern = "*";
        _extractor.OutputDirectoryName = _outDir.FullName;
        _extractor.Check(new ThrowImmediatelyCheckNotifier());

        FileAssert.DoesNotExist(Path.Combine(_outDir.FullName,"blah.txt"));
        FileAssert.DoesNotExist(Path.Combine(_outDir.FullName,"blah2.txt"));

        _extractor.MoveAll(_outDir,ThrowImmediatelyDataLoadEventListener.Quiet,new GracefulCancellationToken());

        FileAssert.Exists(Path.Combine(_outDir.FullName,"blah.txt"));
        FileAssert.Exists(Path.Combine(_outDir.FullName,"blah2.txt"));
    }
    [Test]
    public void OneFile()
    {
        _extractor.Directories = false;
        _extractor.Pattern = "blah.*";
        _extractor.OutputDirectoryName = _outDir.FullName;
        _extractor.Check(new ThrowImmediatelyCheckNotifier());

        FileAssert.DoesNotExist(Path.Combine(_outDir.FullName,"blah.txt"));
        FileAssert.DoesNotExist(Path.Combine(_outDir.FullName,"blah2.txt"));

        _extractor.MoveAll(_outDir,ThrowImmediatelyDataLoadEventListener.Quiet,new GracefulCancellationToken());

        FileAssert.Exists(Path.Combine(_outDir.FullName,"blah.txt"));
        FileAssert.DoesNotExist(Path.Combine(_outDir.FullName,"blah2.txt"));
        DirectoryAssert.DoesNotExist(Path.Combine(_outDir.FullName,"Sub1"));
        DirectoryAssert.DoesNotExist(Path.Combine(_outDir.FullName,"Sub2"));
    }
    [Test]
    public void AllDirs()
    {
        _extractor.Directories = true;
        _extractor.Pattern = "*";
        _extractor.OutputDirectoryName = _outDir.FullName;
        _extractor.Check(new ThrowImmediatelyCheckNotifier());

        FileAssert.DoesNotExist(Path.Combine(_outDir.FullName,"blah.txt"));
        FileAssert.DoesNotExist(Path.Combine(_outDir.FullName,"blah2.txt"));

        _extractor.MoveAll(_outDir,ThrowImmediatelyDataLoadEventListener.Quiet,new GracefulCancellationToken());

        FileAssert.DoesNotExist(Path.Combine(_outDir.FullName,"blah.txt"));
        FileAssert.DoesNotExist(Path.Combine(_outDir.FullName,"blah2.txt"));
        DirectoryAssert.Exists(Path.Combine(_outDir.FullName,"Sub1"));
        DirectoryAssert.Exists(Path.Combine(_outDir.FullName,"Sub2"));
    }
    [Test]
    public void OneDir()
    {
        _extractor.Directories = true;
        _extractor.Pattern = "*1";
        _extractor.OutputDirectoryName = _outDir.FullName;
        _extractor.Check(new ThrowImmediatelyCheckNotifier());

        FileAssert.DoesNotExist(Path.Combine(_outDir.FullName,"blah.txt"));
        FileAssert.DoesNotExist(Path.Combine(_outDir.FullName,"blah2.txt"));

        _extractor.MoveAll(_outDir,ThrowImmediatelyDataLoadEventListener.Quiet,new GracefulCancellationToken());

        FileAssert.DoesNotExist(Path.Combine(_outDir.FullName,"blah.txt"));
        FileAssert.DoesNotExist(Path.Combine(_outDir.FullName,"blah2.txt"));
        DirectoryAssert.Exists(Path.Combine(_outDir.FullName,"Sub1"));
        DirectoryAssert.DoesNotExist(Path.Combine(_outDir.FullName,"Sub2"));
    }
    [Test]
    public void PatientFiles()
    {
        _extractor.PerPatient = true;
        _extractor.Directories = false;
        _extractor.Pattern = "$p.txt";
        _extractor.OutputDirectoryName = _outDir.FullName;
        _extractor.Check(new ThrowImmediatelyCheckNotifier());

        FileAssert.DoesNotExist(Path.Combine(_outDir.FullName,"blah.txt"));
        FileAssert.DoesNotExist(Path.Combine(_outDir.FullName,"blah2.txt"));

        _extractor.MovePatient("Pat1","Rel1",_outDir,ThrowImmediatelyDataLoadEventListener.QuietPicky,new GracefulCancellationToken());
            
        FileAssert.DoesNotExist(Path.Combine(_outDir.FullName,"blah.txt"));
        FileAssert.DoesNotExist(Path.Combine(_outDir.FullName,"blah2.txt"));
        FileAssert.Exists(Path.Combine(_outDir.FullName,"Rel1.txt"));
    }
    [Test]
    public void PatientFileMissingOne()
    {
        _extractor.PerPatient = true;
        _extractor.Directories = false;
        _extractor.Pattern = "$p.txt";
        _extractor.OutputDirectoryName = _outDir.FullName;
        _extractor.Check(new ThrowImmediatelyCheckNotifier());

        FileAssert.DoesNotExist(Path.Combine(_outDir.FullName,"blah.txt"));
        FileAssert.DoesNotExist(Path.Combine(_outDir.FullName,"blah2.txt"));

        var mem = new ToMemoryDataLoadEventListener(true);
            
        _extractor.MovePatient("Pat1","Rel1",_outDir,mem,new GracefulCancellationToken());
        _extractor.MovePatient("Pat2","Rel2",_outDir,mem,new GracefulCancellationToken());
            
        FileAssert.DoesNotExist(Path.Combine(_outDir.FullName,"blah.txt"));
        FileAssert.DoesNotExist(Path.Combine(_outDir.FullName,"blah2.txt"));
        FileAssert.Exists(Path.Combine(_outDir.FullName,"Rel1.txt"));

        Assert.AreEqual(ProgressEventType.Warning,mem.GetWorst());
            
        StringAssert.StartsWith("No Files were found matching Pattern Pat2.txt in ",mem.GetAllMessagesByProgressEventType()[ProgressEventType.Warning].Single().Message);
    }
    [Test]
    public void PatientDirs()
    {
        _extractor.PerPatient = true;
        _extractor.Directories = true;
        _extractor.Pattern = "$p";
        _extractor.OutputDirectoryName = _outDir.FullName;
        _extractor.Check(new ThrowImmediatelyCheckNotifier());

        FileAssert.DoesNotExist(Path.Combine(_outDir.FullName,"blah.txt"));
        FileAssert.DoesNotExist(Path.Combine(_outDir.FullName,"blah2.txt"));

        _extractor.MovePatient("Sub1","Rel1",_outDir,ThrowImmediatelyDataLoadEventListener.QuietPicky,new GracefulCancellationToken());
            
        FileAssert.DoesNotExist(Path.Combine(_outDir.FullName,"blah.txt"));
        FileAssert.DoesNotExist(Path.Combine(_outDir.FullName,"blah2.txt"));
        DirectoryAssert.Exists(Path.Combine(_outDir.FullName,"Rel1"));
        DirectoryAssert.DoesNotExist(Path.Combine(_outDir.FullName,"Rel2"));
        DirectoryAssert.DoesNotExist(Path.Combine(_outDir.FullName,"Sub1"));
        DirectoryAssert.DoesNotExist(Path.Combine(_outDir.FullName,"Sub2"));
    }
    [Test]
    public void PatientBothDirs()
    {
        _extractor.PerPatient = true;
        _extractor.Directories = true;
        _extractor.Pattern = "$p";
        _extractor.OutputDirectoryName = _outDir.FullName;
        _extractor.Check(new ThrowImmediatelyCheckNotifier());

        FileAssert.DoesNotExist(Path.Combine(_outDir.FullName,"blah.txt"));
        FileAssert.DoesNotExist(Path.Combine(_outDir.FullName,"blah2.txt"));

        _extractor.MovePatient("Sub1","Rel1",_outDir, ThrowImmediatelyDataLoadEventListener.QuietPicky, new GracefulCancellationToken());
        _extractor.MovePatient("Sub2","Rel2",_outDir, ThrowImmediatelyDataLoadEventListener.QuietPicky, new GracefulCancellationToken());

        // does not exist
        var ex = Assert.Throws<Exception>(()=>_extractor.MovePatient("Sub3", "Rel3", _outDir, ThrowImmediatelyDataLoadEventListener.QuietPicky, new GracefulCancellationToken()));
        Assert.AreEqual($"No Directories were found matching Pattern Sub3 in {_inDir.FullName}.  For private identifier 'Sub3'", ex.Message);
            
        // if not throwing on warnings then a missing sub just passes through and is ignored
        _extractor.MovePatient("Sub3", "Rel3", _outDir, ThrowImmediatelyDataLoadEventListener.Quiet, new GracefulCancellationToken());

        FileAssert.DoesNotExist(Path.Combine(_outDir.FullName,"blah.txt"));
        FileAssert.DoesNotExist(Path.Combine(_outDir.FullName,"blah2.txt"));
        DirectoryAssert.Exists(Path.Combine(_outDir.FullName,"Rel1"));
        DirectoryAssert.Exists(Path.Combine(_outDir.FullName,"Rel2"));
        DirectoryAssert.DoesNotExist(Path.Combine(_outDir.FullName,"Sub1"));
        DirectoryAssert.DoesNotExist(Path.Combine(_outDir.FullName,"Sub2"));
    }
}