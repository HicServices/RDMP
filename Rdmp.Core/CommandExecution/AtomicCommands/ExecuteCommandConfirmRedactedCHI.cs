using HICPlugin.Curation.Data;
using Rdmp.Core.Curation.Data;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandConfirmRedactedCHI : BasicCommandExecution, IAtomicCommand
{
    RedactedCHI _redactedCHI;
    public ExecuteCommandConfirmRedactedCHI(IBasicActivateItems activator, [DemandsInitialization("redactionto confirm")]RedactedCHI redaction): base(activator)
    {
        _redactedCHI = redaction;
    }

    public override void Execute()
    {
        base.Execute();
        _redactedCHI.DeleteInDatabase();
    }
}
