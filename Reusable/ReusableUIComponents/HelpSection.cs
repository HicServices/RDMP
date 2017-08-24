namespace ReusableUIComponents
{
    internal class HelpSection
    {
        public string Keyword { get; set; }
        public string HelpText { get; set; }

        public HelpSection(string keyword, string helpText)
        {
            Keyword = keyword;
            HelpText = helpText;
        }
    }
}