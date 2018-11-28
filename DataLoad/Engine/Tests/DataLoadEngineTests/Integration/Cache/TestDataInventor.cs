using System;
using System.Collections.Generic;
using System.IO;
using CachingEngine.PipelineExecution.Sources;
using CachingEngine.Requests;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataFlowPipeline;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace DataLoadEngineTests.Integration.Cache
{
    public class TestDataInventor : CacheSource<TestDataWriterChunk>
    {
        Random r = new Random();
        
        [DemandsInitialization("Directory to create files into",Mandatory=true)]
        public string WorkingFolder { get; set; }

        public override TestDataWriterChunk DoGetChunk(ICacheFetchRequest request,IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            // don't load anything for today onwards
            var today = DateTime.Now.Subtract(DateTime.Now.TimeOfDay);
            if (Request.Start > today)
                return null;

            DateTime currentDay = Request.Start;
            
            List<FileInfo> toReturn = new List<FileInfo>();

            while(currentDay <= Request.End)
            {
                toReturn.Add(GetFileForDay(currentDay));
                currentDay = currentDay.AddDays(1);
            }

            return new TestDataWriterChunk(Request,toReturn.ToArray());
        }

        private FileInfo GetFileForDay(DateTime currentDay)
        {
            string filename = Path.Combine(WorkingFolder,currentDay.ToString("yyyyMMdd") + ".csv");

            string contents = "MyRand,DateOfRandom" + Environment.NewLine;
            for (int i = 0; i < 100; i++)
                contents += r.Next(10000) + "," + currentDay.ToString("yyyy-MM-dd") + Environment.NewLine;

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
}