using System;
using System.Collections.Generic;
using BrightIdeasSoftware;
using CatalogueLibrary.Providers;

namespace MapsDirectlyToDatabaseTableUI
{
    public class TextMatchFilterWithWhiteList : TextMatchFilter
    {
        HashSet<object>  _whiteList = new HashSet<object>();
        private string[] _tokens;
        private CompositeAllFilter _compositeFilter;

        public TextMatchFilterWithWhiteList(IEnumerable<object> whiteList ,ObjectListView olv, string text, StringComparison comparison): base(olv, text, comparison)
        {
            if(!string.IsNullOrWhiteSpace(text) && text.Contains(" "))
            {
                List<IModelFilter> filters = new List<IModelFilter>();
                
                _tokens = text.Split(' ');
                foreach (string token in _tokens)
                    filters.Add(new TextMatchFilter(olv,token,comparison));

                _compositeFilter = new CompositeAllFilter(filters);
            }

            foreach (object o in whiteList)
                _whiteList.Add(o);
        }

        public override bool Filter(object modelObject)
        {
            //gets us the highlight and composite match if the user put in spaces
            bool showing = _compositeFilter != null ? _compositeFilter.Filter(modelObject) : base.Filter(modelObject);

            //if its in the whitelist show it
            if (_whiteList.Contains(modelObject))
                return true;

            return showing;
        }
    }
}