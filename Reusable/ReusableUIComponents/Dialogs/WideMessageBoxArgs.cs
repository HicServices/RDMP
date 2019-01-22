namespace ReusableUIComponents.Dialogs
{
    public class WideMessageBoxArgs
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string EnvironmentDotStackTrace { get; set; }
        public string KeywordNotToAdd { get; set; }
        public WideMessageBoxTheme Theme { get; set; }

        public WideMessageBoxArgs(string title, string message,string environmentDotStackTrace,string keywordNotToAdd,WideMessageBoxTheme theme)
        {
            Title = title;
            Message = message;
            EnvironmentDotStackTrace = environmentDotStackTrace;
            KeywordNotToAdd = keywordNotToAdd;
            Theme = theme;
        }
    }
}