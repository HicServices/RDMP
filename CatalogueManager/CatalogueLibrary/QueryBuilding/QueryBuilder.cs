using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text.RegularExpressions;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataHelper;
using CatalogueLibrary.QueryBuilding.Parameters;
using IFilter = CatalogueLibrary.Data.IFilter;

namespace CatalogueLibrary.QueryBuilding
{
    /// <summary>
    /// This class maintains a list of user defined ExtractionInformation objects.  It can produce SQL which will try to 
    /// extract this set of ExtractionInformation objects only from the database.  This includes determining which ExtractionInformation
    /// are Lookups, which tables the various objects come from, figuring out whether they can be joined by using JoinInfo in the catalogue
    /// 
    /// It will throw when query SQL if it is not possible to join all the underlying tables or there are any other problems.
    /// 
    /// You can ask it what is on line X or ask what line number has ExtractionInformation Y on it
    /// 
    /// ExtractionInformation is sorted by DefaultOrder prior to generating the SQL 
    /// </summary>
    public class QueryBuilder : ISqlQueryBuilder
    {
        object oSQLLock = new object();

        public string SQL
        {
            get {

                lock (oSQLLock)
                {
                    if (SQLOutOfDate)
                        RegenerateSQL();
                    return _sql;
                }
            }
        }

        public string LimitationSQL { get; private set; }
        
        public List<QueryTimeColumn> SelectColumns { get; private set; }
        private int[] UserSelectColumns_LineNumbers;
        
        public List<JoinInfo> JoinsUsedInQuery { get; private set; }
        private int[] JoinsUsedInQuery_LineNumbers;

        public List<TableInfo> TablesUsedInQuery { get; private set; }
        
        List<CustomLine> _customLines = new List<CustomLine>();

        public CustomLine[] CustomLines { get { return _customLines.ToArray(); } }

        public ParameterManager ParameterManager { get; private set; }
        
        /// <summary>
        /// Optional field, this specifies where to start gargantuan joins such as when there are 3+ joins and multiple primary key tables e.g. in a star schema.
        /// If this is not set and there are too many JoinInfos defined in the Catalogue then the class will bomb out with the Exception 
        /// </summary>
        public TableInfo PrimaryExtractionTable { get; set; }

        /// <summary>
        /// Determines whether the QueryBuilder will sort the input columns according to their .Order paramter, the default value is true
        /// </summary>
        public bool Sort { get; set; }

        public int CurrentLine
        {
            get { return currentLine;}
        }


        /// <summary>
        /// The number of lines in the SQL parameter
        /// </summary>
        public int LineCount { get; private set; }

        /// <summary>
        /// A container that contains all the subcontainers and filters to be assembled during the query (use PlaceholderContainer if you want only a single AND container with no subcontainers e.g. for testing)
        /// </summary>
        public IContainer RootFilterContainer
        {
            get { return _rootFilterContainer; }
            set {
                _rootFilterContainer = value;
                SQLOutOfDate = true;
            }
        }

        public bool CheckSyntax { get; set; }


        private string _salt = null;

        /// <summary>
        /// Only use this if you want IColumns which are marked as requiring Hashing to be hashed.  Once you set this on a QueryEditor all fields so marked will be hashed using the specified salt
        /// </summary>
        /// <param name="salt">A 3 letter string indicating the desired SALT</param>
        public void SetSalt(string salt)
        {
            if(string.IsNullOrWhiteSpace(salt))
                throw new NullReferenceException("Salt cannot be blank");

            _salt = salt;
        }

        public void SetLimitationSQL(string limitationSQL)
        {
            LimitationSQL = limitationSQL;
            SQLOutOfDate = true;
        }

        public List<IFilter> Filters { get; private set; }
        private int[] Filters_LineNumbers;
        private int Select_LineNumber;
        private int FROM_LineNumber;

        private int currentLine = 0;
        private string _sql;
        public bool SQLOutOfDate = false;
        private IContainer _rootFilterContainer;
        private string _hashingAlgorithm;

        


        /// <summary>
        /// Used to build extraction queries based on ExtractionInformation sets
        /// </summary>
        /// <param name="limitationSQL">Any text you want after SELECT to limit the results e.g. "DISTINCT" or "TOP 10"</param>
        public QueryBuilder(string limitationSQL, string hashingAlgorithm)
        {
            LimitationSQL = limitationSQL;
            Sort = true;
            ParameterManager = new ParameterManager();
            CheckSyntax = true;
            SelectColumns = new List<QueryTimeColumn>();

            _hashingAlgorithm = hashingAlgorithm ?? "Work.dbo.HicHash({0},{1})";
        }

        #region public stuff
        public void AddColumnRange(IColumn[] columnsToAdd)
        {
            //add the new ones to the list
            foreach (IColumn col in columnsToAdd)
                AddColumn(col);
                
            SQLOutOfDate = true;
        }

        public void AddColumn(IColumn col)
        {
            QueryTimeColumn toAdd = new QueryTimeColumn(col);

            //if it is new, add it to the list
            if (!SelectColumns.Contains(toAdd))
            {
                SelectColumns.Add(toAdd);
                SQLOutOfDate = true;
            }   
        }
        
        /// <summary>
        /// Add a custom line of code into the query at the specified position.  This will be maintained throughout the lifespan of the object such that if
        /// you add other columns etc then your code will still be included at the appropriate position.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="positionToInsert"></param>
        public void AddCustomLine(string text, QueryComponent positionToInsert)
        {
            CustomLine toAdd = new CustomLine();
            toAdd.Text = text;
            
            if(positionToInsert == QueryComponent.Filter)
                if (text.Trim().StartsWith("AND ") || text.Trim().StartsWith("OR "))
                    throw new Exception("Custom filters are always AND, you should not specify the operator AND/OR, you passed\"" + text + "\"");

            toAdd.LocationToInsert = positionToInsert;

            if(positionToInsert == QueryComponent.SELECT)
                throw new ArgumentException("Use QueryComponent " + QueryComponent.QueryTimeColumn + " instead for SELECT elements");

            _customLines.Add(toAdd);
            SQLOutOfDate = true;
        }

        /// <summary>
        /// Pass in a column and it will tell you which line of .SQL it wrote it out to.  Returns -1 if it is not found
        /// </summary>
        /// <param name="columnEntity"></param>
        /// <returns></returns>
        [Pure]
        public int GetLineNumberForColumn(IColumn column)
        {
            if (SQLOutOfDate)
                RegenerateSQL();

            for (int i = 0; i < UserSelectColumns_LineNumbers.Length; i++)
                if (SelectColumns[i].IColumn.ID == column.ID)
                    return UserSelectColumns_LineNumbers[i];

            return -1;
        }

        [Pure]
        public QueryComponent WhatIsOnLine(int iLineNumber)
        {
            if (SQLOutOfDate)
                RegenerateSQL();

            if (iLineNumber < Select_LineNumber)
                return QueryComponent.VariableDeclaration;

            if(iLineNumber == Select_LineNumber)
                return QueryComponent.SELECT;
            
            if (UserSelectColumns_LineNumbers.Any())
                if (UserSelectColumns_LineNumbers.Contains(iLineNumber))
                    return QueryComponent.QueryTimeColumn;

            if (JoinsUsedInQuery_LineNumbers.Any())
                if (JoinsUsedInQuery_LineNumbers.Contains(iLineNumber))
                    return QueryComponent.JoinInfoJoin;
            
            if (Filters_LineNumbers.Any())
                if (Filters_LineNumbers.Contains(iLineNumber))
                    return QueryComponent.Filter;
            
            if (iLineNumber == FROM_LineNumber)
                return QueryComponent.FROM;

            return QueryComponent.None;
        }

        [Pure]
        public object GetComponentOnLine(int iLineNumber)
        {

            if (SQLOutOfDate)
                RegenerateSQL();

            QueryComponent component = WhatIsOnLine(iLineNumber);

            switch (component)
            {
                case QueryComponent.QueryTimeColumn:
                    return SelectColumns[Array.IndexOf(UserSelectColumns_LineNumbers, iLineNumber)].IColumn;
                case QueryComponent.Filter:
                    return Filters[Array.IndexOf(Filters_LineNumbers, iLineNumber)];
                case QueryComponent.JoinInfoJoin:
                    return JoinsUsedInQuery[Array.IndexOf(JoinsUsedInQuery_LineNumbers, iLineNumber)];
                case QueryComponent.None:
                    return null;
                default:
                    throw new NotImplementedException();
            }
        }

        [Pure]
        public int GetLineNumberOfFirst(QueryComponent component)
        {
            if (SQLOutOfDate)
                RegenerateSQL();

            if (component == QueryComponent.FROM)
                return FROM_LineNumber;

            if (component == QueryComponent.SELECT)
                return Select_LineNumber;

            throw new NotImplementedException();

        }
        [Pure]
        public string GetSQLSubstringStartingAtLineNumber(int line)
        {
            if (SQLOutOfDate)
                RegenerateSQL();

            if(line > LineCount)
                throw new IndexOutOfRangeException("line must be less than the number of lines in the query");

            string toReturn = "";
            string[] queryLines = SQL.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);


            for (int i = line; i < queryLines.Length; i++)
                toReturn += queryLines[i];

            return toReturn;
        }

       
        [Pure]
        public JoinInfo[] GetRequiredJoins()
        {
            if (SQLOutOfDate)
                RegenerateSQL();

            return JoinsUsedInQuery.ToArray();
        }

        [Pure]
        public Lookup[] GetRequiredLookups()
        {
            if(SQLOutOfDate)
                RegenerateSQL();

            return SqlQueryBuilderHelper.GetRequiredLookups(this).ToArray();
        }

      
        [Pure]
        public int ColumnCount()
        {
            return SelectColumns.Count;
        }
        
        #endregion

        /// <summary>
        /// Updates .SQL Property, note that this is automatically called when you query .SQL anyway so you do not need to manually call it. 
        /// </summary>
        public void RegenerateSQL()
        {
            _sql = "";
            currentLine = 0;

            //reset the Parameter knowledge
            ParameterManager.ClearNonGlobals();

            #region Setup to output the query, where we figure out all the joins etc
            //reset everything
            //maintain a list of the line numbers that relevant things are on so that we can tell users
            UserSelectColumns_LineNumbers = new int[SelectColumns.Count];

            if (Sort)
                SelectColumns.Sort();
            
            //work out all the filters 
            Filters = SqlQueryBuilderHelper.GetAllFiltersUsedInContainerTreeRecursively(RootFilterContainer);
           
            TableInfo primary;
            TablesUsedInQuery = SqlQueryBuilderHelper.GetTablesUsedInQuery(this, out primary);
            this.PrimaryExtractionTable = primary;
            
            SqlQueryBuilderHelper.FindLookups(this);

            JoinsUsedInQuery = SqlQueryBuilderHelper.FindRequiredJoins(this);

            //deal with case when there are no tables in the query or there are only lookup descriptions in the query
            if (TablesUsedInQuery.Count == 0)
                    throw new Exception("There are no fields marked for extraction in this dataset");
     
            //declare parameters
            ParameterManager.AddParametersFor(Filters);
            
            #endregion

            /////////////////////////////////////////////Assemble Query///////////////////////////////

            #region Preamble (including variable declarations/initializations)
            //assemble the query - never use Environment.Newline, use TakeNewLine() so that QueryBuilder knows what line its got up to
            string toReturn = "";

            foreach (ISqlParameter parameter in ParameterManager.GetFinalResolvedParametersList())
            {
                if(CheckSyntax)
                    RDMPQuerySyntaxHelper.CheckSyntax(parameter);

                int newlinesTaken;
                toReturn += GetParameterDeclarationSQL(parameter, out newlinesTaken);

                currentLine += newlinesTaken;
            }

            //add user custom Parameter lines
            foreach (CustomLine line in _customLines.Where(c => c.LocationToInsert == QueryComponent.VariableDeclaration))
                toReturn += line.Text + TakeNewLine();
            #endregion

            #region Select (including all IColumns)
            toReturn += TakeNewLine();
            Select_LineNumber = currentLine;
            toReturn += "SELECT " + LimitationSQL + TakeNewLine();

            //add user custom SELECT lines
            foreach (CustomLine line in _customLines.Where(c => c.LocationToInsert == QueryComponent.QueryTimeColumn))
                toReturn += line.Text + TakeNewLine();
            
            for (int i = 0; i < SelectColumns.Count;i++ )
            {
                //output each of the ExtractionInformations that the user requested and record the line number for posterity
                UserSelectColumns_LineNumbers[i] = currentLine;

                 string columnAsSql = SelectColumns[i].GetSelectSQL(_hashingAlgorithm,_salt);

                 //there is another one coming
                 if (i + 1 < SelectColumns.Count)
                     columnAsSql += ",";
                
                toReturn += columnAsSql + TakeNewLine();
            }

            #endregion

            FROM_LineNumber = currentLine;


            //work out basic JOINS Sql
            int[] joinLineNumbers;
            toReturn += SqlQueryBuilderHelper.GetFROMSQL(this,out joinLineNumbers);
            JoinsUsedInQuery_LineNumbers = joinLineNumbers;

            //add user custom JOIN lines
            foreach (CustomLine line in _customLines.Where(c => c.LocationToInsert == QueryComponent.JoinInfoJoin))
                toReturn += line.Text + TakeNewLine();
            

            #region Filters (WHERE)

            int[] filterLineNumbers;
            toReturn += SqlQueryBuilderHelper.GetWHERESQL(this, out filterLineNumbers, false);
            Filters_LineNumbers = filterLineNumbers;
            
            #region Custom Filters (for people who can't be bothered to implement IFilter or when IContainer doesnt support ramming in additional Filters at runtime because you feel like it ) - these all get AND together and a WHERE is put at the start if needed
            //if there are custom lines being rammed into the Filter section
            if(_customLines.Any(c => c.LocationToInsert == QueryComponent.Filter))
                //if we haven't put a WHERE yet, put one in
                if (Filters.Count == 0)
                    toReturn += "WHERE" + TakeNewLine();
                else
                    toReturn += "AND" + TakeNewLine(); //otherwise just AND it with every other filter we currently have configured

            //add user custom Filter lines
            List<CustomLine> customFilterLines = new List<CustomLine>(_customLines.Where(c => c.LocationToInsert == QueryComponent.Filter));
            
            for (int i = 0; i < customFilterLines.Count(); i++)
            {
                toReturn += customFilterLines[i].Text + TakeNewLine();
                
                if(i+1<customFilterLines.Count())
                    toReturn += "AND" + TakeNewLine();
            }
            #endregion

            _sql = toReturn;
            SQLOutOfDate = false;

            LineCount = currentLine;
            #endregion
            
            
        }


       

        public string TakeNewLine()
        {
            currentLine++;
            return Environment.NewLine;
        }


        public IEnumerable<Lookup> GetDistinctRequiredLookups()
        {
            return SqlQueryBuilderHelper.GetDistinctRequiredLookups(this);
        }

        public static string GetParameterDeclarationSQL(ISqlParameter constantParameter)
        {
            int whoCares;
            return GetParameterDeclarationSQL(constantParameter, out whoCares);
        }

        public static ConstantParameter DeconstructStringIntoParameter(string sql)
        {
            string[] lines = sql.Split(new[] {'\n'});

            string comment = null;

            if (lines[0].StartsWith("--"))
                comment = lines[0].Substring(2).Trim();

            string declaration = comment == null ? lines[0]:lines[1];
            declaration = declaration.TrimEnd(new[] {'\r', ';'});

            string valueLine = comment == null ? lines[1] : lines[2];

            if(!valueLine.StartsWith("SET"))
                throw new Exception("Value line did not start with SET:" + sql);

            var valueLineSplit = valueLine.Split(new[] {'='});
            var value = valueLineSplit[1].TrimEnd(new[] {';','\r'});


            return new ConstantParameter(declaration.Trim(), value.Trim(), comment);
        }

        public static string GetParameterDeclarationSQL(ISqlParameter sqlParameter, out int newlinesTaken)
        {
            string toReturn = "";

            if (sqlParameter.Comment != null)
                toReturn += "--" + sqlParameter.Comment + Environment.NewLine;

            toReturn += sqlParameter.ParameterSQL + ";" + Environment.NewLine;

            //it's a table valued parameter! advanced
            if (!string.IsNullOrEmpty(sqlParameter.Value) && Regex.IsMatch(sqlParameter.Value, @"\binsert\s+into\b",RegexOptions.IgnoreCase))
                toReturn += sqlParameter.Value + ";" + Environment.NewLine;
            else
                toReturn += "SET " + sqlParameter.ParameterName + "=" + sqlParameter.Value + ";" + Environment.NewLine;//its a regular value
            
            //IMPORTANT, if you edit this to have more newlines, correct this value
            newlinesTaken = 3;

            return toReturn;
        }

        public void Invalidate()
        {
            SQLOutOfDate = true;
        }

    }
}
