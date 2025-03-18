using System.Collections.Generic;


namespace Rdmp.Core.Datasets.HDRItems
{
    /// <summary>
    /// 
    /// </summary>
    public class FormatAndStandards
    {
        public List<string> conformsTo { get; set; }
        public List<string> vocabularyEncodingScheme { get; set; }
        public List<string> language { get; set; }
        public List<string> format { get; set; }
    }
}
