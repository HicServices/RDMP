// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using BrightIdeasSoftware;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.UI.ItemActivation;

namespace Rdmp.UI.Collections.Providers;

/// <summary>
/// Handles creating the 'Favourite' column in <see cref="TreeListView"/>.  This column depicts whether a given RDMP object is a favourite
/// of the user (see <see cref="Favourite"/>).
/// </summary>
public class FavouriteColumnProvider
{
    private readonly IActivateItems _activator;
    private readonly TreeListView _tlv;
    private OLVColumn _olvFavourite;

    private Bitmap _starFull;
    private Bitmap _starHollow;


    public FavouriteColumnProvider(IActivateItems activator, TreeListView tlv)
    {
        _activator = activator;
        _tlv = tlv;

        _starFull = CatalogueIcons.Favourite.ImageToBitmap();
        _starHollow = CatalogueIcons.StarHollow.ImageToBitmap();
    }

    public OLVColumn CreateColumn()
    {
        _olvFavourite = new OLVColumn("Favourite", null)
        {
            Text = "Favourite"
        };
        _olvFavourite.ImageGetter += FavouriteImageGetter;
        _olvFavourite.IsEditable = false;
        _olvFavourite.Sortable = true;

        // setup value of column as 1 (favourite) or 0 (not favourite)
        _olvFavourite.AspectGetter = FavouriteAspectGetter;
        // but don't actually write that value when rendering (just use for sort etc)
        _olvFavourite.AspectToStringConverter = st => "";

        _tlv.CellClick += OnCellClick;

        _tlv.AllColumns.Add(_olvFavourite);
        _tlv.RebuildColumns();

        return _olvFavourite;
    }

    private void OnCellClick(object sender, CellClickEventArgs cellClickEventArgs)
    {
        var col = cellClickEventArgs.Column;


        if (col == _olvFavourite && cellClickEventArgs.Model is DatabaseEntity o)
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
                Console.WriteLine("Unable to refresh favourite column");
            }
        }
    }

    private Bitmap FavouriteImageGetter(object rowobject) => rowobject is DatabaseEntity o
        ? _activator.FavouritesProvider.IsFavourite(o) ? _starFull : _starHollow
        : null;

    private object FavouriteAspectGetter(object rowobject) => rowobject is DatabaseEntity o
        ? _activator.FavouritesProvider.IsFavourite(o) ? 1 : 0
        : (object)null;
}