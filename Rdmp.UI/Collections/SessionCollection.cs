using Rdmp.Core.Curation.Data.Dashboarding;
using System;
using System.Collections.Generic;

namespace Rdmp.UI.Collections
{
    /// <summary>
    /// Collection of objects grouped into a session
    /// </summary>
    public class SessionCollection :PersistableObjectCollection
    {
        public string SessionName { get; private set; }

        /// <summary>
        /// for persistence, do not use
        /// </summary>
        public SessionCollection()
        {
        }

        public SessionCollection(string name): this()
        {
            SessionName = name;
        }

        public override string SaveExtraText()
        {
            return Helper.SaveDictionaryToString(new Dictionary<string, string>() {{nameof(SessionName), SessionName}});
        }

        public override void LoadExtraText(string s)
        {
            SessionName = Helper.GetValueIfExistsFromPersistString(nameof(SessionName), s);
        }
    }

}
