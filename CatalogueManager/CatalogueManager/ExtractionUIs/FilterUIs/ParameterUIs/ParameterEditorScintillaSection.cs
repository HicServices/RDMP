using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.QueryBuilding;
using CatalogueManager.ExtractionUIs.FilterUIs.ParameterUIs.Options;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;
using ScintillaNET;

namespace CatalogueManager.ExtractionUIs.FilterUIs.ParameterUIs
{
    public class ParameterEditorScintillaSection
    {
        private readonly ParameterRefactorer _refactorer;
        private IQuerySyntaxHelper _querySyntaxHelper;

        public ParameterEditorScintillaSection( ParameterRefactorer refactorer, int lineStart, int lineEnd, ISqlParameter parameter, bool editable, string originalText)
        {
            _refactorer = refactorer;
            LineStart = lineStart;
            LineEnd = lineEnd;
            Parameter = parameter;
            Editable = editable;
            _querySyntaxHelper = parameter.GetQuerySyntaxHelper();

            var prototype = ConstantParameter.Parse(originalText, _querySyntaxHelper);
            if(prototype.Value != parameter.Value)
                throw new ArgumentException("Parameter " + parameter + " was inconsistent with the SQL passed to us based on QueryBuilder.DeconstructStringIntoParameter, they had different Values");

            if (prototype.ParameterSQL != parameter.ParameterSQL)
                throw new ArgumentException("Parameter " + parameter + " was inconsistent with the SQL passed to us based on QueryBuilder.DeconstructStringIntoParameter, they had different ParameterSQL");

            if (prototype.Comment != parameter.Comment)
                throw new ArgumentException("Parameter " + parameter + " was inconsistent with the SQL passed to us based on QueryBuilder.DeconstructStringIntoParameter, they had different Comment");

        }

        public int LineStart { get; private set; }
        public int LineEnd { get; private set; }

        public ISqlParameter Parameter { get; private set; }
        public bool Editable { get; private set; }

        public bool IncludesLine(int lineNumber)
        {
            return lineNumber >= LineStart && lineNumber <= LineEnd;
        }

        public FreeTextParameterChangeResult CheckForChanges(string sql)
        {
            try
            {
                string oldName = Parameter.ParameterName;
                
                ConstantParameter newPrototype;
                newPrototype = ConstantParameter.Parse(sql, _querySyntaxHelper);

                if (string.Equals(newPrototype.Comment, Parameter.Comment)//can be null you see
                    &&
                    string.Equals(newPrototype.Value, Parameter.Value)
                    &&
                    newPrototype.ParameterSQL.Equals(Parameter.ParameterSQL))
                    return FreeTextParameterChangeResult.NoChangeMade;
                
                Parameter.Comment = newPrototype.Comment;
                Parameter.Value = newPrototype.Value;
                Parameter.ParameterSQL = newPrototype.ParameterSQL;
                Parameter.SaveToDatabase();

                _refactorer.HandleRename(Parameter, oldName, Parameter.ParameterName);


                return FreeTextParameterChangeResult.ChangeAccepted;

            }
            catch (Exception)
            {
                return FreeTextParameterChangeResult.ChangeRejected;
            }
        }
    }

    public enum FreeTextParameterChangeResult
    {
        NoChangeMade,
        ChangeAccepted,
        ChangeRejected
    }
}
