using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.AtomicCommands.WindowArranging;
using DataExportLibrary.Data.DataTables;
using MapsDirectlyToDatabaseTable;

namespace CatalogueManager.ItemActivation.Arranging
{
    public interface IArrangeWindows
    {
        //Advanced cases where you want to show multiple windows at once
        void SetupEditCatalogue(object sender, Catalogue catalogue);
        void SetupEditDataExtractionProject(object sender, Project project);
        void SetupEditLoadMetadata(object sender, LoadMetadata loadMetadata);


        //basic case where you only want to Emphasise and Activate it (after closing all other windows)
        void SetupEditAnything(object sender, IMapsDirectlyToDatabaseTable o);
    }
}
