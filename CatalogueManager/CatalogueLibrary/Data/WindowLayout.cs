using System;
using System.Collections.Generic;
using System.Data.Common;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Data
{
    public class WindowLayout: DatabaseEntity,INamed
    {
	    #region Database Properties

	    private string _name;
	    private string _layoutData;
	    #endregion

	    public string Name
	    {
		    get { return _name;}
		    set { SetField(ref _name, value);}
	    }
	    public string LayoutData
	    {
		    get { return _layoutData;}
		    set { SetField(ref _layoutData, value);}
	    }
        public WindowLayout(IRepository repository, string name, string layoutXml)
        {
            repository.InsertAndHydrate(this,new Dictionary<string, object>()
            {
                {"Name",name},
                {"LayoutData",layoutXml}
            });

            if (ID == 0 || Repository != repository)
                throw new ArgumentException("Repository failed to properly hydrate this class");
        }
	    public WindowLayout(IRepository repository, DbDataReader r): base(repository, r)
	    {
		    Name = r["Name"].ToString();
		    LayoutData = r["LayoutData"].ToString();
	    }

        public override string ToString()
        {
            return Name;
        }
    }
}
