using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using MapsDirectlyToDatabaseTable;

namespace CatalogueManager.Collections.Providers
{
    public class FavouritesProvider
    {
        private readonly IActivateItems _activator;
        private readonly ICatalogueRepository _catalogueRepository;
        public List<Favourite> CurrentFavourites { get; set; }
        
        public FavouritesProvider(IActivateItems activator, ICatalogueRepository catalogueRepository)
        {
            _activator = activator;
            _catalogueRepository = catalogueRepository;
            CurrentFavourites = catalogueRepository.GetAllObjects<Favourite>("WHERE Username='" + Environment.UserName + "'").ToList();
        }

        public void AddFavourite(object sender, IMapsDirectlyToDatabaseTable o)
        {
            //it's already a favourite
            if(IsFavourite(o))
                return;
            
            var newFavourite = new Favourite(_catalogueRepository, o);
            CurrentFavourites.Add(newFavourite);
            _activator.RefreshBus.Publish(sender,new RefreshObjectEventArgs(newFavourite));
        }

        public void RemoveFavourite(object sender, IMapsDirectlyToDatabaseTable o)
        {
            Favourite toRemove = CurrentFavourites.SingleOrDefault(f => f.IsReferenceTo(o));

            if (toRemove != null)
            {
                CurrentFavourites.Remove(toRemove);
                toRemove.DeleteInDatabase();
                _activator.RefreshBus.Publish(sender, new RefreshObjectEventArgs(toRemove));
            }

            //it wasn't a favourite anyway
        }

        public bool IsFavourite(IMapsDirectlyToDatabaseTable o)
        {
            return GetFavouriteIfAny(o) != null;
        }

        public Favourite GetFavouriteIfAny(IMapsDirectlyToDatabaseTable o)
        {
            return CurrentFavourites.SingleOrDefault(f => f.IsReferenceTo(o));
        }
    }
}
