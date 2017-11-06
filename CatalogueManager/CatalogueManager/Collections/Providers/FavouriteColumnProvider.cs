using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightIdeasSoftware;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using MapsDirectlyToDatabaseTable;

namespace CatalogueManager.Collections.Providers
{
    public class FavouriteColumnProvider
    {
        private readonly IActivateItems _activator;
        private readonly TreeListView _tlv;
        OLVColumn _olvFavourite;

        private Bitmap _starFull;
        private Bitmap _starHollow;



        public FavouriteColumnProvider(IActivateItems activator,TreeListView tlv)
        {
            _activator = activator;
            _tlv = tlv;

            _starFull = CatalogueIcons.Favourite;
            _starHollow = CatalogueIcons.StarHollow;
        }

        public OLVColumn CreateColumn()
        {
            _olvFavourite = new OLVColumn("Favourite", null);
            _olvFavourite.Text = "Favourite";
            _olvFavourite.ImageGetter += FavouriteImageGetter;
            _olvFavourite.IsEditable = false;
            _tlv.CellClick += OnCellClick;
            
            _tlv.AllColumns.Add(_olvFavourite);
            _tlv.RebuildColumns();

            return _olvFavourite;
        }

        private void OnCellClick(object sender, CellClickEventArgs cellClickEventArgs)
        {
            var col = cellClickEventArgs.Column;
            var o = cellClickEventArgs.Model as IMapsDirectlyToDatabaseTable;


            if (col == _olvFavourite && o != null)
            {
                if (_activator.FavouritesProvider.IsFavourite(o))
                    _activator.FavouritesProvider.RemoveFavourite(this, o);
                else
                    _activator.FavouritesProvider.AddFavourite(this, o);
                
                try
                {
                    _tlv.RefreshObject(o);
                }
                catch (ArgumentException)
                {
                    
                }
            }
        }

        private object FavouriteImageGetter(object rowobject)
        {
            var o = rowobject as IMapsDirectlyToDatabaseTable;

            if (o != null)
                return _activator.FavouritesProvider.IsFavourite(o) ? _starFull : _starHollow;
                    

            return null;
        }

    }
}
