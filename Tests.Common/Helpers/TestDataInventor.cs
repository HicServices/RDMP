// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using Rdmp.Core.Caching.Pipeline.Sources;
using Rdmp.Core.Caching.Requests;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Tests.Common.Helpers;

/// <summary>
/// Implementation of <see cref="CacheSource{T}"/> which creates csv files with random data in them.  This class can be used if you need
/// to test running a caching pipeline
/// </summary>
public class TestDataInventor : CacheSource<TestDataWriterChunk>
{
    Random r = new Random();
        
    /// <summary>
    /// The path in which to create random files
    /// </summary>
    [DemandsInitialization("Directory to create files into",Mandatory=true)]
    public string WorkingFolder { get; set; }

    public override TestDataWriterChunk DoGetChunk(ICacheFetchRequest request,IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
    {
        // don't load anything for today onwards
        var today = DateTime.Now.Subtract(DateTime.Now.TimeOfDay);
        if (Request.Start > today)
            return null;

        var currentDay = Request.Start;
            
        var toReturn = new List<FileInfo>();

        while(currentDay <= Request.End)
        {
            toReturn.Add(GetFileForDay(currentDay));
            currentDay = currentDay.AddDays(1);
        }

        return new TestDataWriterChunk(Request,toReturn.ToArray());
    }

    private FileInfo GetFileForDay(DateTime currentDay)
    {
        var filename = Path.Combine(WorkingFolder, $"{currentDay:yyyyMMdd}.csv");

        var contents = $"MyRand,DateOfRandom{Environment.NewLine}";
        for (var i = 0; i < 100; i++)
#pragma warning disable SCS0005 // Weak random generator - This is not a secure context as it is simply a test helper.
            contents += $"{r.Next(10000)},{currentDay:yyyy-MM-dd}{Environment.NewLine}";
#pragma warning restore SCS0005 // Weak random generator

        File.WriteAllText(filename, contents);
        return new FileInfo(filename);
    }

    public override void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
    {
            
    }

    public override void Abort(IDataLoadEventListener listener)
    {
            
    }

    public override TestDataWriterChunk TryGetPreview()
    {
        var dt = DateTime.Now.AddYears(-200);

        return new TestDataWriterChunk(new CacheFetchRequest(null, dt){ChunkPeriod = new TimeSpan(1,0,0)}, new []{GetFileForDay(dt)});
    }

    public override void Check(ICheckNotifier notifier)
    {
    }
}