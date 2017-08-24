using System;
using System.Windows.Forms;
using MapsDirectlyToDatabaseTable;

namespace CatalogueManager.ItemActivation.Emphasis
{
    public class EmphasiseEventArgs:EventArgs
    {
        public EmphasiseRequest Request { get; set; }
        public Form FormRequestingActivation { get; set; }

        public EmphasiseEventArgs(EmphasiseRequest request)
        {
            Request = request;
        }
    }
}