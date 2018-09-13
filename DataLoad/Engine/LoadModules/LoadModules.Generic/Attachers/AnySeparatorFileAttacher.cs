using CatalogueLibrary.Data;
using ReusableLibraryCode.Checks;

namespace LoadModules.Generic.Attachers
{
    /// <summary>
    /// Data load component for loading flat files into RAW tables (flat files must be delimited by a specific character (or sequence) e.g. csv)
    /// 
    /// Allows you to load zero or more flat files which are delimited by a given character or sequence of characters.  For example comma separated (use Separator ',')
    /// or Tab separated (Use Separator '\t').
    /// </summary>
    public class AnySeparatorFileAttacher : DelimitedFlatFileAttacher
    {
        [DemandsInitialization(@"The file separator e.g. , for CSV.  For tabs type \t", Mandatory = true)]
        public string Separator
        {
            get { return _source.Separator; }
            set { _source.Separator = value; }
        }

        public AnySeparatorFileAttacher() : base('A')
        {
        }
        
        public override void Check(ICheckNotifier notifier)
        {
            base.Check(notifier);

            //Do not use IsNullOrWhitespace because \t is whitespace! worse than that user might get it into his head to set separator to \t\t or something else horrendous
            if (Separator == null)
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        "Separator has not been set yet, this is the character or sequence which seperates cells in your flat file.  For example in the case of a CSV (comma seperated values) file the Separator argument should be set to ','",
                        CheckResult.Fail));

        }
    }
}