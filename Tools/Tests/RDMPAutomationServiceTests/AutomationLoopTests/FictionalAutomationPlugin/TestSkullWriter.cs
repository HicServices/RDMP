using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Automation;
using CatalogueLibrary.Repositories;
using RDMPAutomationService;
using RDMPAutomationService.Interfaces;
using ReusableLibraryCode.Progress;

namespace RDMPAutomationServiceTests.AutomationLoopTests.FictionalAutomationPlugin
{
    public class TestSkullWriter : IAutomateable
    {
        private readonly AutomationServiceSlot _slot;
        private readonly DateTime _timeToGenerateFor;
        private readonly IDataLoadEventListener _listener;

        public TestSkullWriter(AutomationServiceSlot slot,DateTime timeToGenerateFor, IDataLoadEventListener listener)
        {
            _slot = slot;
            _timeToGenerateFor = timeToGenerateFor;
            _listener = listener;
        }

        public OnGoingAutomationTask GetTask()
        {
            return new OnGoingAutomationTask(_slot.AddNewJob( AutomationJobType.UserCustomPipeline, "writing out a skull image - this is a unit test task"),this);
        }

        public void RunTask(OnGoingAutomationTask task)
        {
            try
            {
                task.Job.TickLifeline();
                task.Job.SetLastKnownStatus(AutomationJobStatus.Running);

                string filename = GetFilenameForToday();
            
                _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Creating file:" +filename));


                using (Stream sfile = typeof(TestSkullWriter).Assembly.GetManifestResourceStream("RDMPAutomationServiceTests.AutomationLoopTests.FictionalAutomationPlugin.2brains.png"))
                {
                    byte[] buf = new byte[sfile.Length];
                    sfile.Read(buf, 0, Convert.ToInt32(sfile.Length));

                    using (FileStream fs = File.Create(filename))
                    {
                        fs.Write(buf, 0, Convert.ToInt32(sfile.Length));
                        fs.Close();
                    }
                }

                task.Job.SetLastKnownStatus(AutomationJobStatus.Finished);
                task.Job.DeleteInDatabase();//it is done now.
            }
            catch (Exception e)
            {
                task.Job.SetLastKnownStatus(AutomationJobStatus.Cancelled);
                new AutomationServiceException((ICatalogueRepository) _slot.Repository, e);
            }
        }

        private string GetFilenameForToday()
        {
            return _timeToGenerateFor.ToString("yy_MM_dd_HHmmss") + "_skull.png";
        }
    }
}
