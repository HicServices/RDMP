using System;
using BrightIdeasSoftware;

namespace CatalogueManager.DataLoadUIs.LoadMetadataUIs
{
    public class AdvertisedProcessTaskFilter:IModelFilter
    {
        private readonly string _textFilter;
        private readonly bool _includeSearchingDescription;

        public AdvertisedProcessTaskFilter(string textFilter,bool includeSearchingDescription)
        {
            _textFilter = textFilter;
            _includeSearchingDescription = includeSearchingDescription;
        }

        public bool Filter(object modelObject)
        {
            if (string.IsNullOrWhiteSpace(_textFilter))
                return true;

            var task = (AdvertisedProcessTask) modelObject;

            if (task.ToString().ToLower().Contains(_textFilter.ToLower()))
                return true;

            if (_includeSearchingDescription && task.Description != null)
                if(task.Description.ToLower().Contains(_textFilter.ToLower()))
                    return true;

            return false;
        }
    }
}