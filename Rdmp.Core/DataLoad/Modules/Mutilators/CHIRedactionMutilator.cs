using FAnsi.Discovery;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Runtime;
using Rdmp.Core.DataLoad.Engine.Mutilators;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Rdmp.Core.DataLoad.Modules.Mutilators;

internal class CHIRedactionMutilator : IPluginMutilateDataTables
{

    private DiscoveredDatabase _db;
    private LoadStage _loadStage;
    private readonly Dictionary<string, List<string>> _allowLists = null;



    [DemandsInitialization("Location of Allow list file that lists columns to ignore", DemandType = DemandType.Unspecified,
        Mandatory = false)]
    public string AllowListFileLocation { get; set; }



    [DemandsInitialization("Automatically redact found CHIS. Otherwise, a warning is raised", DemandType = DemandType.Unspecified)]
    public bool Redact { get; set; } = true;
    public void Check(ICheckNotifier notifier)
    {
        if (AllowListFileLocation != null)
        {
            var allowListFileContent = File.ReadAllText(AllowListFileLocation);
            var deserializer = new DeserializerBuilder().Build();
            var yamlObject = deserializer.Deserialize<Dictionary<string, List<string>>>(allowListFileContent);
            foreach (var (cat, columns) in yamlObject)
            {
                _allowLists.Add(cat, columns);
            }
        }
    }

    public void Initialize(DiscoveredDatabase dbInfo, LoadStage loadStage)
    {
        _db = dbInfo;
        _loadStage = loadStage;
    }

    public void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventsListener)
    {
    }

    public ExitCodeType Mutilate(IDataLoadJob job)
    {
        var redactor = new ExecuteCHIRedactionStage(job, _db, _loadStage);
        return redactor.Execute(Redact, _allowLists);
    }
}
