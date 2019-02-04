using System;
using System.Collections.Generic;
using System.Data.Common;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Attributes;
using ReusableLibraryCode.Annotations;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// Persisted window layout of RDMPMainForm as Xml.  This can be used to reload RDMP to a given layout of windows and can be shared between users.
    /// </summary>
    public class WindowLayout: DatabaseEntity,INamed
    {
	    #region Database Properties

	    private string _name;
	    private string _layoutData;
	    #endregion

        /// <inheritdoc/>
        [NotNull]
        [Unique]
        public string Name
	    {
		    get { return _name;}
		    set { SetField(ref _name, value);}
	    }

        /// <summary>
        /// The Xml representation of the window layout being (e.g. what tabs are open, objects pinned etc)
        /// </summary>
	    public string LayoutData
	    {
		    get { return _layoutData;}
		    set { SetField(ref _layoutData, value);}
	    }

        /// <summary>
        /// Record the new layout in the database
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="name">Human readable name for the layout</param>
        /// <param name="layoutXml">The layout Xml of RDMPMainForm, use GetCurrentLayoutXml to get this, cannot be null</param>
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

        /// <inheritdoc/>
	    public WindowLayout(IRepository repository, DbDataReader r): base(repository, r)
	    {
		    Name = r["Name"].ToString();
		    LayoutData = r["LayoutData"].ToString();
	    }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }
    }
}
