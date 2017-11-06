using System;
using System.Collections.Generic;
using BrightIdeasSoftware;
using CatalogueLibrary.Providers;

namespace CatalogueManager.Collections.Providers
{
    public class TextMatchFilterWithWhiteList : TextMatchFilter
    {
        HashSet<object>  _whiteList = new HashSet<object>();
        private ICoreChildProvider _coreChildProvider;

        public TextMatchFilterWithWhiteList(IEnumerable<object> whiteList ,ObjectListView olv, string text, StringComparison comparison): base(olv, text, comparison)
        {
            foreach (object o in whiteList)
                _whiteList.Add(o);
        }
        
        public override bool Filter(object modelObject)
        {
            //gets us the highlight?
            bool showing = base.Filter(modelObject);

            //if its in the whitelist show it
            if (_whiteList.Contains(modelObject))
                return true;

            return showing;
        }
    }
}