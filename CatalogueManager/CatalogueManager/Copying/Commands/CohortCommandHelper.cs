using System;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cohort;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTableUI;

namespace CatalogueManager.Copying.Commands
{
    public class CohortCommandHelper
    {

        public static ExtractionInformation PickOneExtractionIdentifier(Catalogue c, ExtractionInformation[] candidates)
        {
            if (candidates.Length == 0)
                throw new Exception("None of the ExtractionInformations in Catalogue " + c + " are marked IsExtractionIdentifier.  You will need to edit the Catalogue in CatalogueManager and select one of the columns in the dataset as the extraction identifier");

            MessageBox.Show("Dataset " + c + " has " + candidates.Length + " columns marked IsExtractionInformation, which one do you want to do cohort identification on?");

            var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(candidates, false, false);

            dialog.ShowDialog();

            if (dialog.DialogResult == DialogResult.OK)
                return (ExtractionInformation)dialog.Selected;


            throw new Exception("User refused to choose an extraction identifier");
        }
    }
}