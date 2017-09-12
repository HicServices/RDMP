using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;

namespace RDMPObjectVisualisation.DemandsInitializationUIs.ArgumentValueControls
{
    public interface IArgumentValueUI : IContainerControl
    {
        void SetUp(Argument argument, DemandsInitializationAttribute demand, DataTable previewIfAny);
    }
}
