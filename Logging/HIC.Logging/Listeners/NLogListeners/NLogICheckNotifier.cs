using HIC.Logging.Listeners.Extensions;
using NLog;
using ReusableLibraryCode.Checks;

namespace HIC.Logging.Listeners.NLogListeners
{
    public class NLogICheckNotifier : NLogListener,ICheckNotifier
    {
        public bool AcceptFixes { get; set; }

        public NLogICheckNotifier(bool acceptFixes, bool throwOnError): base(throwOnError)
        {
            AcceptFixes = acceptFixes;
        }
        public bool OnCheckPerformed(CheckEventArgs args)
        {
            var level = args.ToLogLevel();
            
            if (args.ProposedFix != null && AcceptFixes)
                //downgrade it to warning if we are accepting the fix
                if (level > LogLevel.Warn)
                    level = LogLevel.Warn;
            
            Log("Checks",level,args.Ex,args.Message);
            
            return AcceptFixes;
        }
    }
}
