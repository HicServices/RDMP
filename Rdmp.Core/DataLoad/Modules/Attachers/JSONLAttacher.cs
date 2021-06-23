using Newtonsoft.Json;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.Attachers;
using Rdmp.Core.DataLoad.Engine.Job;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;
using System.IO;

namespace Rdmp.Core.DataLoad.Modules.Attachers
{
    public class JsonLAttacher : Attacher
    {
        [DemandsInitialization("The root table in RAW that will be loaded by this Attacher", Mandatory = true)]
        public TableInfo RootTable { get; set; }

        [DemandsInitialization("Pattern to match files in forLoading in.  Defaults to *.json", DefaultValue = "*.json", Mandatory = true)]
        public string FilePattern { get; set; } = "*.json";

        [DemandsInitialization(@"Map for sub attributes to tables.  Format is MyProp=MyTable,MyProp2=MyTable2.  
If not specified then properties must exactly match table names")]
        public string AttributeTableMap { get; set; }

        public JsonLAttacher():base(true)
        {

        }

        public override ExitCodeType Attach(IDataLoadJob job, GracefulCancellationToken cancellationToken)
        {
            foreach(var file in LoadDirectory.ForLoading.GetFiles(FilePattern))
            {
                using (var sr = new StreamReader(file.FullName))
                {
                    var jsonReader = new JsonTextReader(sr)
                    {
                        SupportMultipleContent = true // This is important!
                    };

                    while (jsonReader.Read())
                    {
                        // load tables
                    }
                }
            }
            

            return ExitCodeType.Success;
        }

        public override void Check(ICheckNotifier notifier)
        {
        }

        public override void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventListener)
        {
        }
    }
}
