using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;

namespace CatalogueManager.ExtractionUIs.FilterUIs.Options
{
    public abstract class FilterUIOptions
    {
        protected IFilter Filter;

        protected FilterUIOptions(IFilter filter)
        {
            Filter = filter;
        }

        public abstract TableInfo[] GetTableInfos();
        public abstract ISqlParameter[] GetGlobalParametersInFilterScope();
        public abstract IColumn[] GetIColumnsInFilterScope();
    }
}
