using System;
using System.Text;
using BrightIdeasSoftware;
using CatalogueLibrary.Data.Aggregation;
using MapsDirectlyToDatabaseTable;

namespace CatalogueManager.Collections.Providers
{
    /// <summary>
    /// Handles creating the ID column in a tree list view where the ID is populated for all models of Type IMapsDirectlyToDatabaseTable and null
    /// for all others
    /// </summary>
    public class IDColumnProvider
    {
        private readonly TreeListView _tree;

        public IDColumnProvider(TreeListView tree)
        {
            _tree = tree;
        }

        private object IDColumnAspectGetter(object rowObject)
        {
            var imaps = rowObject as IMapsDirectlyToDatabaseTable;

            if (imaps == null)
                return null;

            return imaps.ID;
        }

        public OLVColumn CreateColumn()
        {
            var toReturn = new OLVColumn();
            toReturn.Text = "ID";
            toReturn.IsVisible = false;
            toReturn.AspectGetter += IDColumnAspectGetter;
            toReturn.IsEditable = false;
            return toReturn;
        }
    }
}
