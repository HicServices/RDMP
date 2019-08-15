using System;
using System.Collections.Generic;
using System.Text;
using Rdmp.Core.DataLoad.Triggers;

namespace Rdmp.Core
{
    public static class EnumExtensions
    {
        /// <summary>
        /// Returns a culture specific string for the <see cref="Enum"/>
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static string S(this Enum e)
        {
            if(e is TriggerStatus ts)
                switch (ts)
                {
                    case TriggerStatus.Enabled:
                        return GlobalStrings.Enabled;
                    case TriggerStatus.Disabled:
                        return GlobalStrings.Disabled;
                    case TriggerStatus.Missing:
                        return GlobalStrings.Missing;
                    default:
                        throw new ArgumentOutOfRangeException();
                }


            return e.ToString();
        }
    }
}
