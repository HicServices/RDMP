using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using Rdmp.Core.Curation.DataHelper.RegexRedaction;
using System.Text.RegularExpressions;
using Rdmp.Core.Curation.Data;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandAddNewRegexRedactionConfiguration : BasicCommandExecution, IAtomicCommand
{

    private string _name;
    private string _redactionPattern;
    private string _redactionString;
    private string _description;
    private IBasicActivateItems _activator;


    public ExecuteCommandAddNewRegexRedactionConfiguration(IBasicActivateItems activator, [DemandsInitialization("Name")] string name, [DemandsInitialization("pattern")] string redactionPattern, [DemandsInitialization("redaction")] string redactionString, string description = null) : base(activator)
    {
        _activator = activator;
        _name = name;
        _redactionPattern = redactionPattern;
        _redactionString = redactionString;
        _description = description;
    }

    public override void Execute()
    {
        base.Execute();
        var config = new RegexRedactionConfiguration(_activator.RepositoryLocator.CatalogueRepository, _name, new Regex(_redactionPattern), _redactionString, _description);
        config.SaveToDatabase();
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
      iconProvider.GetImage(RDMPConcept.StandardRegex, OverlayKind.Add);
}
