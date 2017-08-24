using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;

namespace ReusableUIComponents
{
    /// <summary>
    /// Maintains lists of recently typed things into text boxes etc, use HostControl to have this class setup all the autocomplete and monitor .Leave events for self population
    /// Once you call HostControl then that is you done, this class does the rest.
    /// </summary>
    public class RecentHistoryOfControls
    {
        private static object oLockInstance = new object();
        private static RecentHistoryOfControls _instance;

        private const string DictionaryNameInRegistry = "RecentlyUsedValues";
        private Dictionary<string, List<string>> recentValues = new Dictionary<string, List<string>>();

        public static string RDMPRegistryRootKey;

        private RecentHistoryOfControls()
        {
            byte[] results = Registry.GetValue(RDMPRegistryRootKey, DictionaryNameInRegistry, null) as byte[];

            if(results != null)
                recentValues = Deserialize(new MemoryStream(results));
        }


        public void HostControl(TextBox c)
        {
           c.AutoCompleteCustomSource = GetListAsAutocomplete(c.Name);
           c.AutoCompleteSource  = AutoCompleteSource.CustomSource;
           c.AutoCompleteMode = AutoCompleteMode.Suggest;
           c.Leave += (sender, args) => AddResults(c, c.Text);
        }

        /// <summary>
        /// Must be called on the main Thread (UI Thread to prevent cross thread access exceptions), grabs control of the autocomplete functionality of the 
        /// control, populating it when .Leave events are encountered and setting up it's AutoCompleteCustomSource/ AutoCompleteSource
        /// </summary>
        /// <param name="c"></param>
        public void HostControl(ComboBox c)
        {
            c.AutoCompleteCustomSource = GetListAsAutocomplete(c.Name);

            if (c.DropDownStyle == ComboBoxStyle.DropDownList)
            {
                c.SelectedIndexChanged += (sender, args) => AddResults(c, c.Text);
            }
            else
            {
                c.AutoCompleteMode = AutoCompleteMode.Suggest;
                c.AutoCompleteSource = AutoCompleteSource.CustomSource;
                c.Leave += (sender, args) => AddResults(c, c.Text);
            }

            
        }

        #region binary serialization 

        private void Serialize(Dictionary<string, List<string>> dictionary, Stream stream)
        {
            BinaryWriter writer = new BinaryWriter(stream);

            writer.Write(dictionary.Count);
            foreach (var key in dictionary.Keys)
            {
                writer.Write(key);
                writer.Write(dictionary[key].Count);

                foreach (var value in dictionary[key])
                    writer.Write(value);
            }
            writer.Flush();
        }

        private Dictionary<string, List<string>> Deserialize(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            int countOfKeys = reader.ReadInt32();

            var dictionary = new Dictionary<string, List<string>>(countOfKeys);
            for (int k = 0; k < countOfKeys; k++)
            {
                var key = reader.ReadString();
                int countOfValues = reader.ReadInt32();

                dictionary.Add(key, new List<string>());

                for(int v = 0;v<countOfValues;v++)
                    dictionary[key].Add(reader.ReadString());
            }


            return dictionary;
        }


        #endregion


        public void AddResults(Control c, string value)
        {
            AddResults(c.Name,value);
        }

        /// <summary>
        /// Adds the given value to the collection of known values that have been previously used under the given key 'name'.  If alsoSave is true then the dictionary is serialized into
        /// the registry.  Returns true if values were successfully added to the registry (should match alsoSave bool).  The only time values fail writing to registry is when the registry
        /// size is exceeded (See TestOverflowPrevention - about 29,000 registry values cause overflow)
        /// </summary>
        /// <param name="name">Collection name - usually the .Name of the control</param>
        /// <param name="value">A new value that has been used with the control/collection that you want to remember from now on</param>
        /// <param name="alsoSave">true (default) to save to registry.  Use false if you are for some reason adding a large number of values</param>
        /// <returns></returns>
        public bool AddResults(string name, string value, bool alsoSave=true)
        {
            //add new control/collection if it isn't there already
            if (!recentValues.ContainsKey(name))
                recentValues.Add(name, new List<string>());

            //collection now exists

            //we don't want to add duplicates but we do want to bump newer values up to the top again so if it has it take it out
            if (recentValues[name].Contains(value))
                recentValues[name].Remove(value);

            //then add it to the head
            recentValues[name].Add(value);

            if(alsoSave)
                return Save();
            
            return false;
        }

        private bool Save()
        {
            MemoryStream s = new MemoryStream();
            Serialize(recentValues, s);
            s.Position = 0;
            try
            {
                Registry.SetValue(RDMPRegistryRootKey, DictionaryNameInRegistry, s.ToArray(), RegistryValueKind.Binary);
            }
            catch (IOException e)
            {
                if (e.Message.Contains("Insufficient system resources exist to complete the requested service")) //occurs when user adds 1,000,000 values to this dictionary
                    return false;

                throw;
            }

            return true;
        }


        public static RecentHistoryOfControls GetInstance()
        {
            lock (oLockInstance)
            {
                return _instance ?? (_instance = new RecentHistoryOfControls());
            }
        }

        public List<string> GetList(string key)
        {
            if(recentValues.ContainsKey(key))
                return recentValues[key];
            
            return null;
        }


        public AutoCompleteStringCollection GetListAsAutocomplete(string key)
        {
            List<string> knownValues = GetList(key);

            if (knownValues == null)
                return null;

            var vals = new AutoCompleteStringCollection();
            vals.AddRange(knownValues.ToArray());

            return vals;
        }

        public void Clear()
        {
            //reset it to empty 
            recentValues = new Dictionary<string, List<string>>();
            //save
            Save();
        }

        public void Clear(string key)
        {
            //clear the selected key only
            if(recentValues.ContainsKey(key))
            {
                //Remove it (same as)
                recentValues.Remove(key);
                Save();//Save to registry
            }
        }

        public void SetValueToMostRecentlySavedValue(TextBox c)
        {
            if (c.AutoCompleteCustomSource.Count > 0)
                c.Text = c.AutoCompleteCustomSource[c.AutoCompleteCustomSource.Count - 1]; //set the current text to the last used text
        }
        public void SetValueToMostRecentlySavedValue(ComboBox c)
        {
            if (c.AutoCompleteCustomSource.Count > 0)
                c.Text = c.AutoCompleteCustomSource[c.AutoCompleteCustomSource.Count - 1]; //set the current text to the last used text
        }

        public void AddHistoryAsItemsToComboBox(ComboBox c)
        {
            if (c.AutoCompleteCustomSource.Count > 0)
                foreach (string s in c.AutoCompleteCustomSource)
                    c.Items.Add(s);
        }
    }
}
