using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Management.Smo;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ScintillaNET;

namespace CatalogueManager.PluginManagement.CodeGeneration
{
    public class MapsDirectlyToDatabaseTableClassCodeGenerator
    {
        private readonly DiscoveredTable _table;

        public MapsDirectlyToDatabaseTableClassCodeGenerator(DiscoveredTable table)
        {
            _table = table;
        }

        public string GetCode()
        {
            var columns = _table.DiscoverColumns();

            if (!columns.Any(c => c.GetRuntimeName().Equals("ID")))
                throw new CodeGenerationException("Table must have an ID automnum column to become an IMapsDirectlyToDatabaseTable class");
            
            StringBuilder classStart = new StringBuilder();

            classStart.Append("public class " + _table.GetRuntimeName() + ": DatabaseEntity");

            bool isINamed = columns.Any(c => c.GetRuntimeName() == "Name");
            if (isINamed)
                classStart.Append(",INamed");

            classStart.AppendLine();
            classStart.AppendLine("{");

            StringBuilder databaseFields = new StringBuilder();
            databaseFields.AppendLine("\t#region Database Properties");
            databaseFields.AppendLine();

            StringBuilder databaseProperties = new StringBuilder();

            StringBuilder constructors = new StringBuilder();

            constructors.AppendLine("\tpublic " + _table.GetRuntimeName() + "(IRepository repository/*, TODO Required Construction Properties For NEW*/)");
            constructors.AppendLine(@"  {
        repository.InsertAndHydrate(this,new Dictionary<string, object>()
        {
            //TODO Any parameters here as key value pairs
        });

        if (ID == 0 || Repository != repository)
            throw new ArgumentException(""Repository failed to properly hydrate this class"");
    }");


            constructors.AppendLine("\tpublic " + _table.GetRuntimeName() + "(IRepository repository, DbDataReader r): base(repository, r)");
            constructors.AppendLine("\t{");
            
            foreach (var col in columns.Where(c=>c.GetRuntimeName() != "ID"))
            {
                string setCode;
                var type = GetCSharpTypeFor(col,out setCode);
                var propertyName = col.GetRuntimeName();
                var fieldString = col.GetRuntimeName();
                
                //cammel case it
                fieldString = "_" + fieldString.Substring(0, 1).ToLower() + fieldString.Substring(1);

                databaseFields.AppendLine("\tprivate " + type + " " + fieldString + ";");

                databaseProperties.AppendLine("\tpublic " + type + " " + propertyName);
                databaseProperties.AppendLine("\t{");
                databaseProperties.AppendLine("\t\tget { return " + fieldString + ";}");
                databaseProperties.AppendLine("\t\tset { SetField(ref " + fieldString + ", value);}");
                databaseProperties.AppendLine("\t}");

                constructors.AppendLine("\t\t" + propertyName + " = "+ setCode);
            }

            databaseFields.AppendLine("\t#endregion");
            databaseFields.AppendLine();

            if (isINamed)
                constructors.AppendLine(@"\tpublic override string ToString()
        {
            return Name;
        }");


            constructors.AppendLine("\t}");

            return classStart.ToString() + databaseFields + databaseProperties + constructors + "}";

        }

        private string GetCSharpTypeFor(DiscoveredColumn col,out string setCode)
        {
            var r = "r[\"" + col.GetRuntimeName() + "\"]";

            if (col.DataType.GetLengthIfString() != -1)
            {
                if (col.AllowNulls)
                    setCode = r + " as string;";
                else
                    setCode = r + ".ToString();";

                return "string";
            }

            if (col.DataType.SQLType.Contains("date"))
                if (col.AllowNulls)
                {
                    setCode = "ObjectToNullableDateTime(" + r + ");";
                    return "DateTime?";
                }
                else
                {
                    setCode = "Convert.ToDateTime(" + r + ");";
                    return "DateTime";
                }

            if (col.DataType.SQLType.Contains("int"))
                if (col.AllowNulls)
                {
                    setCode = "ObjectToNullableInt(" + r + ");";
                    return "int?";
                }
                else
                {
                    setCode = "Convert.ToInt32(" + r + ");";
                    return "int";
                }
            
            if (col.DataType.SQLType.Contains("bit"))
                if (col.AllowNulls)
                {
                    setCode = "ObjectToNullableBool(" + r + ");//TODO: Confirm you actually mean true/false/null?";
                    return "bool?";
                }
                else
                {
                    setCode = "Convert.ToBoolean(" + r + ");";
                    return "bool";
                }
            

            setCode = "TODO Unrecognised Type";
            return "TODO  Unrecognised Type";
        }
    }
}

