using System.Collections.Generic;
using CatalogueLibrary.FilterImporting.Construction;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Revertable;
using Microsoft.SqlServer.Management.Smo;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;

namespace CatalogueLibrary.Data
{
    public interface IFilter : ISaveable, IMapsDirectlyToDatabaseTable,IDeleteable,IRevertable,ICollectSqlParameters, INamed
    {
        string WhereSQL { get; set; }
        string Description { get; set; }
        
        bool IsMandatory { get; set; }

        int? ClonedFromExtractionFilter_ID { get; set; }
        
        int? FilterContainer_ID { get; set; }
        
        [NoMappingToDatabase]
        IContainer FilterContainer { get;}

        ColumnInfo GetColumnInfoIfExists();
        IFilterFactory GetFilterFactory();
        Catalogue GetCatalogue();

        IQuerySyntaxHelper GetQuerySyntaxHelper();
    }
}
