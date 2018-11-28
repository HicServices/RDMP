using System;
using System.Data;
using System.Windows.Forms;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;

namespace CatalogueManager.PipelineUIs.DemandsInitializationUIs.ArgumentValueControls
{
    public interface IArgumentValueUI : IContainerControl
    {
        void SetUp(ArgumentValueUIArgs args);
    }
}
