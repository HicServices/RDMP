using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.ANOEngineering;
using DataLoadEngine.Attachers;
using ReusableLibraryCode.Checks;

namespace LoadModules.Generic.Mutilators.Dilution.Operations
{
    /// <summary>
    /// MEF discoverable version of IDilutionOperation
    /// </summary>
    [InheritedExport(typeof(IDilutionOperation))]
    [InheritedExport(typeof(ICheckable))]
    public interface IPluginDilutionOperation:IDilutionOperation
    {
    }
}
