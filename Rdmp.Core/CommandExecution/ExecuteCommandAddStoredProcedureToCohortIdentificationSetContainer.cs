using System.Data;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution;

public class ExecuteCommandAddStoredProcedureToCohortIdentificationSetContainer : BasicCommandExecution
{
    private readonly CohortAggregateContainer _targetCohortAggregateContainer;
    private string _storedProcedureName;
    private IBasicActivateItems _activator;

    private readonly string[] _knownStoredProcedures = [];

    public ExecuteCommandAddStoredProcedureToCohortIdentificationSetContainer(IBasicActivateItems activator,
        [DemandsInitialization("The container you want to add the set into")]
        CohortAggregateContainer targetCohortAggregateContainer,
        [DemandsInitialization("The name of the stored procedure")]
        string storedProcedureName = null
    )
    {
        _activator = activator;
        _targetCohortAggregateContainer = targetCohortAggregateContainer;
        _storedProcedureName = storedProcedureName;

        using var foundStoredProcedures = new DataTable();


        //todo need to figure out how to grab all the stored procedures available to us
        //using var con = (SqlConnection)db.Server.GetConnection();
        //using (var cmd = new SqlCommand("SELECT name from [dbo].[sysobjects] where (type='p')", con))
        //using (var da = new SqlDataAdapter(cmd))
        //    da.Fill(foundStoredProcedures);

        //       SELECT name
        // FROM dbo.sysobjects
        //WHERE(type = 'P')

        if (_knownStoredProcedures is null || _knownStoredProcedures.Length == 0)
            SetImpossible("There are no stored procedures found");
        if (targetCohortAggregateContainer.ShouldBeReadOnly(out var reason))
            SetImpossible(reason);
        if (storedProcedureName != null)
        {
            //todo check the sotred procedure exists
        }

        UseTripleDotSuffix = true;
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return iconProvider.GetImage(RDMPConcept.Catalogue, OverlayKind.Add);
    }

    public override void Execute()
    {
        base.Execute();
        if (_storedProcedureName is null)
            //make the user pick a stored procedure
            if (!BasicActivator.SelectObject(new DialogArgs
                {
                    WindowTitle = "Add a Stored Procedure to the Container",
                    TaskDescription =
                        $"Choose which Stored Procedure to add to the cohort container '{_targetCohortAggregateContainer.Name}'."
                }, _knownStoredProcedures, out _storedProcedureName))
                // user didn't pick one
                return;
        Execute(_storedProcedureName);
    }

    public void Execute(string storedProcedureName)
    {
    }
}