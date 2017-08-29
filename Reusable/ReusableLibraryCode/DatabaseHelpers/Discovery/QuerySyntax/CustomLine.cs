using System;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax
{
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