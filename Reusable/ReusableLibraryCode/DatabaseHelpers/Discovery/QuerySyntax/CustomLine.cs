using System;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax
{
    /// <summary>
    /// An arbitrary string to be injected into an SQL query being built by an ISqlQueryBuilder.  This is needed to handle differences in Database Query Engine Implementations
    /// e.g. Top X is done as part of SELECT in Microsoft Sql Server (e.g. select top x * from bob) while in Oracle it is done as part of WHERE (e.g. select * from bob where ROWNUM
    ///  less than x) (See IQuerySyntaxHelper.HowDoWeAchieveTopX).
    /// 
    /// <para>Each CustomLine must have an QueryComponent of the Query that it relates to (LocationToInsert) and may have a CustomLineRole. </para>
    /// 
    /// <para>AggregateBuilder relies heavily on CustomLine because of the complexity of cross database platform GROUP BY (e.g. dynamic pivot with calendar table).  Basically converting
    /// the entire query into CustomLines and passing off implementation to the specific database engine (See IAggregateHelper.BuildAggregate).</para>
    /// </summary>
    public class CustomLine
    {
        public string Text { get; set; }
        public QueryComponent LocationToInsert { get; set; }

        public CustomLineRole Role { get; set; }
        
        /// <summary>
        /// The line of code that caused the CustomLine to be created, this can be a StackTrace passed into the constructor or calculated automatically by CustomLine 
        /// </summary>
        public string StackTrace { get; private set; }

        public CustomLine(string text, QueryComponent locationToInsert)
        {
            Text = string.IsNullOrWhiteSpace(text) ? text : text.Trim();
            LocationToInsert = locationToInsert;
            StackTrace = Environment.StackTrace;
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
