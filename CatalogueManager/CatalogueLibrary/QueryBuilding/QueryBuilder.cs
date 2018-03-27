using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text.RegularExpressions;
using CatalogueLibrary.Checks;
using CatalogueLibrary.Checks.SyntaxChecking;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataHelper;
using CatalogueLibrary.QueryBuilding.Parameters;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;
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
    /// ExtractionInformation is sorted by column order prior to generating the SQL (i.e. not the order you add them to the query builder)
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
        
        public List<CustomLine> CustomLines { get; private set; }
        public CustomLine TopXCustomLine { get; set; }

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
        /// A container that contains all the subcontainers and filters to be assembled during the query (use a SpontaneouslyInventedFilterContainer if you want to inject your 
        /// own container tree at runtime rather than referencing a database entity)
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
        /// Only use this if you want IColumns which are marked as requiring Hashing to be hashed.  Once you set this on a QueryEditor all fields so marked will be hashed using the
        /// specified salt
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
            if(limitationSQL != null && limitationSQL.Contains("top"))
                throw new Exception("Use TopX property instead of limitation SQL to acheive this");

            LimitationSQL = limitationSQL;
            SQLOutOfDate = true;
        }

        public List<IFilter> Filters { get; private set; }

        public int TopX
        {
            get { return _topX; }
            set
            {
                //it already has that value
                if(_topX == value)
                    return;

                _topX = value;
                SQLOutOfDate = true;
            }
        }

        private int[] Filters_LineNumbers;
        private int Select_LineNumber;
        private int FROM_LineNumber;

        private int currentLine = 0;
        private string _sql;
        public bool SQLOutOfDate { get; set; }
        private IContainer _rootFilterContainer;
        private string _hashingAlgorithm;
        private int _topX;
        private IQuerySyntaxHelper _syntaxHelper;

        /// <summary>
        /// Used to build extraction queries based on ExtractionInformation sets
        /// </summary>
        /// <param name="limitationSQL">Any text you want after SELECT to limit the results e.g. "DISTINCT" or "TOP 10"</param>
        public QueryBuilder(string limitationSQL, string hashingAlgorithm)
        {
            SetLimitationSQL(limitationSQL);
            Sort = true;
            ParameterManager = new ParameterManager();
            CustomLines = new List<CustomLine>();

            CheckSyntax = true;
            SelectColumns = new List<QueryTimeColumn>();

            _hashingAlgorithm = hashingAlgorithm ?? "Work.dbo.HicHash({0},{1})";

            TopX = -1;
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
        /// Pass in a column and it will tell you which line of .SQL it wrote it out to.  Returns -1 if it is not found
        /// </summary>
        /// <param name="column"></param>
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
                    return QueryComponent.WHERE;
            
            if (iLineNumber == FROM_LineNumber)
                return QueryComponent.FROM;

            return QueryComponent.None;
        }

        public CustomLine AddCustomLine(string text, QueryComponent positionToInsert)
        {
            SQLOutOfDate = true;
            return SqlQueryBuilderHelper.AddCustomLine(this, text, positionToInsert);
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
                case QueryComponent.WHERE:
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
            string[] queryLines = SQL.Split(new []{Environment.NewLine},StringSplitOptions.None);


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
            var checkNotifier = new ThrowImmediatelyCheckNotifier();

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
                throw new Exception("There are no TablesUsedInQuery in this dataset");


            _syntaxHelper = SqlQueryBuilderHelper.GetSyntaxHelper(TablesUsedInQuery);

            if (TopX != -1)
                SqlQueryBuilderHelper.HandleTopX(this, _syntaxHelper, TopX);
            else
                SqlQueryBuilderHelper.ClearTopX(this);

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
                    parameter.Check(checkNotifier);

                int newlinesTaken;
                toReturn += GetParameterDeclarationSQL(parameter, out newlinesTaken);

                currentLine += newlinesTaken;
            }

            //add user custom Parameter lines
            toReturn = AppendCustomLines(toReturn, QueryComponent.VariableDeclaration);

            #endregion

            #region Select (including all IColumns)
            toReturn += TakeNewLine();
            Select_LineNumber = currentLine;
            toReturn += "SELECT " + LimitationSQL + TakeNewLine();

            toReturn = AppendCustomLines(toReturn, QueryComponent.SELECT);
            toReturn += TakeNewLine();

            toReturn = AppendCustomLines(toReturn, QueryComponent.QueryTimeColumn);
            
            for (int i = 0; i < SelectColumns.Count;i++ )
            {
                //output each of the ExtractionInformations that the user requested and record the line number for posterity
                UserSelectColumns_LineNumbers[i] = currentLine;

                string columnAsSql = SelectColumns[i].GetSelectSQL(_hashingAlgorithm, _salt, _syntaxHelper);

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
            toReturn = AppendCustomLines(toReturn, QueryComponent.JoinInfoJoin);
            
            #region Filters (WHERE)

            int[] filterLineNumbers;
            toReturn += SqlQueryBuilderHelper.GetWHERESQL(this, out filterLineNumbers);
            Filters_LineNumbers = filterLineNumbers;

            toReturn = AppendCustomLines(toReturn, QueryComponent.WHERE);
            toReturn = AppendCustomLines(toReturn, QueryComponent.Postfix);
            
            _sql = toReturn;
            SQLOutOfDate = false;

            LineCount = currentLine;
            #endregion
            
            
        }

        private string AppendCustomLines(string toReturn, QueryComponent stage)
        {
            var lines = SqlQueryBuilderHelper.GetCustomLinesSQLForStage(this, stage).ToArray();
            if (lines.Any())
            {
                toReturn += TakeNewLine();
                toReturn += string.Join(TakeNewLine(), lines.Select(l=>l.Text));
            }

            return toReturn;
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

        public static ConstantParameter DeconstructStringIntoParameter(string sql, IQuerySyntaxHelper syntaxHelper)
        {
            string[] lines = sql.Split(new[] {'\n'},StringSplitOptions.RemoveEmptyEntries);

            string comment = null;

            Regex commentRegex = new Regex(@"/\*(.*)\*/");
            var matchComment = commentRegex.Match(lines[0]);
            if (lines.Length >= 3 && matchComment.Success)
                comment = matchComment.Groups[1].Value;

            string declaration = comment == null ? lines[0]:lines[1];
            declaration = declaration.TrimEnd(new[] {'\r'});

            string valueLine = comment == null ? lines[1] : lines[2];

            if(!valueLine.StartsWith("SET"))
                throw new Exception("Value line did not start with SET:" + sql);

            var valueLineSplit = valueLine.Split(new[] {'='});
            var value = valueLineSplit[1].TrimEnd(new[] {';','\r'});


            return new ConstantParameter(declaration.Trim(), value.Trim(), comment, syntaxHelper);
        }

        public static string GetParameterDeclarationSQL(ISqlParameter sqlParameter, out int newlinesTaken)
        {
            string toReturn = "";

            if (!string.IsNullOrWhiteSpace(sqlParameter.Comment))
            {
                toReturn += "/*" + sqlParameter.Comment + "*/" + Environment.NewLine;
                newlinesTaken = 3;
            }
            else
                newlinesTaken = 2;//IMPORTANT, if you edit this to have more newlines, correct this value

            toReturn += sqlParameter.ParameterSQL + Environment.NewLine;

            //it's a table valued parameter! advanced
            if (!string.IsNullOrEmpty(sqlParameter.Value) && Regex.IsMatch(sqlParameter.Value, @"\binsert\s+into\b",RegexOptions.IgnoreCase))
                toReturn += sqlParameter.Value + ";" + Environment.NewLine;
            else
                toReturn += "SET " + sqlParameter.ParameterName + "=" + sqlParameter.Value + ";" + Environment.NewLine;//its a regular value
            
            
            

            return toReturn;
        }

        public void Invalidate()
        {
            SQLOutOfDate = true;
        }

    }
}
