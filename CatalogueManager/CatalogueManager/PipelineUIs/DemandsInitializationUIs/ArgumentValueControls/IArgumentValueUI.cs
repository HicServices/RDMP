using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;

namespace CatalogueManager.PipelineUIs.DemandsInitializationUIs.ArgumentValueControls
{
    public interface IArgumentValueUI : IContainerControl
    {
        void SetUp(Argument argument, RequiredPropertyInfo requirement, DataTable previewIfAny);
    }
}
